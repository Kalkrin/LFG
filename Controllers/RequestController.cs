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
            ServiceResponse<List<Request>> response = await _RequestRepository.GetAllRequests();

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(CreateRequestDto request)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            ServiceResponse<Request> response = await _RequestRepository.CreateRequest(request, userId);

            return Ok(response);
        }
    }
}