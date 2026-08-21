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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AbrirAcompanhamentoResult>> Abrir(
            [FromBody] AbrirAcompanhamentoRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await abrirAcompanhamentoUseCase.AbrirAcompanhamento(dto.ParaRequest(), cancellationToken);

            return CreatedAtAction(nameof(Abrir), new { id = result.DossieId }, result);
        }
    }
}
