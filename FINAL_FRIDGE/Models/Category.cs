using FINAL_FRIDGE.Models;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace FINAL_FRIDGE.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele colectiei este obligatoriu")]
        public string Name { get; set; }

        [StringLength(110, ErrorMessage = "Note-ul poate sa aiba maxim 110 de caractere")]
        public string? Note { get; set; }

        // o colectie este creata de catre un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // relatia many-to-many dintre Bookmark si Category
        public virtual ICollection<BookmarkCategory>? BookmarkCategories { get; set; }

    }
}
