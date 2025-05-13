using System.Text.Json.Serialization;
using LoungeSaber_Server.SQL;
using Newtonsoft.Json;

namespace LoungeSaber_Server.Api;

public static class Api
{
    public static void Start()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
            
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}