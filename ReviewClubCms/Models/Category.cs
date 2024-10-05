using System.ComponentModel.DataAnnotations;

namespace ReviewClubCms.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(25)]
        public string BookCategory { get; set; } = "";
        //A category can be applied to multiple books
        public virtual ICollection<Book>? Books { get; set; }
    }
}
