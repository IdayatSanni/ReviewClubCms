using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewClubCms.Data;
using ReviewClubCms.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewClubCms.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the list of reviewers and the number of reviews they have submitted in the database. 
        /// The response is an instance of each reviewer in the database.
        /// </summary>
        /// <returns>
        /// A list of ReviewerDto instances representing the reviewers in the database, 
        /// including the count of books reviewed.
        /// </returns>
        /// <example>
        /// GET api/Reviewers/List
        /// [
        ///     { "reviewersId": 1, "reviewersName": "narmin gurbani", "reviewersEmail": "narmin.gurb@example.com", "reviewedBookCount": 1}, 
        ///     { "reviewersId": 2, "reviewersName": "idowu sule", "reviewersEmail": "idowu.sule@example.com", "reviewedBookCount": 1},
        /// ]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<ReviewerDto>>> ListReviewers()
        {
            var reviewers = await _context.Reviewers
                .Include(r => r.Reviews) // Include reviews to count them
                .ToListAsync();

            var reviewerDtos = reviewers.Select(reviewer => new ReviewerDto
            {
                ReviewersId = reviewer.ReviewersId,
                ReviewersName = reviewer.ReviewersName,
                ReviewersEmail = reviewer.ReviewersEmail,
                ReviewedBookCount = reviewer.Reviews?.Count() ?? 0 // Count of reviewed books
            }).ToList();

            return Ok(reviewerDtos);
        }

        /// <summary>
        /// Displays the reviewer associated with the id in the database with their review count.
        /// </summary>
        /// <returns>
        /// An object representing the details of the requested reviewer.
        /// </returns>
        /// <example>
        /// GET /api/Reviewers/Find/{3}
        /// { "reviewersId": 3, "reviewersName": "Jupet Jig", "reviewersEmail": "jupet.jig@example.com", "reviewedBookCount": 1}
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<ReviewerDto>> FindReviewer(int id)
        {
            var reviewer = await _context.Reviewers
                .Include(r => r.Reviews) // 
                .FirstOrDefaultAsync(r => r.ReviewersId == id);

            if (reviewer == null)
            {
                return NotFound();
            }

            var reviewerDto = new ReviewerDto
            {
                ReviewersId = reviewer.ReviewersId,
                ReviewersName = reviewer.ReviewersName,
                ReviewersEmail = reviewer.ReviewersEmail,
                ReviewedBookCount = reviewer.Reviews?.Count() ?? 0 
            };

            return Ok(reviewerDto);
        }

        /// <summary>
        /// Creates a new reviewer in the database.
        /// </summary>
        /// <param name="reviewerDto">The object containing the details of the reviewer.</param>
        /// <returns>
        /// A ReviewerDto instance representing the created reviewer.
        /// </returns>
        /// <example>
        /// POST api/Reviewers/Add
        /// Request Body: {"reviewersName": "New Reviewer", "reviewersEmail": "reviewer@email.com"}
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<ActionResult<ReviewerDto>> Add([FromBody] ReviewerDto reviewerDto)
        {
            if (string.IsNullOrWhiteSpace(reviewerDto.ReviewersName))
            {
                ModelState.AddModelError("ReviewersName", "Name is required");
                return BadRequest(ModelState);
            }

            
            if (string.IsNullOrWhiteSpace(reviewerDto.ReviewersEmail) ||
                !new EmailAddressAttribute().IsValid(reviewerDto.ReviewersEmail))
            {
                ModelState.AddModelError("ReviewersEmail", "A valid email address is required.");
                return BadRequest(ModelState);
            }

            
            var reviewer = new Reviewer
            {
                ReviewersName = reviewerDto.ReviewersName,
                ReviewersEmail = reviewerDto.ReviewersEmail
            };

            _context.Reviewers.Add(reviewer);
            await _context.SaveChangesAsync();

            reviewerDto.ReviewersId = reviewer.ReviewersId; 

            return CreatedAtAction(nameof(ListReviewers), new { id = reviewer.ReviewersId }, reviewerDto);
        }

        /// <summary>
        /// Updates the reviewer details if it is in the database.
        /// </summary>
        /// <param name="id">Reviewer Id to be updated.</param>
        /// <param name="reviewerDto">The updated reviewer details.</param>
        /// <returns>
        /// Returns a No Content (204) response if it is updated.
        /// Returns a Bad Request (400) response if the id doesn't match.
        /// Returns a Not Found (404) response when the reviewer doesn't exist.
        /// </returns>
        /// <example>
        /// PUT /api/Reviewers/Update/{2}
        /// Request Body: {"reviewersId": 2, "reviewersName": "Updated Name", "reviewersEmail": "Updated Email"}
        /// </example>
        [HttpPut(template: "Update/{id}")]
        public async Task<IActionResult> UpdateReviewer(int id, [FromBody] ReviewerDto reviewerDto)
        {
            if (id != reviewerDto.ReviewersId)
            {
                return BadRequest();
            }

            var reviewer = await _context.Reviewers.FindAsync(id);
            if (reviewer == null)
            {
                return NotFound();
            }

            reviewer.ReviewersName = reviewerDto.ReviewersName;
            reviewer.ReviewersEmail = reviewerDto.ReviewersEmail;

            _context.Entry(reviewer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewerExists(id))
                {
                    return NotFound();
                }
                throw; 
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a reviewer from the database.
        /// </summary>
        /// <param name="id">The ID of the reviewer to delete.</param>
        /// <returns>
        /// No content if the deletion is successful; otherwise, NotFound.
        /// </returns>
        /// <example>
        /// DELETE /api/Reviewers/Delete/{3}
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteReviewer(int id)
        {
            var reviewer = await _context.Reviewers.FindAsync(id);
            if (reviewer == null)
            {
                return NotFound();
            }

            _context.Reviewers.Remove(reviewer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Returns true or false if a specific reviewer with the provided id is in the database.
        /// </summary>
        /// <param name="id">The ID of the reviewer.</param>
        /// <returns>
        /// If the reviewer exists, it is true; else it is false.
        /// </returns>
        private bool ReviewerExists(int id)
        {
            return _context.Reviewers.Any(e => e.ReviewersId == id);
        }
    }
}
