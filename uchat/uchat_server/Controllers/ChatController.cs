using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using uchat_server.Services;
using uchat_server.Models;
using uchat_server.Hubs;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IChatService chatService, IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpPost("create/chat/{sourceUserId}-{targetUserId}")]
    public async Task<IActionResult> CreateChat(int sourceUserId, int targetUserId)
    {
        try
        {
            var newChatId = await chatService.CreateChatAsync(sourceUserId, targetUserId);
            
            var chat = await chatService.GetChatByIdAsync(newChatId, sourceUserId);
            var chatForTarget = await chatService.GetChatByIdAsync(newChatId, targetUserId);
            
            await hubContext.Clients.Group($"user_{sourceUserId}").SendAsync("NewChat", chat);
            await hubContext.Clients.Group($"user_{targetUserId}").SendAsync("NewChat", chatForTarget);
            
            return Ok(newChatId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create/groupChat")]
    public async Task<IActionResult> CreateGroupChat([FromBody] GroupChatCreateRequest chat)
    {
        try
        {
            var newChatId = await chatService.CreateGroupChatAsync(chat);
            
            var groupChatsForMembers = await chatService.GetGroupChatForAllMembersAsync(newChatId);
            
            foreach (var kvp in groupChatsForMembers)
            {
                await hubContext.Clients.Group($"user_{kvp.Key}").SendAsync("NewGroupChat", kvp.Value);
            }
            
            return Ok(newChatId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId, [FromQuery] int userId) 
    {
        try 
        {
            bool deleted = await chatService.DeleteChatAsync(chatId, userId);
            
            if (deleted) return Ok(new { Message = "Chat deleted successfully" });
            return NotFound("Chat not found");
        } 
        catch (Exception ex) 
        {
            return BadRequest(new { Error = ex.Message });
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

    [HttpGet("{chatId}/members")]
    public async Task<IActionResult> GetChatMembers(int chatId)
    {
        try
        {
            var members = await chatService.GetChatMembersAsync(chatId);
            return Ok(members);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
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
        if (file is null || file.Length == 0) {
            return BadRequest("No file uploaded.");
        }

        try {
            await chatService.UploadAvatarAsync(chatId, file);
            return Ok(new { Message = "Avatar uploaded successfully"});
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        } catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("groupChat/avatar/{chatId}")]
    public async Task<IActionResult> RemoveGroupChatAvatar(int chatId) {
        try {
            return Ok(await chatService.RemoveGroupChatAvatarAsync(chatId));
        } catch (InvalidOperationException ex) {
            return NotFound(ex.Message);
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        } catch (Exception ex) {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("{chatId}/isMember/{userId}")]
    public async Task<IActionResult> CheckIsMember(int chatId, int userId)
    {
        try
        {
            bool isMember = await chatService.IsMemberOfChatAsync(chatId, userId);
            return Ok(isMember);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{chatId}/isOwner/{userId}")]
    public async Task<IActionResult> CheckIsOwner(int chatId, int userId)
    {
        try
        {
            bool isOwner = await chatService.IsOwnerOfChatAsync(chatId, userId);
            return Ok(isOwner);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
