using Confluent.Kafka;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FluxoCaixa.Infrastructure.Kafka;

public class KafkaConsumerService : BackgroundService, IKafkaConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _bootstrapServers;
    private readonly string _groupId;
    private readonly string _topic;

    public KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _groupId = configuration["Kafka:GroupId"] ?? "consolidado-group";
        _topic = configuration["Kafka:TopicLancamentoCriado"] ?? "lancamento-criado";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka consumer service starting. Topic: {Topic}, Group: {GroupId}", _topic, _groupId);

        await Task.Yield();

        await StartConsumingAsync(stoppingToken);
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        try
        {
            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", _topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cancellationToken);
                    if (result?.Message?.Value == null) continue;

                    _logger.LogInformation("Received message from topic {Topic}: {Message}", _topic, result.Message.Value);

                    var lancamentoDto = JsonConvert.DeserializeObject<LancamentoDto>(result.Message.Value);
                    if (lancamentoDto == null) continue;

                    await ProcessLancamentoAsync(lancamentoDto);

                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
            }

            consumer.Close();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer service stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka consumer encountered an error. Will not retry automatically.");
        }
    }

    private async Task ProcessLancamentoAsync(LancamentoDto lancamentoDto)
    {
        using var scope = _scopeFactory.CreateScope();
        var consolidadoRepo = scope.ServiceProvider.GetRequiredService<IConsolidadoDiarioRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var data = lancamentoDto.DataLancamento.Date;
        var consolidado = await consolidadoRepo.GetByDataAsync(data);

        if (consolidado == null)
        {
            consolidado = new ConsolidadoDiario(data);
            await consolidadoRepo.AddAsync(consolidado);
        }

        if (lancamentoDto.Tipo == 1) // Credito
            consolidado.AddCredito(lancamentoDto.Valor);
        else
            consolidado.AddDebito(lancamentoDto.Valor);

        await consolidadoRepo.SaveChangesAsync();

        var cacheKey = $"consolidado:{data:yyyy-MM-dd}";
        await cacheService.RemoveAsync(cacheKey);

        _logger.LogInformation("Consolidated balance updated for date {Data}. Credits: {Credits}, Debits: {Debits}, Balance: {Balance}",
            data.ToString("yyyy-MM-dd"), consolidado.TotalCreditos, consolidado.TotalDebitos, consolidado.Saldo);
    }
}
