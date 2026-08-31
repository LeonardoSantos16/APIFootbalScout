using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AcompanhamentoController(AbrirAcompanhamentoUseCase abrirAcompanhamentoUseCase, ConsultarMudancaAcompanhamentoUseCase consultarAcompanhamentoUseCase) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(AbrirAcompanhamentoResult), StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<AbrirAcompanhamentoResult>> Abrir(
            [FromBody] AbrirAcompanhamentoRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await abrirAcompanhamentoUseCase.AbrirAcompanhamento(dto.ParaRequest(User.ObterUserId()), cancellationToken);

            return CreatedAtAction(nameof(Consultar), new { jogadorId = dto.JogadorId }, result);
        }

        [HttpGet("{jogadorId:int:min(1)}")]
        [ProducesResponseType(typeof(ConsultarMudancaAcompanhamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<ConsultarMudancaAcompanhamentoResponseDto>> Consultar(
            [FromRoute] ConsultarMudancaAcompanhamentoRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await consultarAcompanhamentoUseCase.ConsultarMudancaAcompanhamento(dto.ParaRequest(User.ObterUserId()), cancellationToken);

            return Ok(result.ParaResponse());
        }
    }
}
