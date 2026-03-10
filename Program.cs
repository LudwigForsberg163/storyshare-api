using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StoryShare.Api;
using StoryShare.Api.Users;
using StoryShare.Api.Health;
using StoryShare.Api.Books;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// At the top
var allowedOrigins = new[] {
    "https://happy-island-094c05803.4.azurestaticapps.net", // your Azure Static Web App URL
    "http://localhost:3000" // for local development, optional
};


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add authentication and authorization
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();

// Seed database with initial books
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    StoryShare.Api.Data.DataSeeder.SeedDatabase(db);
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS policy
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

// Map user endpoints (register and login are public, others can be protected in their own files)
app.MapUsersEndpoints();
// Map health endpoints
app.MapHealthEndpoints();
// Map books endpoints
app.MapBooksEndpoints();


app.Run();
