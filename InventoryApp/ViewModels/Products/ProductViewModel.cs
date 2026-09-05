using System.ComponentModel.DataAnnotations;

namespace InventoryApp.ViewModels.Products
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, 999999999999999.99, ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Stock quantity must be greater than 0")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Low stock threshold is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Low stock threshold must be greater than 0")]
        public int LowStockThreshold { get; set; }

        public Guid? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}