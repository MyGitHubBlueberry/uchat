using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services
    .AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    })
    .AddSignalR();

builder.Services.AddControllers();

var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");
app.MapControllers();

app.Run();