using Microsoft.EntityFrameworkCore;

namespace StoryShare.Api;


public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<User> Users => Set<User>();
}
public class User
{
    public int Id { get; set; } // Primary key
    public string Username { get; set; } = string.Empty; // Unique, required
    public string PasswordHash { get; set; } = string.Empty; // Hashed password
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Registration date
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public DateTime LoanedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string UserId { get; set; } = "user"; // Single user for simplicity
}
