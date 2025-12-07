using Microsoft.AspNetCore.Mvc;
using uchat_server.Services;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpGet("{chatId}/key")]
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

    [HttpPost("create")]
    public IActionResult CreateChat(string chatName)
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

    [HttpGet("exists/{chatId}")]
    public IActionResult ChatExists(int chatId) {
        try {
            return Ok(chatService.ChatExistsAsync(chatId));
        } catch (Exception ex) {
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

    [HttpGet("{chatId}")]
    public IActionResult GetChatById(int chatId) {
        try {
            return Ok(chatService.DeleteChatAsync(chatId));
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
