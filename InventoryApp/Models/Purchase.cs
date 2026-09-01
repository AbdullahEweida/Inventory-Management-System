using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryApp.Models
{
    public class Purchase
    {
        [Key]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "The Total Price is required")]
        [Range(typeof(decimal), "1", "9999999999999999.99", ErrorMessage = "Total Price must be a positive number")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        // foreign key
        [ForeignKey("Supplier")]
        public Guid SupplierID { get; set; }
        // navigation property
        public Supplier Supplier { get; set; }
        public List<Purchase_Item> PurchaseItems { get; set; }
        //derived Attribute
        [NotMapped]
        public decimal TotalPrice {
            get {
                if(PurchaseItems == null || !PurchaseItems.Any()) return 0;
                return PurchaseItems.Sum(x => x.Quantity * x.UnitPrice);
            }
        }
    }
}
