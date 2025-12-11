using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using uchat_server.Services;
using uchat_server.Models;
using uchat_server.Hubs;
using uchat_server.Files;

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
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
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
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{chatId}/user/{userId}")]
    public async Task<IActionResult> DeleteChat(int chatId, int userId)
    {
        try
        {
            return await chatService.DeleteChatAsync(chatId, userId)
                ? Ok(new { Message = "Chat deleted successfully" })
                : NotFound(new { Message = "Chat doesn't exist" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("chat/{chatId}-{userId}")]
    public async Task<IActionResult> GetChatById(int chatId, int userId)
    {
        try
        {
            return Ok(await chatService.GetChatByIdAsync(chatId, userId));
        }
        catch (Exception e) when(e is (InvalidDataException or InvalidOperationException))
        {
            return NotFound(new { Error = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
        }
    }

    [HttpGet("groupChat/{chatId}-{userId}")]
    public async Task<IActionResult> GetGroupChatById(int chatId, int userId)
    {
        try
        {
            return Ok(await chatService.GetGroupChatByIdAsync(chatId, userId));
        }
        catch (Exception e) when(e is (InvalidDataException or InvalidOperationException))
        {
            return NotFound(new { Error = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
        }
    }

    [HttpGet("{userId}/chats")]
    public async Task<IActionResult> GetUserChats(int userId)
    {
        try
        {
            var test = await chatService.GetUserChatsAsync(userId);
            return Ok(new
            {
                Chats = test.Item1,
                GroupChats = test.Item2,
            });
        }
        catch (Exception e) when(e is (InvalidDataException or InvalidOperationException))
        {
            return NotFound(new { Error = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
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
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
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
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
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
    public async Task<IActionResult> UploadGroupChatAvatar(int chatId, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { Error = "No file uploaded." });
        }

        try
        {
            await chatService.UploadAvatarAsync(chatId, file);
            return Ok(new { Message = "Avatar uploaded successfully" });
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex) when(ex is (InvalidFileSizeException or InvalidFileFormatException)) {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("groupChat/avatar/{chatId}")]
    public async Task<IActionResult> RemoveGroupChatAvatar(int chatId)
    {
        try
        {
            return Ok(await chatService.RemoveGroupChatAvatarAsync(chatId));
        }
        catch (InvalidDataException e)
        {
            return NotFound(new { Error = e.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
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

    [HttpPut("{chatId}/info")]
    public async Task<IActionResult> UpdateGroupInfo(int chatId, [FromQuery] int userId, [FromBody] UpdateGroupChatRequest request)
    {
        try
        {
            await chatService.UpdateGroupChatAsync(chatId, userId, request);
            return Ok(new { Message = "Group info updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
