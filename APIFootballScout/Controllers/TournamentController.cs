using APIFootballScout.Models.DTOs.Sofascore.Tournament;
using APIFootballScout.Services.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController(ITournamentService tournamentService) : ControllerBase
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
