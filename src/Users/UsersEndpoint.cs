using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StoryShare.Api;
using Microsoft.AspNetCore.Identity;

namespace StoryShare.Api.Users;

public static class UsersEndpoint
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (LibraryContext db, RegisterRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Användarnamn och lösenord krävs.");

            if (await db.Users.AnyAsync(u => u.Username == req.Username))
                return Results.BadRequest("Användarnamnet finns redan.");


            var user = new User
            {
                Username = req.Username,
                CreatedAt = DateTime.UtcNow
            };
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, req.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok(new { user.Id, user.Username });
        });

        app.MapPost("/login", async (LibraryContext db, LoginRequest req, IConfiguration config) =>
        {

            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Användarnamn och lösenord krävs.");

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
                return Results.BadRequest("Felaktigt användarnamn eller lösenord.");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (result == PasswordVerificationResult.Failed)
                return Results.BadRequest("Felaktigt användarnamn eller lösenord.");

            // Generate JWT
            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                return Results.Problem("JWT-nyckel är inte konfigurerad.");

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(jwtKey);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim("userId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return Results.Ok(new { token = jwt });
        });
    }

    public record RegisterRequest(string Username, string Password);
    public record LoginRequest(string Username, string Password);
}
