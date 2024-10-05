using System.ComponentModel.DataAnnotations;

namespace ReviewClubCms.Models
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ReviewText { get; set; } = "";

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public string? BookName { get; set; } = "";
        public string? ReviewersName { get; set; } = "";
    }

    public class CreateReviewDto
    {
        [Required]
        [MaxLength(1000)]
        public string ReviewText { get; set; } = "";

        [Required]
        public int ReviewersId { get; set; } 

        [Required]
        public int BookId { get; set; } 
    }

    public class UpdateReviewDto
    {
        [Required]
        [MaxLength(1000)]
        public string ReviewText { get; set; } = "";
    }
}
