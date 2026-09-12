using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Supplier
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [MaxLength(100, ErrorMessage = "Address must be less than 100 characters")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(50, ErrorMessage = "Phone number must be less than 50 characters")]
        public string Phone { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Example: name@example.com")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Contact name is required")]
        [MaxLength(50, ErrorMessage = "Contact name must be less than 50 characters")]
        public string ContactName { get; set; }
        // navigation properties
        public List<Purchase>? Purchases { get; set; }
    }
}