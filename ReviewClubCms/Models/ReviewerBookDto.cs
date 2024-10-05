namespace ReviewClubCms.Models
{
    public class ReviewerBookDto
    {
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = ""; 

        public int BookId { get; set; }
        public string BookTitle { get; set; } = ""; 
    }
}
