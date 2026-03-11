using FluxoCaixa.Application.Mappings;
using FluxoCaixa.Application.Services;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Domain.Notifications;
using FluxoCaixa.Domain.Services;
using FluxoCaixa.Infrastructure.Cache;
using FluxoCaixa.Infrastructure.Context;
using FluxoCaixa.Infrastructure.Kafka;
using FluxoCaixa.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluxoCaixa.IOC;

public static class DependencyInjection
{
    public static IServiceCollection AddFluxoCaixaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext - InMemory for development
        services.AddDbContext<FluxoCaixaDbContext>(options =>
            options.UseInMemoryDatabase("FluxoCaixaDb"));

        // Repositories
        services.AddScoped<ILancamentoRepository, LancamentoRepository>();
        services.AddScoped<IConsolidadoDiarioRepository, ConsolidadoDiarioRepository>();

        // Domain Services
        services.AddScoped<LancamentoDomainService>();

        // Application Services
        services.AddScoped<ILancamentoAppService, LancamentoAppService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Infrastructure.Commands.Lancamentos.CreateLancamentoCommand).Assembly));

        // Domain Notifications
        services.AddScoped<DomainNotificationHandler>();
        services.AddScoped<INotificationHandler<DomainNotification>>(sp => sp.GetRequiredService<DomainNotificationHandler>());

        // Cache
        services.AddScoped<ICacheService, RedisCacheService>();

        // Kafka
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        // Redis - with fallback to in-memory distributed cache
        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "FluxoCaixa:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddHostedService<KafkaConsumerService>();
        return services;
    }
}
