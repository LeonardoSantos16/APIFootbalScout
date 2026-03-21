using APIFootballScout.Models.DTOs.Sofascore.player;
using APIFootballScout.Services.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController(IPlayerService playerService) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<ActionResult<SofaSearchResponse>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Name is required");

            var result = await playerService.SearchPlayersAsync(q);

            return result;
        }

        [HttpGet("{id:int}/profile")]
        public async Task<ActionResult<PlayerFullProfileDto>> GetProfile (int id)
        {
            var profile = await playerService.GetPlayerProfileAsync(id);
            Console.WriteLine($"profileprofile");
            if (profile.Details == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(profile);
        }
    }
}
