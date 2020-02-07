using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            _config = config;
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegister userForRegister)
        {

            /*
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            */

            userForRegister.Username = userForRegister.Username.ToLower();

            var checkForUser = await _authRepository.UserAccountExist(userForRegister.Username);
            if (checkForUser)
                return BadRequest("Username already exists");

            var userCreated = new User
            {
                Username = userForRegister.Username
            };

            var createdUser = await _authRepository.Register(userCreated, userForRegister.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLogin userForLogin)
        {
           var userFromRepo = await _authRepository.Login(userForLogin.Username.ToLower(), userForLogin.Password);
           if (userFromRepo == null)
                return Unauthorized();
                    
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("Application:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDiscriptor = new SecurityTokenDescriptor() {
                Subject = new ClaimsIdentity(claims),
                 Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDiscriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
             
        }
    }
}
