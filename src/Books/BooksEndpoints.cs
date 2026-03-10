using Microsoft.EntityFrameworkCore;
using StoryShare.Api;

namespace StoryShare.Api.Books;

public static class BooksEndpoints
{
	public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
	{
		app.MapGet("/books/search", async (LibraryContext db, string? search) =>
		{
			if (string.IsNullOrWhiteSpace(search))
				return Results.BadRequest("Search term is required.");

			// Substring search (case-insensitive)
			var books = await db.Books
				.Where(b => b.Title.ToLower().Contains(search.ToLower()))
				.ToListAsync();
			return Results.Ok(books);
		})
		.RequireAuthorization()
		.WithName("SearchBooksByTitleSubstring");
	}

	// SequenceMatch helper removed; now using .Contains for substring search
}
