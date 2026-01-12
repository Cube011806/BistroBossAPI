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

            // Sprawdzenie poprawności hasła
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var expireDate = model.RememberMe ? DateTime.UtcNow.AddYears(10) : DateTime.UtcNow.AddHours(3);

                var token = GenerateJwtToken(user, expireDate);

                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddHours(3)
                });
            }

            return Unauthorized(new
            {
                error = "Błąd logowania",
                message = "Nieprawidłowa nazwa użytkownika lub hasło."
            });
        }

        private string GenerateJwtToken(Uzytkownik user, DateTime expires)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var authSigningKey = new SymmetricSecurityKey(keyBytes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: expires, // Używamy przekazanej daty
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
