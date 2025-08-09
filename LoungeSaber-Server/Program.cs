using LoungeSaber_Server.Installer;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

namespace LoungeSaber_Server
{
    public class Program
    {
        public static bool Debug { get; private set; } = false;
        
        public static async Task Main(string[] args)
        {
            if (args.Contains("--debug"))
                Debug = true;
            
            var builder = WebApplication.CreateBuilder(args);

            BindingsInstaller.InstallBindings(builder.Services);
            
            builder.Services.AddDiscordGateway().AddApplicationCommands();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            var host = builder.Build();
            
            if (host.Environment.IsDevelopment())
            {
                host.UseSwagger();
                host.UseSwaggerUI();
            }

            host.UseHttpsRedirection();
            host.MapControllers();
            
            host.AddModules(typeof(Program).Assembly);

            host.UseGatewayHandlers();

            await host.RunAsync();
        }
    }
}