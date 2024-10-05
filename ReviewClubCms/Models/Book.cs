using ReviewClubCms.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Book
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string BookName { get; set; } = "";

    [MaxLength(50)]
    public string BookAuthor { get; set; } = "";

    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BookPicture { get; set; } = "";

    [Required]
    public bool IsBookOfTheMonth { get; set; }

    public virtual Category Category { get; set; } = new();

    // Many-to-many relationship with Reviewers
    public ICollection<ReviewerBook>? ReviewerBooks { get; set; }
}
