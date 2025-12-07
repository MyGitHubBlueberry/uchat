using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Hubs;
using uchat_server.Args;

class Program {
    static void Main(string[] args) {
        int port;

        if (!Parser.Parse(args, out port)) return;

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

        app.Run($"http://localhost:{port}");
    }
}
