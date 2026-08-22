using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Contracts.Acompanhamento;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcompanhamentoController(AbrirAcompanhamentoUseCase abrirAcompanhamentoUseCase) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(AbrirAcompanhamentoResult), StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<AbrirAcompanhamentoResult>> Abrir(
            [FromBody] AbrirAcompanhamentoRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await abrirAcompanhamentoUseCase.AbrirAcompanhamento(dto.ParaRequest(), cancellationToken);

            return CreatedAtAction(nameof(Abrir), new { id = result.DossieId }, result);
        }
    }
}
