using Microsoft.EntityFrameworkCore;

namespace StoryShare.Api;


public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BookTag> BookTags => Set<BookTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BookTag>()
            .HasKey(bt => new { bt.BookId, bt.TagId });
    }
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
    public string? ISBN { get; set; }
    public DateTime? PublicationDate { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public int? PageCount { get; set; }
    public double? Rating { get; set; }
    public int TotalCopies { get; set; } = 1;
    public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<BookTag> BookTags { get; set; } = new List<BookTag>();
}

public class BookTag
{
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
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
