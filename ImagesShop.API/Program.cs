using ImagesShop.Application.DependencyInjection;
using ImagesShop.Infrastructure.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Infrastructure and Application
builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddApplicationServices();

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

// Run data seeding using Infrastructure.DbInitializer
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await ImagesShop.Infrastructure.Data.DbInitializer.SeedAsync(services);
}

app.Run();

