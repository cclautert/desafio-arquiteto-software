using Asp.Versioning;
using FluxoCaixa.Application.DTOs;
using FluxoCaixa.Infrastructure.Commands.Lancamentos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluxoCaixa.API.Lancamentos.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lancamentos")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LancamentosController> _logger;

    public LancamentosController(IMediator mediator, ILogger<LancamentosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLancamentoDto dto)
    {
        var command = new CreateLancamentoCommand(dto.Descricao, dto.Valor, dto.Tipo, dto.DataLancamento);

        try
        {
            var result = await _mediator.Send(command);
            _logger.LogInformation("Lancamento created with ID {Id}", result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LancamentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllLancamentosQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetLancamentoByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { Message = $"Lancamento with ID {id} not found." });

        return Ok(result);
    }
}
