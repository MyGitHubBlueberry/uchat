using uchat_server;
using uchat_server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<FakeDatabase>()
    .AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();