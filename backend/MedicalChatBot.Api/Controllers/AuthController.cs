using MedicalChatBot.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalChatBot.Api.Data;
using MedicalChatBot.Api.Models;
using Microsoft.AspNetCore.Cors;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace MedicalChatBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowLocalhost")] // Optional, ensures CORS is allowed
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Register (Signup) endpoint
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists." });

            var user = new User
            {
                Username = dto.Username,
                Password = HashPassword(dto.Password) // 🚀 Save hashed password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        // Login endpoint
        [AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        return BadRequest(new { message = "Username and password are required." });

    var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
    if (user == null || !VerifyPassword(dto.Password, user.Password))
    {
        return Unauthorized(new { message = "Invalid credentials" });
    }

    return Ok(new { message = "Login successful" });
}
        // Hash password before saving (simple SHA256 for example)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Verify password when logging in
        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}
