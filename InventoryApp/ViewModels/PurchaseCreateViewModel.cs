using System.ComponentModel.DataAnnotations;

namespace InventoryApp.ViewModels
{
    public class PurchaseCreateViewModel
    {
        [Required]
        public Guid SupplierID { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public List<PurchaseItemViewModel> Items { get; set; } = new();
    }

    public class PurchaseItemViewModel
    {
        [Required]
        public Guid ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ErrorMessage = "Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }
}
