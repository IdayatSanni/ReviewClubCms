using System.ComponentModel.DataAnnotations;

namespace ReviewClubCms.Models
{
    public class ReviewerDto
    {
        public int ReviewersId { get; set; }

        [Required]
        public string ReviewersName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string ReviewersEmail { get; set; } = "";

        public int ReviewedBookCount { get; set; } 
    }
}
