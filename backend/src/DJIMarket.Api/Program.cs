using DJIMarket.Application;
using DJIMarket.Infrastructure;
using DJIMarket.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseInitializer.InitializeAsync(db);
}

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "dji-market-api" }));
app.MapControllers();

app.Run();

public partial class Program { }
