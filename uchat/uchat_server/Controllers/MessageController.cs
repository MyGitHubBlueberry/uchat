using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Models;
using uchat_server.Hubs;

namespace uchat_server.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class MessageController(IHubContext<ChatHub> hubContext, FakeDatabase db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] Message msg)
    {
        db.SaveMessage(msg);

        await hubContext.Clients.Group(msg.ChatId.ToString())
            .SendAsync("ReceiveMessage", msg);
        
        Console.WriteLine("Message sent");

        return Ok(new { Status = "Sent", MessageId = msg.Id });
    }
    
    [HttpGet("{chatId}")]
    public List<Message> GetMessages(int chatId)
    {
        var result =  db.GetMessagesByChat(chatId);
        
        return result;
    }
}