using Microsoft.AspNetCore.Mvc;
using uchat_server.Services;
using uchat_server.Models;

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

    [HttpPost("create/groupChat")]
    public IActionResult CreateGroupChat([FromBody] GroupChatCreateRequest chat)
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
    public async Task<IActionResult> GetUserChats(int userId) {
        try {
            var test = await chatService.GetUserChatsAsync(userId);
            return Ok(new {
                    Chats = test.Item1,
                    GroupChats = test.Item2,
                    });
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }
}
