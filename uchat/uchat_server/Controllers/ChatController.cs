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

    [HttpGet("chat/{chatId}-{userId}")]
    public IActionResult GetChatById(int chatId, int userId) {
        try {
            return Ok(chatService.GetChatByIdAsync(chatId, userId));
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("groupChat/{chatId}-{userId}")]
    public IActionResult GetGroupChatById(int chatId, int userId) {
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

    [HttpPost("{chatId}/members/{userId}")]
    public async Task<IActionResult> AddMember(int chatId, int userId)
    {
        try
        {
            await chatService.AddChatMemberAsync(chatId, userId);
            return Ok(new { Message = "Member added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
    
    [HttpDelete("{chatId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int chatId, int userId)
    {
        try
        {
            bool success = await chatService.RemoveChatMemberAsync(chatId, userId);
            if (success)
            {
                return Ok(new { Message = "Member removed successfully" });
            }
            else
            {
                return NotFound(new { Error = "Member not found in this chat" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("groupChat/avatar/{chatId}")]
    public async Task<IActionResult> UploadGroupChatAvatar(int chatId, [FromForm]IFormFile file) {
        try {
            await chatService.UploadAvatar(chatId, file);
            return Ok(new { Message = "Avatar uploaded successfully"});
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }
}
