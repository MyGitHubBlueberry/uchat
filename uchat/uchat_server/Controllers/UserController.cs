using Microsoft.AspNetCore.Mvc;
using uchat_server.Services;
using uchat_server.Models;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var response = await userService.RegisterAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await userService.LoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        try
        {
            var user = await userService.GetUserByIdAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("search/{part}")]
    public async Task<IActionResult> SearchUsers(string part)
    {
        try
        {
            var users = await userService.GetUserByNameAsync(part);
            return Ok(users);
        }
        catch (Exception ex)
        {
            // Even if empty, returning an empty list is better than error for search
            return Ok(new List<SharedLibrary.Models.User>());
        }
    }
}
