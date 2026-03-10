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
        app.MapPost("/register", async (LibraryContext db, RegisterRequest req, UserService userService) =>
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

            // Generate JWT using UserService
            string jwt;
            try
            {
                jwt = userService.GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
            return Results.Ok(new { token = jwt });
        });

        app.MapPost("/login", async (LibraryContext db, LoginRequest req, UserService userService) =>
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

            // Generate JWT using UserService
            string jwt;
            try
            {
                jwt = userService.GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
            return Results.Ok(new { token = jwt });
        });
    }

    public record RegisterRequest(string Username, string Password);
    public record LoginRequest(string Username, string Password);
}
