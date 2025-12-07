using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Hubs;
using uchat_server.Args;
using Scalar.AspNetCore;
using uchat_server.Services;

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

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddScoped<IMessageService, MessageService>()
                        .AddScoped<IUserService, UserService>();


        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseCors();
        app.MapHub<ChatHub>("/chatHub");
        app.MapControllers();

        Console.WriteLine($"http://localhost:{port}/scalar/v1");

        app.Run($"http://localhost:{port}");
    }
}
