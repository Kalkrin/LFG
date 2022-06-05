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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetGameById(int id)
        {
            ServiceResponse<Game> response = new ServiceResponse<Game>();

            response = await _GameRepository.GetGameById(id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddGame(AddGameDto gameToAdd)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            response = await _GameRepository.CeateGame(gameToAdd, userId);

            if(response.Success)
                return Ok(response);
            else 
                return BadRequest(response);
        }

        [HttpPut("UpdateGame/{id}")]
        public async Task<IActionResult> UpdateGame([FromBody] UpdateGameDto updatedGame, int id)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            updatedGame.Id = id;

            response = await _GameRepository.UpdateGame(updatedGame, userId);

            if(response.Success)
                return Ok(response);
            else 
                return BadRequest(response);
        }

        [HttpDelete("DeleteGame/{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            response = await _GameRepository.DeleteGame(id, userId);

            if(response.Success)
                return Ok(response);
            else 
                return BadRequest(response);
        }
    }
}