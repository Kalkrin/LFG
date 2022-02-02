using System.Security.Claims;
using lfg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lfg
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _UserRepository;

        public UserController(UserRepository UserRepository)
        {
            _UserRepository = UserRepository;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetUsers(){
            //Console.WriteLine(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ServiceResponse<List<PublicUserDto>> response = await _UserRepository.GetAllUsers();

            return Ok(response);
        }  

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetUserById(int id){
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
                 response = await _UserRepository.GetUserById(id);
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message.ToString());
                response.Success = false;
                response.Message = "No users found with the provided id";
                return BadRequest(response);
            }

            if(response.Data == null)
            {
                response.Success = false;
                response.Message = "No users found with the provided id";
                return BadRequest(response);
            }   

            return Ok(response);
        }
    }
}