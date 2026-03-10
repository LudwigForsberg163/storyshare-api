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
                    Title = "Hobbiten",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928227",
                    PublicationDate = new DateTime(1937, 9, 21),
                    Description = "En fantasyroman och förspel till Sagan om ringen.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/6979861-L.jpg",
                    Language = "Svenska",
                    PageCount = 310,
                    Rating = 4.7,
                    TotalCopies = 1,
                    BorrowDays = 21
                },
                new Book {
                    Title = "Sagan om ringen",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928210",
                    PublicationDate = new DateTime(1954, 7, 29),
                    Description = "Den första delen av Sagan om ringen.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8172082-L.jpg",
                    Language = "Svenska",
                    PageCount = 423,
                    Rating = 4.8,
                    TotalCopies = 2,
                    BorrowDays = 21
                },
                new Book {
                    Title = "De två tornen",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928203",
                    PublicationDate = new DateTime(1954, 11, 11),
                    Description = "Den andra delen av Sagan om ringen.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/14627082-L.jpg",
                    Language = "Svenska",
                    PageCount = 352,
                    Rating = 4.7,
                    TotalCopies = 2,
                    BorrowDays = 21
                },
                new Book {
                    Title = "Konungens återkomst",
                    Author = "J.R.R. Tolkien",
                    ISBN = "978-0547928197",
                    PublicationDate = new DateTime(1955, 10, 20),
                    Description = "Den tredje delen av Sagan om ringen.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/14626055-L.jpg",
                    Language = "Svenska",
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
                    Description = "En dystopisk roman och varning om framtiden.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/7222246-L.jpg",
                    Language = "Svenska",
                    PageCount = 328,
                    Rating = 4.6,
                    TotalCopies = 4,
                    BorrowDays = 14
                },
                new Book {
                    Title = "Dödssynden",
                    Author = "Harper Lee",
                    ISBN = "978-0061120084",
                    PublicationDate = new DateTime(1960, 7, 11),
                    Description = "En roman om allvarliga frågor som våldtäkt och rasism.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/15153500-L.jpg",
                    Language = "Svenska",
                    PageCount = 281,
                    Rating = 4.8,
                    TotalCopies = 3,
                    BorrowDays = 14
                },
                new Book {
                    Title = "Stolthet och fördom",
                    Author = "Jane Austen",
                    ISBN = "978-1503290563",
                    PublicationDate = new DateTime(1813, 1, 28),
                    Description = "En romantisk roman om seder och bruk.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/8090214-L.jpg",
                    Language = "Svenska",
                    PageCount = 279,
                    Rating = 4.5,
                    TotalCopies = 2,
                    BorrowDays = 14
                },
                new Book {
                    Title = "Räddaren i nöden",
                    Author = "J.D. Salinger",
                    ISBN = "978-0316769488",
                    PublicationDate = new DateTime(1951, 7, 16),
                    Description = "En roman om tonårsuppror och ångest.",
                    ImageUrl = "https://covers.openlibrary.org/b/id/15172466-L.jpg",
                    Language = "Svenska",
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
