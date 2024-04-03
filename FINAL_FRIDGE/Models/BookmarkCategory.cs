using FINAL_FRIDGE.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace FINAL_FRIDGE.Models
{

    public class BookmarkCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // cheie primara compusa (Id, BookmarkId, CategoryId)
        public int Id { get; set; }
        public int? BookmarkId { get; set; }

        public int? CategoryId { get; set; }

        public virtual Bookmark? Bookmark { get; set; }
        public virtual Category? Category { get; set; }

        public DateTime CategoryDate { get; set; }
    }
}
