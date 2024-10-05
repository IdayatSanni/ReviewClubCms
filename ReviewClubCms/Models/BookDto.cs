using ReviewClubCms.Models;
using System.ComponentModel.DataAnnotations;

public class BookDto
{
    public int? Id { get; set; }
    [Required, MaxLength(50)]
    public string BookName { get; set; } = "";

    [Required, MaxLength(50)]
    public string BookAuthor { get; set; } = "";

    [Required]
    public int CategoryId { get; set; } 

    [Required]
    public bool IsBookOfTheMonth { get; set; }

    
}
