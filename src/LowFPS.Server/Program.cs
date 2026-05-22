using LowFPS.Server.StreamingHubs;

var builder = WebApplication.CreateBuilder(args);
var magiconion = builder.Services.AddMagicOnion();

builder.Services.AddSingleton<RoomContextRepository>();

builder.Services.AddMagicOnion();

builder.Services.AddMvcCore().AddApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapMagicOnionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();