using Microsoft.AspNetCore.Mvc;

namespace Malia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("ValuesController is working");
        }
    }
}