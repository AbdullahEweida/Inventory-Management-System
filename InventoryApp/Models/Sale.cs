using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryApp.Models
{
    public class Sale
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Customer info is required")]
        [MaxLength(100, ErrorMessage = "Customer info must be less than 100 characters")]
        public string CustomerInfo { get; set; }
        // navigating prop
        public List<Sale_Item> Sale_Items { get; set; } = new();
        // derived prop
        [NotMapped]
        public decimal TotalAmount { get { 
                if(Sale_Items is null || !Sale_Items.Any()) return 0;
                return Sale_Items.Sum(x => x.Quantity * x.UnitCost);
            } 
        }
    }
}
