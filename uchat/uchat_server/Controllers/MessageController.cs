using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Models;
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
        
        if (msg == null) return BadRequest("Invalid message format");

        await messageService.SaveMessageAsync(msg, files);

        await hubContext.Clients.Group(msg.ChatId.ToString())
            .SendAsync("ReceiveMessage", msg);

        Console.WriteLine("Message sent");

        return Ok(new { Status = "Sent", MessageId = msg.Id });
    }

    [HttpGet("{chatId}")]
    public async Task<List<Message>> GetMessages(int chatId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        return await messageService.GetChatMessagesDtoAsync(chatId, pageNumber, pageSize);
    }

    [HttpPatch("text/{messageId}")]
    public async Task<IActionResult> ChangeMessageText(int messageId, string text) {
        try {
            await messageService.ChangeMessageTextAsync(messageId, text);
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        }
        return Ok(new { Status = "Changed" });
    }

    [HttpPost("attachment/{messageId}")]
    public async Task<IActionResult> AddAttachments(int messageId, [FromForm] params IFormFile[] files) {
        try {
            await messageService.AddAttachmentsAsync(messageId, files);
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        }
        return Ok(new { Status = "Added" });
    }

    [HttpDelete("attachment/{messageId}")]
    public async Task<IActionResult> RemoveAttachments(int messageId, [FromQuery] params int[]? idxes) {
        try {
            return Ok(new { Status = await messageService
                    .RemoveAttachmentsAsync(messageId, idxes)
                    ? "Removed" : "Nothing to remove"
                });
        } catch (InvalidDataException ex) {
            return NotFound(ex.Message);
        }
    }
}

