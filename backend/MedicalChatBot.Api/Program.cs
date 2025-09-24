using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MedicalChatBot.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// HttpClient for Ollama requests
builder.Services.AddHttpClient();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")  // Angular frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 COMMENT OUT HTTPS redirection during local testing
// app.UseHttpsRedirection();

app.UseCors("AllowLocalhost"); // CORS must be before authorization
app.UseAuthorization();
app.MapControllers();
app.Run();
