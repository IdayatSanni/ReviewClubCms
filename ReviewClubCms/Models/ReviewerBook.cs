namespace ReviewClubCms.Models
{
    public class ReviewerBook
    {
        public int ReviewerId { get; set; }
        public virtual Reviewer Reviewer { get; set; } = new();

        public int BookId { get; set; }
        public virtual Book Book { get; set; } = new();
    }
}
