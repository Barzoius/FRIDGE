using FINAL_FRIDGE.Models;
using System.ComponentModel.DataAnnotations;

namespace FINAL_FRIDGE.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // un comentariu apartine unui bookmark
        public int? BookmarkId { get; set; }

        // un comentariu este postat de catre un user
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Bookmark? Bookmark { get; set; }
    }

}
