using Api.FurnitureStore.Api.Configuration;
using Api.FurnitureStore.Share.auth;
using Api.FurnitureStore.Share.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.FurnitureStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthenticationController(UserManager<IdentityUser> userManager,
                                        IOptions<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var emailExist = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (emailExist != null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                  {
                    "Email already exist"
                  }
                });
            }

            //create user
            var user = new IdentityUser()
            {
                Email = request.EmailAddress,
                UserName = request.EmailAddress
            };

            var iscreated = await _userManager.CreateAsync(user,request.Password);
            if (iscreated.Succeeded)
            {
                var token = GenerateToken(user);
                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = token
                });
            }
            else
            {
                var errors = new List<string>();
                foreach (var err in iscreated.Errors)
                    errors.Add(err.Description);

                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = errors

                });
            }

            return BadRequest(new AuthResult
            {
                Result = false,
                Errors = new List<string> { "user couldn't be created" }

            });
        }

        [HttpPost("Login")]

        public async Task<IActionResult> Login([FromBody] UserloginRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest();

            //check if user exist 
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser == null)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid payload" },
                    Result = false
                });
            }

            var checkUserAndPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkUserAndPass)
            {
                return BadRequest(
                    new AuthResult
                    {
                        Errors = new List<string> { "Ivalid  Credentials" },
                        Result = false
                    });
            }

            var token = GenerateToken(existingUser);
            return Ok(new AuthResult { Token = token, Result = true });
        }
        private string GenerateToken(IdentityUser user)
        {
            var jwtTokenHanddler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(new[]
                {
                  new Claim("Id",user.Id),
                  new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                  new Claim(JwtRegisteredClaimNames.Email,user.Email),
                  new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

                })),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHanddler.CreateToken(tokenDescriptor);
            return jwtTokenHanddler.WriteToken(token);
        }
    }
}
