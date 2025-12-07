using Microsoft.AspNetCore.Mvc;
using uchat_server.Services;

namespace uchat_server.Controllers;

[ApiController]
[Route("api")]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpGet("chat/key/{chatId}")]
    public IActionResult GetChatKey(int chatId)
    {
        try
        {
            var key = chatService.GetChatKeyAsync(chatId);

            return Ok(key);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("chat/create")]
    public IActionResult CreateChat([FromBody] string chatName)
    {
        try
        {
            var newChatId = chatService.CreateChatRoomAsync(chatName);
            return Ok(newChatId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
