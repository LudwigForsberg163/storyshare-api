using Microsoft.EntityFrameworkCore;
using StoryShare.Api;

namespace StoryShare.Api.Books;

public static class BooksEndpoints
{
	public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapGet("/books/{id}", async (int id, LibraryContext db, HttpContext http) =>
			{
				var userId = http.User?.Identity?.Name ?? "user";
				var book = await db.Books
					.Include(b => b.BookTags)
						.ThenInclude(bt => bt.Tag)
					.FirstOrDefaultAsync(b => b.Id == id);
				if (book == null)
					return Results.NotFound("Boken hittades inte.");

				// Calculate available copies
				var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.ReturnedAt == null);
				var availableCopies = book.TotalCopies - activeLoans;
				// Check if the current user has an active loan for this book
				var userHasLoan = await db.Loans.AnyAsync(l => l.BookId == id && l.UserId == userId && l.ReturnedAt == null);
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
					LoanedByCurrentUser = userHasLoan,
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

			// Get user id (replace with real user logic as needed)
			var userId = http.User?.Identity?.Name ?? "user";

			// Check if user already has an active loan on this specific book
			var userActiveLoanForBook = await db.Loans.AnyAsync(l => l.UserId == userId && l.BookId == id && l.ReturnedAt == null);
			if (userActiveLoanForBook)
			{
				return Results.BadRequest("Du har redan en aktiv utlåning på denna bok.");
			}

			// Check available copies
			var activeLoans = await db.Loans.CountAsync(l => l.BookId == id && l.ReturnedAt == null);
			var availableCopies = book.TotalCopies - activeLoans;
			if (availableCopies <= 0)
			{
				return Results.BadRequest("Inga exemplar tillgängliga för utlåning.");
			}

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
			var allLoans = await db.Loans
				.Include(l => l.Book)
				.Where(l => l.UserId == userId)
				.ToListAsync();

			var active = allLoans
				.Where(l => l.ReturnedAt == null)
				.OrderBy(l => l.DueDate)
				.Select(l => new {
					l.Id,
					l.BookId,
					BookTitle = l.Book != null ? l.Book.Title : null,
					l.LoanedAt,
					l.DueDate,
					BorrowedTime = ToDurationString(l.DueDate - l.LoanedAt),
					TimeRemaining = ToDurationString(l.DueDate - DateTime.UtcNow),
					TotalCopies = l.Book?.TotalCopies ?? 0,
					AvailableCopies = l.Book != null ? l.Book.TotalCopies - db.Loans.Count(x => x.BookId == l.BookId && x.ReturnedAt == null) : 0
				})
				.ToList();

			var inactive = allLoans
				.Where(l => l.ReturnedAt != null)
				.OrderByDescending(l => l.ReturnedAt)
				.Select(l => new {
					l.Id,
					l.BookId,
					BookTitle = l.Book != null ? l.Book.Title : null,
					l.LoanedAt,
					l.DueDate,
					ReturnedAt = l.ReturnedAt,
					BorrowedTime = ToDurationString(l.DueDate - l.LoanedAt),
					DaysLeftWhenReturned = l.ReturnedAt.HasValue ? ToDurationString(l.DueDate - l.ReturnedAt.Value) : null
				})
				.ToList();

			var username = userId;
			return Results.Ok(new { active, inactive, username });

			// Local helper for formatting TimeSpan as days, hours, minutes
			static string ToDurationString(TimeSpan ts)
			{
				var sign = ts.TotalSeconds < 0 ? "-" : "";
				ts = ts.Duration();
				var days = (int)ts.TotalDays;
				var hours = ts.Hours;
				var minutes = ts.Minutes;
				var parts = new List<string>();
				if (days != 0) parts.Add($"{days} dagar");
				if (hours != 0) parts.Add($"{hours} timmar");
				if (minutes != 0 || parts.Count == 0) parts.Add($"{minutes} minuter");
				return sign + string.Join(", ", parts);
			}
		})
		.RequireAuthorization()
		.WithName("GetCurrentLoans");

	}
}
