using lfg.Models;
using Microsoft.AspNetCore.Mvc;

namespace lfg
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _UserRepository;

        public AuthController(UserRepository UserRepository)
        {
            _UserRepository = UserRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserAuthDto request)
        {
            ServiceResponse<int> response = await _UserRepository.Register(
                new User { Username = request.Username, Email = request.Email, FirstName = request.FirstName, LastName = request.LastName }, request.Password
            );
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            ServiceResponse<string> response = await _UserRepository.Login(
                request.Username, request.Password
            );

            if (!response.Success)
                return BadRequest(response);
            else
                return Ok(response);
        }
    }
}