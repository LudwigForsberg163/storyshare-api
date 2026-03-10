using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StoryShare.Api;
using StoryShare.Api.Users;
using StoryShare.Api.Health;
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



// Test endpoint to display JWT secret key (for testing only)
// app.MapGet("/test-jwt-key", (IConfiguration config) =>
// {
//     var key = config["Jwt:Key"];
//     return Results.Ok(new { jwtKey = key });
// });

app.MapGet("/", () => "Hello from Azure!");

// GET: Search/list books (with availability)

app.MapGet("/books", async (LibraryContext db, string? search) =>
{
    var query = db.Books.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
    var books = await query.ToListAsync();
    return Results.Ok(books);
})
    .RequireAuthorization()
    .WithName("GetBooks");

// POST: Borrow a book

app.MapPost("/books/{id}/borrow", async (int id, LibraryContext db) =>
{
    var book = await db.Books.FindAsync(id);
    if (book is null) return Results.NotFound();
    if (!book.IsAvailable) return Results.BadRequest("Book is already borrowed.");
    book.IsAvailable = false;
    var loan = new Loan { BookId = id, LoanedAt = DateTime.UtcNow, UserId = "user" };
    db.Loans.Add(loan);
    await db.SaveChangesAsync();
    return Results.Ok(loan);
})
    .RequireAuthorization()
    .WithName("BorrowBook");

// GET: View current loans

app.MapGet("/loans", async (LibraryContext db) =>
{
    var loans = await db.Loans
        .Include(l => l.Book)
        .Where(l => l.UserId == "user" && l.ReturnedAt == null)
        .ToListAsync();
    return Results.Ok(loans);
})
    .RequireAuthorization()
    .WithName("GetLoans");

// POST: Return a book

app.MapPost("/loans/{id}/return", async (int id, LibraryContext db) =>
{
    var loan = await db.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id && l.UserId == "user" && l.ReturnedAt == null);
    if (loan is null) return Results.NotFound();
    loan.ReturnedAt = DateTime.UtcNow;
    if (loan.Book is not null)
        loan.Book.IsAvailable = true;
    await db.SaveChangesAsync();
    return Results.Ok(loan);
})
    .RequireAuthorization()
    .WithName("ReturnBook");

app.Run();

record Story
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
