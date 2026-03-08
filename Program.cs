// C# – Backend (ASP.NET Core)
using Microsoft.EntityFrameworkCore;
using TouristSafetySystem.Data;
using TouristSafetySystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// notification and blockchain helpers
builder.Services.AddSingleton<NotificationService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new NotificationService(config);
});

builder.Services.AddSingleton<BlockchainService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new BlockchainService(config["Blockchain:RpcUrl"]);
});

builder.Services.AddHttpClient<MapsService>();
builder.Services.AddHttpClient<AIService>();
builder.Services.AddScoped<LocationTrackingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:8080", "file://") // Add your frontend URLs
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();