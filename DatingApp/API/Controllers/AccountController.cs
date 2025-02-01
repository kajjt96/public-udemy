using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

public class AccountController(DataContext dataContext, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> RegisterAsync(RegisterDto registerDto)
    {
        if (await UserExistsAsync(registerDto.Username)) return BadRequest("Username is taken");
        
        using HMACSHA512 hmacsha512 = new HMACSHA512();

        AppUser appUser = new AppUser
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmacsha512.Key
        };

        dataContext.Users.Add(appUser);
        await dataContext.SaveChangesAsync();

        return new UserDto
        {
            Username = appUser.UserName,
            Token = tokenService.CreateToken(appUser)
        };
    }

    [HttpPost("login")] // api/account/login
    public async Task<ActionResult<UserDto>> LoginAsync(LoginDto loginDto)
    {
        AppUser? appUser = await dataContext.Users.FirstOrDefaultAsync(
            x => x.UserName == loginDto.Username.ToLower());

        if (appUser == null) return Unauthorized("Invalid username");

        using HMACSHA512 hmacsha512 = new(appUser.PasswordSalt);

        byte[] computedHash = hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != appUser.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserDto
        {
            Username = appUser.UserName,
            Token = tokenService.CreateToken(appUser)
        };
    }

    private async Task<bool> UserExistsAsync(string username)
    {
        return await dataContext.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
}
