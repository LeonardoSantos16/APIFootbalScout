using APIFootballScout.Application.ShortlistPersonalizada;
using APIFootballScout.Contracts.ShortlistPersonalizada;
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
    public class ShortlistController(AdicionarAlvoUseCase adicionarAlvoUseCase, CriarShortlistUseCase criarShortlistUseCase,
        ListarShortlistsDoOlheiroUseCase listarShortlistDoOlheiroUseCase, ObterShortlistUseCase obterShortlistUseCase,
        RemoverAlvoUseCase removerAlvoUseCase, RepriorizarAlvoUseCase repriorizarAlvoUseCase) : ControllerBase
    {
        private Guid OlheiroId => User.ObterUserId();

        [HttpPost]
        [ProducesResponseType<ShortlistResponseDto>(StatusCodes.Status201Created)]
        public async Task<ActionResult<ShortlistResponseDto>> Criar(
            [FromBody] CriarShortlistRequestDto dto, CancellationToken cancellationToken)
        {
            var shortlistCriada = await criarShortlistUseCase.CriarShortlist(
                dto.ParaRequest(OlheiroId), cancellationToken);

            return CreatedAtAction(
                nameof(Obter),
                new { shortlistId = shortlistCriada.ShortlistId },
                shortlistCriada.ParaResponse());
        }

        [HttpGet("{shortlistId:guid}")]
        [ProducesResponseType<ShortlistResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<ShortlistResponseDto>> Obter(
            [FromRoute] Guid shortlistId, CancellationToken cancellationToken)
        {
            var shortlist = await obterShortlistUseCase.ObterShortlist(
                ShortlistDtoMappers.ParaRequest(OlheiroId, shortlistId), cancellationToken);

            return Ok(shortlist.ParaResponse());
        }

        [HttpGet]
        [ProducesResponseType<IReadOnlyList<ShortlistResponseDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ShortlistResponseDto>>> Listar(
            CancellationToken cancellationToken)
        {
            var shortlists = await listarShortlistDoOlheiroUseCase.ListarShortlists(
                ShortlistDtoMappers.ParaRequest(OlheiroId), cancellationToken);

            return Ok(shortlists.ParaResponse());
        }

        [HttpPost("{shortlistId:guid}/alvos")]
        [ProducesResponseType<ShortlistResponseDto>(StatusCodes.Status201Created)]
        public async Task<ActionResult<ShortlistResponseDto>> AdicionarAlvo(
            [FromRoute] Guid shortlistId,
            [FromBody] AdicionarAlvoRequestDto dto,
            [FromHeader(Name = "If-Match")] long expectedVersion,
            CancellationToken cancellationToken)
        {
            var shortlist = await adicionarAlvoUseCase.AdicionarAlvo(
                dto.ParaRequest(OlheiroId, shortlistId, expectedVersion), cancellationToken);

            return CreatedAtAction(
                nameof(Obter),
                new { shortlistId = shortlist.ShortlistId },
                shortlist.ParaResponse());
        }

        [HttpPut("{shortlistId:guid}/alvos/{jogadorId:int:min(1)}")]
        [ProducesResponseType<ShortlistResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<ShortlistResponseDto>> RepriorizarAlvo(
            [FromRoute] Guid shortlistId,
            [FromRoute] int jogadorId,
            [FromBody] RepriorizarAlvoRequestDto dto,
            [FromHeader(Name = "If-Match")] long expectedVersion,
            CancellationToken cancellationToken)
        {
            var shortlist = await repriorizarAlvoUseCase.RepriorizarAlvo(
                dto.ParaRequest(OlheiroId, shortlistId, jogadorId, expectedVersion), cancellationToken);

            return Ok(shortlist.ParaResponse());
        }

        [HttpDelete("{shortlistId:guid}/alvos/{jogadorId:int:min(1)}")]
        [ProducesResponseType<ShortlistResponseDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<ShortlistResponseDto>> DeletarAlvo(
            [FromRoute] Guid shortlistId,
            [FromRoute] int jogadorId,
            [FromHeader(Name = "If-Match")] long expectedVersion,
            CancellationToken cancellationToken)
        {
            var shortlist = await removerAlvoUseCase.RemoverAlvo(
                ShortlistDtoMappers.ParaRequest(OlheiroId, shortlistId, jogadorId, expectedVersion), cancellationToken);

            return Ok(shortlist.ParaResponse());
        }
    }
}