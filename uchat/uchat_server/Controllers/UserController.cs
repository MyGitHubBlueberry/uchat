using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;

namespace uchat_server.Controllers;

[ApiController]
[Route("api")]
public class UserController(FakeDatabase db) : ControllerBase
{
    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        db.CreateUser(user);
        
        return Ok(new { Status = "Create", MessageId = user.Id });
    }

    [HttpGet("users")]
    public async Task<List<User>> GetUsers()
    {
        return db.GetUsers();
    }
}