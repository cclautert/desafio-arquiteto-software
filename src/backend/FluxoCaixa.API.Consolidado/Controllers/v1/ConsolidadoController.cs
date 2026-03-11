using Asp.Versioning;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Infrastructure.Commands.ConsolidadoDiario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluxoCaixa.API.Consolidado.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/consolidado")]
[Authorize]
public class ConsolidadoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsolidadoController> _logger;

    public ConsolidadoController(IMediator mediator, ILogger<ConsolidadoController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{data:datetime}")]
    [ProducesResponseType(typeof(ConsolidadoDiarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDate(DateTime data)
    {
        var query = new GetConsolidadoDiarioQuery(data);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { Message = $"No consolidated data found for {data:yyyy-MM-dd}" });

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ConsolidadoDiarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetToday()
    {
        var query = new GetConsolidadoDiarioQuery(DateTime.UtcNow.Date);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { Message = "No consolidated data found for today." });

        return Ok(result);
    }
}
