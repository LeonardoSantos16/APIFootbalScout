using APIFootballScout.Application.UseCases.Interfaces;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController(ITournamentUseCase tournamentService) : ControllerBase
    {
        [HttpGet("tournament")]
        public async Task<ActionResult<SofaTournamentFullDTO>> GetTournament([FromQuery] int tournamentId, [FromQuery] int? seasonId)
        {
            var tournament = await tournamentService.GetTournamentDetailAsync(tournamentId, seasonId);
            if (tournament == null)
            {
                return NotFound($"tournament with ID {tournamentId} not found");
            }

            return Ok(tournament);
        }
    }
}
