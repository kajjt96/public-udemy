using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    // [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsersAsync()
    {
        var users = await userRepository.GetMembersAsync();
     
        return Ok(users);
    }

    // [Authorize]    
    // [HttpGet("{id:int}")] // api/users/1
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUserAsync(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserAsync(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (username == null) return BadRequest("No username found in token");

        var user = await userRepository.GetUserByUsernameAsync(username);

        if (user == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);

        if (await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update the user");
    }
}
