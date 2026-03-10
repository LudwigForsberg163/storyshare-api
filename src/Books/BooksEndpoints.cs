using Microsoft.EntityFrameworkCore;
using StoryShare.Api;

namespace StoryShare.Api.Books;

public static class BooksEndpoints
{
	public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
	{

        // GET: Get all data for a single book
        app.MapGet("/books/{id}", async (int id, LibraryContext db) =>
        {
            var book = await db.Books
                .Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id);
			if (book == null)
				return Results.NotFound("Boken hittades inte.");

			// Calculate available copies
			var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.ReturnedAt == null);
			var availableCopies = book.TotalCopies - activeLoans;
			var result = new {
				book.Id,
				book.Title,
				book.Author,
				book.ISBN,
				book.PublicationDate,
				book.Description,
				book.ImageUrl,
				book.Language,
				book.PageCount,
				book.Rating,
				book.TotalCopies,
				book.BorrowDays,
				AvailableCopies = availableCopies,
				Tags = book.BookTags.Select(bt => bt.Tag.Name).ToList()
			};
			return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetBookDetails");

		app.MapGet("/books/search", async (LibraryContext db, string? search) =>
		{
			// If search is empty, return all books
			List<Book> books;
			if (string.IsNullOrWhiteSpace(search))
			{
				books = await db.Books.ToListAsync();
			}
			else
			{
				// Substring search (case-insensitive)
				books = await db.Books
					.Where(b => b.Title.ToLower().Contains(search.ToLower()))
					.ToListAsync();
			}

			// Get loan counts for each book as a dictionary
			var loanCounts = (await db.Loans
				.Where(l => l.ReturnedAt == null)
				.GroupBy(l => l.BookId)
				.Select(g => new { BookId = g.Key, Count = g.Count() })
				.ToListAsync())
				.ToDictionary(x => x.BookId, x => x.Count);

			var result = books.Select(b => new {
				b.Id,
				b.Title,
				b.Author,
				b.ISBN,
				b.PublicationDate,
				b.Description,
				b.ImageUrl,
				b.Language,
				b.PageCount,
				b.Rating,
				b.TotalCopies,
				b.BorrowDays,
				AvailableCopies = b.TotalCopies - (loanCounts.TryGetValue(b.Id, out var count) ? count : 0)
			});
			return Results.Ok(result);
		})
		.RequireAuthorization()
		.WithName("SearchBooksByTitleSubstring");

        // POST: Loan a book
        app.MapPost("/books/{id}/loan", async (int id, LibraryContext db, HttpContext http) =>
        {
			var book = await db.Books.FindAsync(id);
			if (book == null)
				return Results.NotFound("Boken hittades inte.");

			// Check if there are available copies
			var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.ReturnedAt == null);
			var availableCopies = book.TotalCopies - activeLoans;
			if (availableCopies <= 0)
				return Results.BadRequest("Inga exemplar tillgängliga för utlåning.");

			// Get user id (replace with real user logic as needed)
			var userId = http.User?.Identity?.Name ?? "user";

			var now = DateTime.UtcNow;
			var loan = new Loan
			{
				BookId = book.Id,
				LoanedAt = now,
				DueDate = now.AddDays(book.BorrowDays),
				UserId = userId
			};
			db.Loans.Add(loan);
			await db.SaveChangesAsync();
			return Results.Ok(loan);
        })
        .RequireAuthorization()
        .WithName("LoanBook");

        // POST: Return a book
        app.MapPost("/books/{id}/return", async (int id, LibraryContext db, HttpContext http) =>
        {
            // Get user id (replace with real user logic as needed)
            var userId = http.User?.Identity?.Name ?? "user";

            // Find the active loan for this user and book
            var loan = await db.Loans.FirstOrDefaultAsync(l => l.BookId == id && l.UserId == userId && l.ReturnedAt == null);
			if (loan == null)
				return Results.NotFound("Ingen aktiv utlåning hittades för denna bok och användare.");

            loan.ReturnedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(loan);
        })
        .RequireAuthorization()
        .WithName("ReturnBook");

        // GET: View current loans for the authenticated user
			app.MapGet("/loans/current", async (LibraryContext db, HttpContext http) =>
			{
				var userId = http.User?.Identity?.Name ?? "user";
				var loans = await db.Loans
					.Include(l => l.Book)
					.Where(l => l.UserId == userId && l.ReturnedAt == null)
					.Select(l => new {
						l.Id,
						l.BookId,
						BookTitle = l.Book != null ? l.Book.Title : null,
						l.LoanedAt,
						l.DueDate
					})
					.ToListAsync();
				return Results.Ok(loans);
			})
			.RequireAuthorization()
			.WithName("GetCurrentLoans");

	}
}
