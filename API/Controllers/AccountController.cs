using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService tokenService;

        public AccountController(ILogger<UsersController> logger, DataContext context, ITokenService tokenService)
            : base(logger, context)
        {
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);
            if (user == null)
            {
                return Unauthorized("invalid username or password");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("invalid username or password");
                }

            }

            UserDto userDto = new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };


            return userDto;

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> register(RegisterDto regUser)
        {
            if (await UserExist(regUser.UserName))
            {
                return BadRequest("Username already exist");
            }
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = regUser.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(regUser.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
                UserDto userDto = new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };


            return userDto;

        }

        private async Task<bool> UserExist(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName.ToLower());
        }
    }
}