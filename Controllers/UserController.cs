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
         
            /*TODO: IF response.success = false return bad request*/
            response = await _UserRepository.GetUserById(id);
           
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
        
            if(User.FindFirst(ClaimTypes.NameIdentifier).Value != id.ToString())
            {
                response.Success = false;
                response.Message = "provided ID does not belong to the current user";
                return BadRequest(response);    
            }
            /*TODO: IF response.success = false return bad request*/
            response = await _UserRepository.UpdateUser(id, updatedUser);
            
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            

            if(User.FindFirst(ClaimTypes.NameIdentifier).Value != id.ToString())
            {
                response.Success = false;
                response.Message = "provided ID does not belong to the current user";
                return BadRequest(response);    
            }
            /*TODO: IF response.success = false return bad request*/
            response = await _UserRepository.DeleteUser(id);
            
            if(!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}