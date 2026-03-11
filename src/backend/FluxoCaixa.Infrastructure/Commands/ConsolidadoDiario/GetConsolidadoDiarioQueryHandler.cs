using AutoMapper;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FluxoCaixa.Infrastructure.Commands.ConsolidadoDiario;

public class GetConsolidadoDiarioQueryHandler : IRequestHandler<GetConsolidadoDiarioQuery, ConsolidadoDiarioDto?>
{
    private readonly IConsolidadoDiarioRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetConsolidadoDiarioQueryHandler> _logger;
    private readonly int _cacheTtlMinutes;

    public GetConsolidadoDiarioQueryHandler(
        IConsolidadoDiarioRepository repository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetConsolidadoDiarioQueryHandler> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
        _cacheTtlMinutes = configuration.GetValue("Redis:CacheTtlMinutes", 5);
    }

    public async Task<ConsolidadoDiarioDto?> Handle(GetConsolidadoDiarioQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"consolidado:{request.Data:yyyy-MM-dd}";

        var cached = await _cacheService.GetAsync<ConsolidadoDiarioDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Cache miss for {CacheKey}, querying database", cacheKey);

        var consolidado = await _repository.GetByDataAsync(request.Data);
        if (consolidado == null)
            return null;

        var dto = _mapper.Map<ConsolidadoDiarioDto>(consolidado);

        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(_cacheTtlMinutes));

        return dto;
    }
}
