using ImagesShop.Application.DependencyInjection;
using ImagesShop.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddApplicationServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
   .WriteTo.File("log/ImagesShopLogs.txt", rollingInterval: RollingInterval.Day).CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await ImagesShop.Infrastructure.Data.DbInitializer.SeedAsync(services);
}

app.Run();

