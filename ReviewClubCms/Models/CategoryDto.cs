using System.ComponentModel.DataAnnotations;

namespace ReviewClubCms.Models
{
    public class CategoryDto
    {
        public int Id { get; set; }
        [Required, MaxLength(25)]
        public string BookCategory { get; set; } = "";
    }
}
