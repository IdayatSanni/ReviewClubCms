using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewClubCms.Data;
using ReviewClubCms.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewClubCms.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the list of reviews in the database.
        /// The response contains instances of each review in the database.
        /// </summary>
        /// <returns>
        /// A list of ReviewDto instances representing the reviews in the database.
        /// </returns>
        /// <example>
        /// GET api/Books/List
        /// [
        ///     {"reviewId": 1, "reviewText": "This book changed my life", "reviewDate": "2024-10-03T01:35:29.8627405", "bookName": "I Am the Dark That Answers When You Call", "reviewersName": "idowu sule"}, 
        ///     {"reviewId": 2,"reviewText": "Everyone needs to read this book before clocking 30","reviewDate": "2024-10-03T01:35:29.8627405","bookName": "I Am the Dark That Answers When You Call","reviewersName": "narmin gurbani"}
        /// ]
        /// </example>
        [HttpGet(template: "List")]
        public ActionResult<IEnumerable<ReviewDto>> ListReviews()
        {
            var reviews = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Reviewers)
                .ToList();

            var reviewDtos = reviews.Select(review => new ReviewDto
            {
                ReviewId = review.ReviewId,
                ReviewText = review.ReviewText,
                ReviewDate = review.ReviewDate,
                BookName = review.Book?.BookName ?? "None",
                ReviewersName = review.Reviewers?.ReviewersName ?? "Don't exsist"
            }).ToList();

            return Ok(reviewDtos);
        }

        /// <summary>
        /// Displays the review associated with the ID in the database.
        /// Returns the details of the Review that matches the Id.
        /// </summary>
        /// /// <param name="id">Review Id to be display</param>
        /// <returns>
        /// An object representing the details of the requested review.
        /// </returns>
        /// <example>
        /// GET api/Books/Find/{3}
        /// { "reviewId": 3, "reviewText": "Meh, not pleased", "reviewDate": "2024-10-03T01:35:29.8627405", "bookName": "A Love Song for Ricki Wilde", "reviewersName": "Jupet Jig"}
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<ReviewDto>> FindReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Reviewers)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            var result = new ReviewDto
            {
                ReviewId = review.ReviewId,
                ReviewText = review.ReviewText,
                ReviewDate = review.ReviewDate,
                BookName = review.Book?.BookName ?? "Unknown",
                ReviewersName = review.Reviewers?.ReviewersName ?? "Anonymous"
            };

            return Ok(result);
        }

        /// <summary>
        /// Creates a new review in the database by a reviewer already in the database and for a book already in the database.
        /// Returns the created review as a ReviewDto instance.
        /// </summary>
        /// <param name="createReviewDto">The object containing the details of the review.</param>
        /// <returns>
        /// A ReviewDto instance representing the created review.
        /// </returns>
        /// <example>
        /// POST api/Books/Add
        /// Request Body: {"reviewText": "Amazing read!", "reviewersId": 4, "bookId": 4}
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<ActionResult<ReviewDto>> Add([FromBody] CreateReviewDto createReviewDto)
        {
            // Validate that the reviewer exists
            var reviewer = await _context.Reviewers.FindAsync(createReviewDto.ReviewersId);
            if (reviewer == null)
            {
                ModelState.AddModelError("ReviewersId", "Invalid reviewer ID");
                return BadRequest(ModelState);
            }

            // Validate that the book exists
            var book = await _context.Books.FindAsync(createReviewDto.BookId);
            if (book == null)
            {
                ModelState.AddModelError("BookId", "Invalid book ID");
                return BadRequest(ModelState);
            }

            
            var review = new Review
            {
                ReviewText = createReviewDto.ReviewText,
                ReviewersId = createReviewDto.ReviewersId,
                BookId = createReviewDto.BookId,
                ReviewDate = DateTime.UtcNow
            };

            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            
            var createdReviewDto = new ReviewDto
            {
                ReviewId = review.ReviewId,
                ReviewText = review.ReviewText,
                ReviewDate = review.ReviewDate,
                BookName = book.BookName,
                ReviewersName = reviewer.ReviewersName
            };

            return CreatedAtAction(nameof(ListReviews), new { id = review.ReviewId }, createdReviewDto);
        }

        /// <summary>
        /// Updates the review details if it is in the database.
        /// The request body should contain the updated review text using UpdateReviewDto.
        /// </summary>
        /// <param name="id">Review ID to be updated.</param>
        /// <param name="updateReviewDto">The updated review details.</param>
        /// <returns>
        /// Returns No Content (204) response if the update is successful.
        /// Returns Bad Request (400) response if the ID doesn't match or invalid.
        /// Returns Not Found (404) response when the review doesn't exist.
        /// </returns>
        /// <example>
        /// PUT api/Books/Update/{id}
        /// Request Body: {"reviewText": "Updated review text"}
        /// </example>
        [HttpPut(template: "Update/{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            // Check if the provided ID matches
            if (id <= 0)
            {
                return BadRequest();
            }

            // Retrieve the existing review from the database
            var existingReview = await _context.Reviews.FindAsync(id);
            if (existingReview == null)
            {
                return NotFound();
            }

            // Update the ReviewText
            existingReview.ReviewText = updateReviewDto.ReviewText;

            // Mark the entity as modified
            _context.Entry(existingReview).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound();
                }
                throw; 
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a review from the database.
        /// </summary>
        /// <param name="id">The ID of the review to delete.</param>
        /// <returns>
        /// No Content (204) if the deletion is successful.
        /// </returns>
        /// <example>
        /// DELETE api/Books/Delete/{1}
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Returns true or false if a specific review with the provided ID is in the database.
        /// </summary>
        /// <param name="id">The ID of the review.</param>
        /// <returns>
        /// If the review exists, it returns true; else it returns false.
        /// </returns>
        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}
