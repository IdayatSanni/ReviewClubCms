using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewClubCms.Data;
using ReviewClubCms.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewClubCms.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
            
        }
        /// <summary>
        /// Displays the list of books in the database including their category 
        /// The response is an instance of each book in the database
        /// </summary>
        /// <returns>
        /// A list of BookDto instances representing the books in the database.
        /// </returns>
        /// <example>
        /// GET /api/Books/List [{"id": 1,"bookName": "I Am the Dark That Answers When You Call","bookAuthor": "Jamison Shea","categoryId": 3,"bookImage": null,"isBookOfTheMonth": false},
        /// {"id": 2,"bookName": "A Love Song for Ricki Wilde","bookAuthor": "Tia Williams","categoryId": 1,"bookImage": null,"isBookOfTheMonth": true},]
        /// </example>
        [HttpGet(template: "List")]
        public ActionResult<IEnumerable<BookDto>> ListBooks()
        {
            
            var books = _context.Books.Include(b => b.Category).ToList();
            var bookDtos = books.Select(book => new BookDto
            {
                Id = book.Id,
                BookName = book.BookName,
                BookAuthor = book.BookAuthor,
                CategoryId = book.CategoryId,
                IsBookOfTheMonth = book.IsBookOfTheMonth,
            }).ToList();

            return Ok(bookDtos);
        }
        /// <summary>
        /// Displays the book associated with the id in the database including it's category information
        /// </summary>
        /// <returns>
        /// An object representing the details of the requested book, including its category.
        /// </returns>
        /// <example>
        /// GET /api/Books/Find/{2} {"id": 2,"bookName": "A Love Song for Ricki Wilde","bookAuthor": "Tia Williams","bookPicture": "A Love Song.jpg","isBookOfTheMonth": true,"category": {"id": 1,"bookCategory": "Fiction"}}
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<object>> FindBook(int id)
        {
            var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var result = new
            {
                book.Id,
                book.BookName,
                book.BookAuthor,
                book.BookPicture,
                book.IsBookOfTheMonth,
                Category = new
                {
                    book.Category.Id,
                    book.Category.BookCategory
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Creates a new book in the database.
        /// </summary>
        /// <param name="bookDto">The object with the book details.</param>
        /// <returns>
        /// A BookDto instance representing the created book.
        /// </returns>
        /// <example>
        /// POST api/Books/Add 
        /// </example>

        [HttpPost(template: "Add")]
        public async Task<ActionResult<BookDto>> Add([FromForm] BookDto bookDto)
        {

            // Validate the CategoryId
            if (!await _context.Categories.AnyAsync(c => c.Id == bookDto.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Choose a category");
                return BadRequest(ModelState);
            }

            // Validate if the id is already in the database
            if (bookDto.Id.HasValue && await _context.Books.AnyAsync(b => b.Id == bookDto.Id.Value))
            {
                ModelState.AddModelError("Id", "The ID is already taken. Please choose another ID.");
                return BadRequest(ModelState);
            }


            
            var book = new Book
            {
                BookName = bookDto.BookName,
                BookAuthor = bookDto.BookAuthor,
                CategoryId = bookDto.CategoryId,
                IsBookOfTheMonth = bookDto.IsBookOfTheMonth
            };

            
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            
            var createdBookDto = new BookDto
            {
                Id = book.Id, 
                BookName = book.BookName,
                BookAuthor = book.BookAuthor,
                CategoryId = book.CategoryId,
                IsBookOfTheMonth = book.IsBookOfTheMonth
            };

            return CreatedAtAction(nameof(ListBooks), new { id = book.Id }, createdBookDto);
        }
        /// <summary>
        /// Update the book details if it is in the database
        /// </summary>
        /// <param name="id">Book Id to be updated</param>
        /// <param name="bookDto">The book detailss.</param>
        /// <returns>
        /// Returns a No Content (204) response if it is updated
        /// Returns a Bad Request (400) response if the id doesnt match
        /// Returns a Not Found (404) response when the book doesnt exist
        /// </returns>
        /// <example>
        /// PUT /api/Books/Update/{id}
        /// Request Body:
        /// {
        ///     "id": 7,
        ///     "bookName": "Updated Book Title",
        ///     "bookAuthor": "Updated Author",
        ///     "categoryId": 3,
        ///     "isBookOfTheMonth": true
        /// }
        /// </example>


        [HttpPut(template: "Update/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            if (id != bookDto.Id)
            {
                return BadRequest();
            }

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            
            existingBook.BookName = bookDto.BookName;
            existingBook.BookAuthor = bookDto.BookAuthor;
            existingBook.CategoryId = bookDto.CategoryId;
            existingBook.IsBookOfTheMonth = bookDto.IsBookOfTheMonth;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }


        /// <summary>
        /// Deletes book from the database
        /// </summary>
        /// <param name="id">The ID of the book to be deleted.</param>
        /// <returns>
        /// Response is No content 204 if the book is deleted.
        /// Response is Not Found 404 if the book is not found
        /// </returns>
        /// <example>
        /// /api/Books/Delete/{3}
        /// </example>

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// Returns true or false if a specific book with the provided id is in the databse
        /// </summary>
        /// <param name="id">The ID of the book.</param>
        /// <returns>
        /// If the book exists, it is true else it is false
        /// </returns>
        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
