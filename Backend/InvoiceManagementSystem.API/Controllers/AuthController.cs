using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InvoiceManagementSystem.BLL.Models;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceManagementSystem.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
       [HttpPost("login")]
     public IActionResult Login(LoginDto loginDto)
        {
            //  Dummy validation (for now)
            if (loginDto.Username != "admin" || loginDto.Password != "1234")
                return Unauthorized("Invalid username or password");

            //  Generate Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var role = "Admin"; // change based on user
            //var role = "FinanceUser";
             var userId = 1;
            var tokenDescriptor = new SecurityTokenDescriptor
            {

              Subject = new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.Name, loginDto.Username),

    //  ADD THESE
    new Claim("UserId", userId.ToString()),
    new Claim(ClaimTypes.Role, role)
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
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}