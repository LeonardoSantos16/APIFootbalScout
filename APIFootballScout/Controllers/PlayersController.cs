using APIFootballScout.Infrastructure.SofascoreExternalAdapter;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayersController(ISofascorePlayerReader sofascorePlayerReader) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<ActionResult<SofaSearchResponse>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Name is required");

            var result = await sofascorePlayerReader.SearchPlayersAsync(q);

            return result;
        }

        [HttpGet("{id:int}/profile")]
        public async Task<ActionResult<PlayerFullProfileDto>> GetProfile (int id)
        {
            var profile = await sofascorePlayerReader.GetPlayerProfileAsync(id);
            Console.WriteLine($"profileprofile");
            if (profile.Details == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(profile);
        }
    }
}
