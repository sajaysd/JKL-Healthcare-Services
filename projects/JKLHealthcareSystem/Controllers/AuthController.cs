using JKLHealthcareSystem.Data;
using JKLHealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace JKLHealthcareSystem.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Render the Login View
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        // Render the Register View
        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View("~/Views/Auth/Register.cshtml");
        }

        // Handle Login API
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and Password cannot be empty.");
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username.");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            if (Convert.ToBase64String(computedHash) != user.PasswordHash)
            {
                return Unauthorized("Invalid password.");
            }

            // Set session after login
            HttpContext.Session.SetString("isLoggedIn", "true");

            return Ok(new { Message = "Login successful!" });
        }

        // Handle Register API
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and Password cannot be empty.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Username = request.Username,
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password))),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully!");
        }
    }

    // DTO for Login and Register
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
