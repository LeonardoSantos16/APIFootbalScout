using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Analise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnaliseController(ConsultarMetricasPor90UseCase consultarMetricasPor90UseCase) : ControllerBase
    {
        [HttpGet("{jogadorId:int:min(1)}/metricas-por-90")]
        [ProducesResponseType(typeof(MetricasPor90ResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<MetricasPor90ResponseDto>> ConsultarMetricasPor90(
            [FromRoute] int jogadorId,
            [FromQuery] ConsultarMetricasPor90RequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await consultarMetricasPor90UseCase.ConsultarMetricasPor90(
                (dto with { JogadorId = jogadorId }).ParaRequest(), cancellationToken);

            return Ok(result.ParaResponse());
        }
    }
}
