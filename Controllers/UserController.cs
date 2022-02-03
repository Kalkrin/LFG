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
        public async Task<IActionResult> GetUsers()
        {
            //Console.WriteLine(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            ServiceResponse<List<PublicUserDto>> response = await _UserRepository.GetAllUsers();

            return Ok(response);
        }  

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
                 response = await _UserRepository.GetUserById(id);
            }
            catch(Exception e) 
            {
                response.Success = false;
                response.Message = e.Message.ToString();
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

        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updatedUser, int id)
        {
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
                if(id != updatedUser.Id)
                {
                    response.Success = false;
                    response.Message = "ID of User provided does not match the ID of the user to be updated";
                    return BadRequest(response);
                }

                response = await _UserRepository.UpdateUser(updatedUser);
                
                if(!response.Success)
                    return BadRequest(response);

            }
            catch(Exception e)
            {
                response.Success = false;
                response.Message = e.Message.ToString();
            }

            return Ok(response);
        }
    }
}