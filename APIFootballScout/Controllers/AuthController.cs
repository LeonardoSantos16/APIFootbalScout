using APIFootballScout.Application.User;
using APIFootballScout.Contracts.Auth;
using APIFootballScout.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        SignUpUserUseCase signUpUserUseCase,
        SignInUserUseCase signInUserUseCase,
        RefreshTokenUseCase refreshTokenUseCase,
        SignOutUserUseCase signOutUserUseCase,
        DeleteUserUseCase deleteUserUseCase) : ControllerBase
    {
        [HttpPost("signup")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> SignUp(
            [FromBody] SignUpRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await signUpUserUseCase.Execute(
                new SignUpUserRequest(dto.Name, dto.Email, dto.Password), cancellationToken);

            return Created(string.Empty, AuthResponseDto.De(result));
        }

        [HttpPost("signin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> SignIn(
            [FromBody] SignInRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await signInUserUseCase.Execute(dto.Email, dto.Password, cancellationToken);

            return Ok(AuthResponseDto.De(result));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            [FromBody] RefreshTokenRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await refreshTokenUseCase.Execute(dto.RefreshToken, cancellationToken);

            return Ok(AuthResponseDto.De(result));
        }

        [HttpPost("signout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SignOutSessao(
            [FromBody] RefreshTokenRequestDto dto,
            CancellationToken cancellationToken)
        {
            await signOutUserUseCase.Execute(User.ObterUserId(), dto.RefreshToken, cancellationToken);

            return NoContent();
        }

        [HttpPost("signout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SignOutTodasSessoes(CancellationToken cancellationToken)
        {
            await signOutUserUseCase.ExecuteTodasSessoes(User.ObterUserId(), cancellationToken);

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioAutenticadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public ActionResult<UsuarioAutenticadoDto> Me() =>
            Ok(new UsuarioAutenticadoDto(User.ObterUserId(), User.ObterEmail(), User.ObterRoles()));

        [HttpDelete("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
        {
            await deleteUserUseCase.Execute(User.ObterUserId(), cancellationToken);

            return NoContent();
        }
    }
}
