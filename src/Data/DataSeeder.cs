using StoryShare.Api;

namespace StoryShare.Api.Data;

public static class DataSeeder
{
    public static void SeedDatabase(LibraryContext db)
    {
        if (!db.Books.Any())
        {
            db.Books.AddRange(
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien" },
                new Book { Title = "The Fellowship of the Ring", Author = "J.R.R. Tolkien" },
                new Book { Title = "The Two Towers", Author = "J.R.R. Tolkien" },
                new Book { Title = "The Return of the King", Author = "J.R.R. Tolkien" },
                new Book { Title = "1984", Author = "George Orwell" },
                new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee" },
                new Book { Title = "Pride and Prejudice", Author = "Jane Austen" },
                new Book { Title = "The Catcher in the Rye", Author = "J.D. Salinger" }
            );
            db.SaveChanges();
        }
    }
}
