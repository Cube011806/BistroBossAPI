using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginControllerAPI : ControllerBase
    {
        private readonly UserManager<Uzytkownik> _userManager;
        private readonly IConfiguration _configuration;

        public LoginControllerAPI(UserManager<Uzytkownik> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new
                {
                    error = "Błąd walidacji",
                    message = "Login i hasło są wymagane."
                });
            }

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var expireDate = model.RememberMe ? DateTime.UtcNow.AddYears(10) : DateTime.UtcNow.AddHours(3);

                var token = user.GenerateJwtToken(_configuration, expireDate);

                return Ok(new
                {
                    userId = user.Id,
                    token = token,
                    expiration = expireDate
                });
            }

            return Unauthorized(new
            {
                error = "Błąd logowania",
                message = "Nieprawidłowa nazwa użytkownika lub hasło."
            });
        }
    }

    public static class UzytkownikJwtExtension
    {
        public static string GenerateJwtToken(this Uzytkownik user, IConfiguration configuration, DateTime? expires = null)
        {
            if (expires == null)
            {
                expires = DateTime.UtcNow.AddHours(3);
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var keyBytes = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
            var authSigningKey = new SymmetricSecurityKey(keyBytes);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                expires: expires, 
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
