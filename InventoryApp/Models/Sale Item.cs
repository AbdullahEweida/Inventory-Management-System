using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Models
{
    public class Sale_Item
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Unit Cost is required")]
        [Range(typeof(decimal), "1", "9999999999999999.99", ErrorMessage = "Unit Cost must be greater than 0")]
        public decimal UnitCost { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        //foriegn keys
        public Guid SaleID { get; set; }
        public Guid ProductID { get; set; }
        //navigating properties
        public Sale Sale { get; set; }
        public Product Product { get; set; }
    }
}
