using FINAL_FRIDGE.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace FINAL_FRIDGE.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul Bookmarkului este obligatoriu")]
        [StringLength(50, ErrorMessage = "Titlul Bookmarkului poate sa aiba maxim 50 de caractere")]
        [MinLength(1, ErrorMessage = "Titlul Bookmarkului trebuie sa aiba cel putin un caracter")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Bookmarkul trebuie sa aiba continut")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        // un Bookmark poate avea o colectie de comentarii
        public virtual ICollection<Comment>? Comments { get; set; }

        public int TotalLikes { get; set; }
        public virtual ICollection<Like>? Likes { get; set; }

        public string? Image { get; set; }

        // un Bookmark este creat de catre un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // relatia many-to-many dintre Bookmark si Category
        public virtual ICollection<BookmarkCategory>? BookmarkCategories { get; set; }

    }
}
