using Microsoft.AspNetCore.Mvc;
using uchat_server.Services;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost("create")]
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

    [HttpDelete("{chatId}")]
    public IActionResult DeleteChat(int chatId) {
        try {
            return Ok(chatService.DeleteChatAsync(chatId));
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    // ADDED SECRET KEY PARAMETER
    [HttpGet("{chatId}")]
    public IActionResult GetChatById(int chatId, [FromQuery] int userId) {
        try {
            return Ok(chatService.GetChatByIdAsync(chatId, userId));
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}/chats")]
    public IActionResult GetUserChats(int userId) {
        try {
            return Ok(chatService.GetUserChatsAsync(userId));
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }
}
