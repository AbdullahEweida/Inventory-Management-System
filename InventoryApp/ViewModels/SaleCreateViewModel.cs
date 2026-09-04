using System.ComponentModel.DataAnnotations;

namespace InventoryApp.ViewModels
{
    public class SaleCreateViewModel
    {
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Customer information is required")]
        [MaxLength(100, ErrorMessage = "Customer information cannot exceed 100 characters")]
        public string CustomerInfo { get; set; }

        public List<SaleItemViewModel> Items { get; set; } = new();
    }

    public class SaleItemViewModel
    {
        [Required]
        public Guid ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitCost { get; set; }
    }
}
