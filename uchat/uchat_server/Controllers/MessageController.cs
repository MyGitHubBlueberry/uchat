using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Models;
using uchat_server.Files;
using uchat_server.Hubs;
using uchat_server.Services;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/message")]
public class MessageController(IHubContext<ChatHub> hubContext, IMessageService messageService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] string? messageJson, [FromForm] List<IFormFile>? files)
    {
        Message? msg = null;

        if (!string.IsNullOrEmpty(messageJson))
        {
            msg = System.Text.Json.JsonSerializer.Deserialize<Message>(messageJson);
        }

        if (msg == null) return BadRequest(new { Error = "Invalid message format" });

        try
        {
            await messageService.SaveMessageAsync(msg, files);
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

        await hubContext.Clients.Group(msg.ChatId.ToString())
            .SendAsync("ReceiveMessage", msg);

        Console.WriteLine("Message sent");

        return Ok(new { Message = "Sent", MessageId = msg.Id });
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetMessages(int chatId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        return Ok(await messageService.GetChatMessagesDtoAsync(chatId, pageNumber, pageSize));
    }

    [HttpPatch("text/{messageId}")]
    public async Task<IActionResult> ChangeMessageText(int messageId, string text)
    {
        try
        {
            await messageService.ChangeMessageTextAsync(messageId, text);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        return Ok(new { Message = "Changed" });
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        try
        {
            int chatId = await messageService.DeleteMessageAsync(messageId);
            await hubContext.Clients.Group(chatId.ToString())
                .SendAsync("MessageDeleted", messageId);
            return Ok(new { Status = "Deleted" });
        }
        catch (InvalidDataException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("attachment/{messageId}")]
    public async Task<IActionResult> AddAttachments(int messageId, [FromForm] params IFormFile[] files)
    {
        try
        {
            await messageService.AddAttachmentsAsync(messageId, files);
        }
        catch (Exception ex) when(ex is (InvalidFileSizeException or InvalidFileFormatException)) {
            return Forbid(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        return Ok(new { Message = "Added" });
    }

    [HttpDelete("attachment/{messageId}")]
    public async Task<IActionResult> RemoveAttachments(int messageId, [FromQuery] params int[]? idxes)
    {
        try
        {
            return await messageService.RemoveAttachmentsAsync(messageId, idxes)
                ? Ok(new { Message = "Removed" })
                : NotFound(new { Message = "Nothing to remove" });
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}

