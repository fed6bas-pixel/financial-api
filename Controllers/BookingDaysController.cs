using Microsoft.AspNetCore.Mvc;

namespace Malia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingDaysController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("BookingDays Controller Working");
        }
    }
}