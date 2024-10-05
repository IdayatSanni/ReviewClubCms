using ReviewClubCms.Models;
using System.ComponentModel.DataAnnotations;

public class Reviewer
{
    [Key]
    public int ReviewersId { get; set; }

    [Required]
    public string ReviewersName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string ReviewersEmail { get; set; } = "";

    // A review can review many books
    public ICollection<ReviewerBook>? ReviewerBooks { get; set; }

    // A reviewer can write many reviews
    public ICollection<Review>? Reviews { get; set; }
}
