using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Services;
using UpgradeOctoSystem.Api.Models;
using UpgradeOctoSystem.Api.Models.Auth;

namespace UpgradeOctoSystem.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IForgotPasswordProvider _forgotPasswordProvider;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUserProvider _userProvider;
        private readonly IMapper _mapper;

        public AuthController(IForgotPasswordProvider forgotPasswordProvider, ITokenProvider tokenProvider, IUserProvider userProvider, IMapper mapper)
        {
            _forgotPasswordProvider = forgotPasswordProvider;
            _tokenProvider = tokenProvider;
            _userProvider = userProvider;
            _mapper = mapper;
        }

        [HttpPost("Register")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest model)
        {
            if (model.Password != model.ConfirmPassword)
                throw new ValidationException(ExceptionMapping.Register_PasswordMisMatch);

            await _userProvider.RegisterUserAsync(model);

            return Ok();
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest model)
        {
            var result = await _userProvider.LoginUserAsync(model);
            var response = _mapper.Map<AuthResponse>(result);
            return Ok(response);
        }

        [HttpPost("ConfirmEmail")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest model)
        {
            await _userProvider.ConfirmEmailAsync(model.Id, model.Token);
            return Ok();
        }

        [HttpPost("ForgetPassword")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordRequest model)
        {
            await _forgotPasswordProvider.ForgetPasswordAsync(model.Email);
            return Ok();
        }

        [HttpPost("ResetPassword")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (model.Password != model.ConfirmPassword)
                return BadRequest("password no match");

            await _forgotPasswordProvider.ResetPasswordAsync(model);

            return Ok();
        }

        [HttpPost("RefreshToken")]
        [ProducesResponseType(200)]
        // TODO: fix this request body
        public async Task<ActionResult> RefreshToken([FromBody] AuthResponse refreshRequest)
        {
            var result = await _tokenProvider.RefreshTokenAsync(refreshRequest);
            var response = _mapper.Map<AuthResponse>(result);
            return Ok(response);
        }
    }
}