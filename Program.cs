

using Microsoft.EntityFrameworkCore;
using StoryShare.Api;
using StoryShare.Api.Users;
using StoryShare.Api.Health;

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


var app = builder.Build();

// Seed database with initial books
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    if (!db.Books.Any())
    {
        db.Books.AddRange(
            new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien" },
            new Book { Title = "1984", Author = "George Orwell" },
            new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee" },
            new Book { Title = "Pride and Prejudice", Author = "Jane Austen" },
            new Book { Title = "The Catcher in the Rye", Author = "J.D. Salinger" }
        );
        db.SaveChanges();
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS policy
app.UseCors("AllowFrontend");


app.UseHttpsRedirection();

// Map user endpoints
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
    .WithName("ReturnBook");

app.Run();

record Story
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
