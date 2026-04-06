using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InvoiceManagementSystem.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using InvoiceManagementSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Security;

namespace InvoiceManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var normalizedUsername = loginDto.Username.Trim().ToLowerInvariant();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == normalizedUsername);

            if (user == null || !PasswordHasher.VerifyPassword(loginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured.");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                username = user.Username,
                role = user.Role
            });
        }
    }
}
