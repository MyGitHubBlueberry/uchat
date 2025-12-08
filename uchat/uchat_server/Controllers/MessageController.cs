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
    public async Task<IActionResult> SendMessage([FromBody] Message msg)
    {
        await messageService.SaveMessageAsync(msg);

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

    // TODO: add save message
}
