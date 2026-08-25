using APIFootballScout.Infrastructure.SofascoreExternalAdapter;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TournamentController(ISofascoreTournamentReader tournamentReader) : ControllerBase
    {
        [HttpGet("tournament")]
        public async Task<ActionResult<SofaTournamentFullDTO>> GetTournament([FromQuery] int tournamentId, [FromQuery] int? seasonId)
        {
            var tournament = await tournamentReader.GetTournamentDetailAsync(tournamentId, seasonId);
            if (tournament == null)
            {
                return NotFound($"tournament with ID {tournamentId} not found");
            }

            return Ok(tournament);
        }
    }
}
