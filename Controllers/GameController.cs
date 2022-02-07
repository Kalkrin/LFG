using System.Security.Claims;
using lfg.Data;
using lfg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lfg 
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly GameRepository _GameRepository;

        public GameController(GameRepository GameRepository)
        {
            _GameRepository = GameRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            //Console.WriteLine(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ServiceResponse<List<PublicGameDto>> response = await _GameRepository.GetAllGames();

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }  

        [HttpPost]
        public async Task<IActionResult> AddGame(AddGameDto gameToAdd)
        {
            ServiceResponse<Game> response = new ServiceResponse<Game>();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            response = await _GameRepository.CeateGame(gameToAdd, userId);

            if(response.Success)
                return Ok(response);
            else 
                return BadRequest(response);
        }
    }
}