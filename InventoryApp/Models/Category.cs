using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Category
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [MaxLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        public string Description { get; set; }
        // navigation property
        public List<Product> Products { get; set; }
    }
}
