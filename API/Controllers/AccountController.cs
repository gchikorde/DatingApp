using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public  async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if( await UserExists(registerDto.Username)) return BadRequest("User Name already taken");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName=registerDto.Username.ToLower(),
                PasswordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt=hmac.Key
            };
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();
            return new UserDto
            {
                Username=user.UserName,
                Token= tokenService.CreateToken(user)
            };
        }

         [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.context.Users.
                SingleOrDefaultAsync(u => u.UserName == loginDto.Username);
            if(user == null) return Unauthorized("Invalid User Name.");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            
            for(int i=0;i<computedHash.Length;i++)
            {
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                Username=user.UserName,
                Token= tokenService.CreateToken(user)
            };

        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(u => u.UserName == username.ToLower());
        }
    }
}