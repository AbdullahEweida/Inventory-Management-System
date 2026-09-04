using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Product
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }
        [Required()]
        [Range(typeof(decimal), "1", "9999999999999999.99", ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        [Required(ErrorMessage = "Low stock threshold is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Low stock threshold must be greater than 0")]
        public int LowStockThreshold { get; set; }
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Stock quantity must be greater than 0")]
        public int StockQuantity { get; set; }
        // foreign key
        public Guid? CategoryID { get; set; }

        // navigation properties
        public Category? Category { get; set; }
        public List<Sale_Item> Sale_Items { get; set; }
        public List<Purchase_Item> Purchase_Items { get; set; }
    }
}
