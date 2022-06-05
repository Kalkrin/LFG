using System.Security.Claims;
using lfg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lfg 
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class RequestController : ControllerBase
    {

        private readonly RequestRepository _RequestRepository;

        public RequestController(RequestRepository RequestRepository)
        {
            _RequestRepository = RequestRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            ServiceResponse<List<PublicRequestDto>> response = await _RequestRepository.GetAllRequests();

            return Ok(response);
        }

        [HttpGet("GetRequestById/{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            ServiceResponse<PublicRequestDto> response = await _RequestRepository.GetRequestById(id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("GetRequestsByUserId/{id}")]
        public async Task<IActionResult> GetRequestByUserId(int id)
        {
            ServiceResponse<List<PublicRequestDto>> response = await _RequestRepository.GetRequestsByUserId(id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpGet("GetRequestsForCreator/{id}")]
        public async Task<IActionResult> GetRequestForCreator(int id)
        {
            ServiceResponse<List<PublicRequestDto>> response = await _RequestRepository.GetRequestsForCreator(id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateRequestDto request)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ServiceResponse<PublicRequestDto> response = await _RequestRepository.CreateRequest(request, userId);

            return Ok(response);
        }

        [HttpPut("UpdateRequest/{id}")]
        public async Task<IActionResult> UpdateRequest(UpdateRequestDto updatedRequest, int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ServiceResponse<PublicRequestDto> response = await _RequestRepository.UpdateRequest(userId, updatedRequest, id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPut("ApproveOrDenyRequest/{id}")]
        public async Task<IActionResult> ApproveOrDenyRequest(ApproveOrDenyRequestDto updatedRequest, int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ServiceResponse<PublicRequestDto> response = await _RequestRepository.ApproveOrDenyRequest(userId, updatedRequest, id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpDelete("DeleteRequest/{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ServiceResponse<int> response = await _RequestRepository.DeleteRequest(userId, id);

            if(response.Success)
                return Ok(response);
            else
                return BadRequest(response);
        }
        
    }
}