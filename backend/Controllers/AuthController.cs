using System.Security.Claims;
using JobTrail.DTOs;
using JobTrail.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTrail.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto registerRequest)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerRequest);
            return Ok(result);
        }
        catch (InvalidOperationException e)
        {   
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto  loginRequest)
    {
        try
        {
            var result = await _authService.LoginAsync(loginRequest);
            return Ok(result);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshRequestDto refreshRequest)
    {
        try
        {
            var result = await _authService.RefreshAsync(refreshRequest);
            return Ok(result);
        }
        catch (InvalidOperationException e)
        {
             return BadRequest(e.Message);
        }
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _authService.RevokeAsync(Guid.Parse(userId));
            return NoContent();
        }
        catch (InvalidOperationException e)
        {
             return BadRequest(e.Message);
        }
    }
}