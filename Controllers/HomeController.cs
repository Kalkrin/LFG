using Microsoft.AspNetCore.Mvc;

namespace LFG.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet()]
        public IActionResult Index()
        {
            return Ok("Hello!");
        }
    }
}
