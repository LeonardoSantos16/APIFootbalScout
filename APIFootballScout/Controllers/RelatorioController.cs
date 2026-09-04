using APIFootballScout.Application.RelatorioScouting;
using APIFootballScout.Contracts.RelatorioScouting;
using APIFootballScout.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public class RelatorioController(
        ObterRelatorioUseCase obterRelatorioUseCase,
        AbrirRascunhoRelatorioUseCase abrirRascunhoRelatorioUseCase,
        EditarRascunhoRelatorioUseCase editarRascunhoRelatorioUseCase,
        FinalizarRelatorioUseCase finalizarRelatorioUseCase,
        CorrigirRelatorioUseCase corrigirRelatorioUseCase,
        ListarRelatoriosDoJogadorUseCase listarRelatoriosDoJogadorUseCase) : ControllerBase
    {
        private Guid OlheiroId => User.ObterUserId();

        [HttpGet("{relatorioId:guid}")]
        [ProducesResponseType<RelatorioResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<RelatorioResponseDto>> Obter(
            [FromRoute] Guid relatorioId, CancellationToken cancellationToken)
        {
            var result = await obterRelatorioUseCase.ObterRelatorio(
                new ObterRelatorioRequest(OlheiroId: OlheiroId, RelatorioId: relatorioId),
                cancellationToken);

            return Ok(result.ParaResponse());
        }

        [HttpGet("jogador/{jogadorId:int:min(1)}")]
        [ProducesResponseType<IReadOnlyList<RelatorioResponseDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<RelatorioResponseDto>>> Listar(
            [FromRoute] int jogadorId, CancellationToken cancellationToken)
        {
            var result = await listarRelatoriosDoJogadorUseCase.ListarRelatorios(
                new ListarRelatoriosDoJogadorRequest(OlheiroId: OlheiroId, JogadorId: jogadorId),
                cancellationToken);

            return Ok(result.ParaResponse());
        }

        [HttpPost]
        [ProducesResponseType<RelatorioResponseDto>(StatusCodes.Status201Created)]
        public async Task<ActionResult<RelatorioResponseDto>> Abrir(
            [FromBody] AbrirRascunhoRelatorioRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await abrirRascunhoRelatorioUseCase.AbrirRascunho(
                dto.ParaRequest(OlheiroId), cancellationToken);

            return CreatedAtAction(nameof(Obter), new { relatorioId = result.RelatorioId }, result.ParaResponse());
        }

        [HttpPut("{relatorioId:guid}")]
        [ProducesResponseType<RelatorioResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<RelatorioResponseDto>> Editar(
            [FromRoute] Guid relatorioId,
            [FromBody] EditarRascunhoRelatorioRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await editarRascunhoRelatorioUseCase.EditarRascunho(
                dto.ParaRequest(OlheiroId, relatorioId), cancellationToken);

            return Ok(result.ParaResponse());
        }

        [HttpPut("{relatorioId:guid}/finalizar")]
        [ProducesResponseType<RelatorioResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<RelatorioResponseDto>> Finalizar(
            [FromRoute] Guid relatorioId, CancellationToken cancellationToken)
        {
            var result = await finalizarRelatorioUseCase.FinalizarRelatorio(
                new FinalizarRelatorioRequest(OlheiroId: OlheiroId, RelatorioId: relatorioId),
                cancellationToken);

            return Ok(result.ParaResponse());
        }

        [HttpPost("{relatorioId:guid}/correcoes")]
        [ProducesResponseType<RelatorioResponseDto>(StatusCodes.Status201Created)]
        public async Task<ActionResult<RelatorioResponseDto>> Corrigir(
            [FromRoute] Guid relatorioId,
            [FromBody] CorrigirRelatorioRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await corrigirRelatorioUseCase.CorrigirRelatorio(
                dto.ParaRequest(OlheiroId, relatorioId), cancellationToken);

            return CreatedAtAction(nameof(Obter), new { relatorioId = result.RelatorioId }, result.ParaResponse());
        }
    }
}
