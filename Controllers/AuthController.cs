using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Malia.Data;
using Malia.Models;
using Malia.Models.DTO;
using Malia.Services;

namespace Malia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        private readonly PasswordHasher<User> _hasher;

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
            _hasher = new PasswordHasher<User>();
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(x => x.UserName == dto.Username);

            if (exists)
                return BadRequest("Username already exists");

           

            var user = new User
            {
                
                FullName = dto.FullName,
                UserName = dto.Username,
                Role = UserRole.Citizen
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
           // employee.Password = _hasher.HashPassword(employee, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully",
                role = user.Role.ToString()
            });
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            //  var user = await _context.Users
            //    .FirstOrDefaultAsync(x => x.UserName == dto.Username);
            var user = await _context.Users
    .FirstOrDefaultAsync(x => x.UserName == dto.Username); 

            if (user == null)
                return BadRequest("Invalid username or password");

            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Invalid username or password");

            // JWT
            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                username = user.UserName,
               // username = user.Username,
                fullname = user.FullName,

                role = user.Role.ToString()
            });
        }

        // ================= TEST AUTH =================
        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            return Ok("Authorized 🔥");
        }
    }
}