namespace FINAL_FRIDGE.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int? BookmarkId { get; set; }    
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Bookmark? Bookmark { get; set; }

    }

}
