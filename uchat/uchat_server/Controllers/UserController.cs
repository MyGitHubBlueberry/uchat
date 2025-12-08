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

    [HttpGet("login")]
    public async Task<IActionResult> Login([FromQuery] LoginRequest request)
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

    [HttpGet("profile/{userId:int}")]
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

    [HttpPost("picture/{userId}")]
    public async Task<IActionResult> UploadProfilePicture(int userId, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) {
            return BadRequest("No file uploaded.");
        }

        try
        {
            await userService.UploadProfilePicture(userId, file);
            return Ok(new { Message = "Avatar updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpDelete("picture/{userId}")]
    public async Task<IActionResult> RemoveProfilePicture(int userId)
    {
        try {
            await userService.RemoveProfilePicture(userId);
            return Ok(new { Message = "User awatar was removed successfully" });
        } catch (InvalidOperationException ex) {
            return NotFound(ex.Message);
        } catch (Exception ex) {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPut("password/{userId:int}")]
    public async Task<IActionResult> UpdatePassword(int userId, [FromBody] UpdatePasswordRequest request)
    {
        try
        {
            await userService.UpdatePasswordAsync(userId, request);
            return Ok(new { Message = "Password updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> DeleteAccount(int userId)
    {
        try
        {
            await userService.DeleteUserAsync(userId);
            return Ok(new { Message = "Account deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
