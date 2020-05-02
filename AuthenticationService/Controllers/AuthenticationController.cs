using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthenticationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        private readonly ILogger<AuthenticationController> _logger;
        private readonly IOptions<AppSettingsModel> _settings;

        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            IOptions<AppSettingsModel> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        [HttpGet]
        public IActionResult GetToken(string userName, string password)
        {
            try
            {
                if (userName != "user" || password != "1234")
                    return BadRequest("Not found user");

                var tokenHandler = new JwtSecurityTokenHandler();
                var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.Value.SecretKey));
                var jwt = new JwtSecurityToken(
                    issuer: _settings.Value.Iss,
                    audience: _settings.Value.Aud,
                    expires: DateTime.UtcNow.AddDays(1),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var responseJson = new
                {
                    access_token = encodedJwt,
                    expires_in = (int)TimeSpan.FromDays(1).TotalSeconds
                };

                return Ok(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
