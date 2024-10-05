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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the list of categories in the database. 
        /// The response is an instance of each category in the database.
        /// </summary>
        /// <returns>
        /// A list of CategoryDto instances representing the categories in the database.
        /// </returns>
        /// <example>
        /// GET api/categories 
        /// [
        ///     {"id": 1, "bookCategory": "Fiction"},
        ///     {"id": 2, "bookCategory": "Non-Fiction"}
        /// ]
        /// </example>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                BookCategory = c.BookCategory
            }).ToList();

            return Ok(categoryDtos);
        }

        /// <summary>
        /// Displays the category associated with the id in the database.
        /// </summary>
        /// <returns>
        /// An object representing the details of the requested category.
        /// </returns>
        /// <example>
        /// GET api/categories/{id} 
        /// {"id": 1, "bookCategory": "Fiction"}
        /// </example>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                BookCategory = category.BookCategory
            };

            return Ok(categoryDto);
        }

        /// <summary>
        /// Creates a new category in the database.
        /// </summary>
        /// <param name="categoryDto">The object containing the details of the category.</param>
        /// <returns>
        /// A CategoryDto instance representing the created category.
        /// </returns>
        /// <example>
        /// POST api/categories 
        /// Request Body: {"id": 3, "bookCategory": "Science Fiction"}
        /// </example>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto categoryDto)
        {
            if (string.IsNullOrWhiteSpace(categoryDto.BookCategory))
            {
                ModelState.AddModelError("BookCategory", "BookCategory is required.");
                return BadRequest(ModelState);
            }

            if (await _context.Categories.AnyAsync(c => c.Id == categoryDto.Id))
            {
                ModelState.AddModelError("Id", "The ID is already taken. Please choose another ID.");
                return BadRequest(ModelState);
            }

            var category = new Category
            {
                Id = categoryDto.Id,
                BookCategory = categoryDto.BookCategory
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var createdCategoryDto = new CategoryDto
            {
                Id = category.Id,
                BookCategory = category.BookCategory
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, createdCategoryDto);
        }

        /// <summary>
        /// Updates the category details if it is in the database.
        /// </summary>
        /// <param name="id">Category Id to be updated.</param>
        /// <param name="categoryDto">The updated category details.</param>
        /// <returns>
        /// Returns a No Content (204) response if it is updated.
        /// Returns a Bad Request (400) response if the id doesn't match.
        /// Returns a Not Found (404) response when the category doesn't exist.
        /// </returns>
        /// <example>
        /// PUT api/categories/{id} 
        /// Request Body: {"id": 2, "bookCategory": "Updated Category"}
        /// </example>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            if (id != categoryDto.Id)
            {
                return BadRequest();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.BookCategory = categoryDto.BookCategory;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                throw; 
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a category from the database.
        /// </summary>
        /// <param name="id">The ID of the category to be deleted.</param>
        /// <returns>
        /// Response is No Content (204) if the category is deleted.
        /// Response is Not Found (404) if the category is not found.
        /// </returns>
        /// <example>
        /// DELETE api/categories/{id} 
        /// </example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Returns true or false if a specific category with the provided id is in the database.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>
        /// If the category exists, it is true; else it is false.
        /// </returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
