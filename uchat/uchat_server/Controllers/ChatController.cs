using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Models;
using uchat_server.Services;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost("create/chat/{sourceUserId}-{targetUserId}")]
    public IActionResult CreateChat(int sourceUserId, int targetUserId)
    {
        try
        {
            var newChatId = chatService.CreateChatAsync(sourceUserId, targetUserId);
            return Ok(newChatId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create/groupChat/{chat.name}")]
    public IActionResult CreateGroupChat([FromBody] GroupChat chat)
    {
        try
        {
            var newChatId = chatService.CreateGroupChatAsync(chat);
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

    [HttpGet("chat/{chatId}")]
    public IActionResult GetChatById(int chatId, [FromQuery] int userId) {
        try {
            return Ok(chatService.GetChatByIdAsync(chatId, userId));
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("groupChat/{chatId}")]
    public IActionResult GetGroupChatById(int chatId, [FromQuery] int userId) {
        try {
            return Ok(chatService.GetGroupChatByIdAsync(chatId, userId));
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
