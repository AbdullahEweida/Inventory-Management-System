using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryApp.Models
{
    public class Purchase_Item
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        [Range(typeof(decimal), "1", "9999999999999999.99", ErrorMessage ="Unit Price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        //foreign keys
        [ForeignKey("Product")]
        public Guid ProductID { get; set; }
        [ForeignKey("Purchase")]
        public Guid PurchaseID { get; set; }
        //navigation properties
        public Product Product { get; set; }
        public Purchase Purchase { get; set; }
    }
}
