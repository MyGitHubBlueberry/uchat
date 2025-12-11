using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Hubs;
using uchat_server.Args;
using Scalar.AspNetCore;
using uchat_server.Services;
using System.Diagnostics;

class Program {
    static void Main(string[] args) {
        int port;
        bool isDeamon;

        if (!Parser.Parse(args, out port, out isDeamon)) return;
        if (isDeamon) {
            RunAsDeamon(args);
            return;
        }

        Environment.SetEnvironmentVariable("UCHAT_SERVER_PORT", port.ToString(), EnvironmentVariableTarget.User);

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);

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
                        .AddScoped<IUserService, UserService>()
                        .AddScoped<IChatService, ChatService>();


        var app = builder.Build();

        // Apply migrations automatically
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseStaticFiles();
        app.UseCors();
        app.MapHub<ChatHub>("/chatHub");
        app.MapControllers();

        Console.WriteLine($"http://localhost:{port}/scalar/v1");

        app.Run($"http://localhost:{port}");
    }
    
    static void RunAsDeamon(string[] args) {
            var newArgs = args.Where(x => x != "-d" && x != "--daemon");
            
            var psi = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                Arguments = string.Join(" ", newArgs),
                UseShellExecute = true, //todo try false
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

        var p = Process.Start(psi);
        if (p is null) {
            Console.WriteLine("Failed to start server as daemon");
            return;
        }

        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine($"Server process id is: {p.Id}");
        Console.BackgroundColor = ConsoleColor.White;

        return;
    }
}
