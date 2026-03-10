using StoryShare.Api;

namespace StoryShare.Api.Data;

public static class DataSeeder
{
    public static void SeedDatabase(LibraryContext db)
    {
        if (!db.Books.Any())
        {
            db.Books.AddRange(
                new Book {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928227",
                    PublicationDate = new DateTime(1937, 9, 21),
                    Description = "A fantasy novel and prelude to The Lord of the Rings.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/6979861-L.jpg",
                    Language = "English",
                    PageCount = 310,
                    Rating = 4.7,
                    TotalCopies = 3,
                    BorrowDays = 21
                },
                new Book {
                    Title = "The Fellowship of the Ring",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928210",
                    PublicationDate = new DateTime(1954, 7, 29),
                    Description = "The first volume of The Lord of the Rings.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8231856-L.jpg",
                    Language = "English",
                    PageCount = 423,
                    Rating = 4.8,
                    TotalCopies = 2,
                    BorrowDays = 21
                },
                new Book {
                    Title = "The Two Towers",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928203",
                    PublicationDate = new DateTime(1954, 11, 11),
                    Description = "The second volume of The Lord of the Rings.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8231857-L.jpg",
                    Language = "English",
                    PageCount = 352,
                    Rating = 4.7,
                    TotalCopies = 2,
                    BorrowDays = 21
                },
                new Book {
                    Title = "The Return of the King",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928197",
                    PublicationDate = new DateTime(1955, 10, 20),
                    Description = "The third volume of The Lord of the Rings.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8231858-L.jpg",
                    Language = "English",
                    PageCount = 416,
                    Rating = 4.9,
                    TotalCopies = 2,
                    BorrowDays = 21
                },
                new Book {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "978-0451524935",
                    PublicationDate = new DateTime(1949, 6, 8),
                    Description = "A dystopian social science fiction novel and cautionary tale.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/7222246-L.jpg",
                    Language = "English",
                    PageCount = 328,
                    Rating = 4.6,
                    TotalCopies = 4,
                    BorrowDays = 14
                },
                new Book {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    ISBN = "978-0061120084",
                    PublicationDate = new DateTime(1960, 7, 11),
                    Description = "A novel about the serious issues of rape and racial inequality.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8228691-L.jpg",
                    Language = "English",
                    PageCount = 281,
                    Rating = 4.8,
                    TotalCopies = 3,
                    BorrowDays = 14
                },
                new Book {
                    Title = "Pride and Prejudice",
                    Author = "Jane Austen",
                    ISBN = "978-1503290563",
                    PublicationDate = new DateTime(1813, 1, 28),
                    Description = "A romantic novel of manners.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8091016-L.jpg",
                    Language = "English",
                    PageCount = 279,
                    Rating = 4.5,
                    TotalCopies = 2,
                    BorrowDays = 14
                },
                new Book {
                    Title = "The Catcher in the Rye",
                    Author = "J.D. Salinger",
                    ISBN = "978-0316769488",
                    PublicationDate = new DateTime(1951, 7, 16),
                    Description = "A novel about teenage rebellion and angst.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8231996-L.jpg",
                    Language = "English",
                    PageCount = 214,
                    Rating = 4.0,
                    TotalCopies = 2,
                    BorrowDays = 14
                }
            );
            db.SaveChanges();
        }
    }
}
