using System.Collections.Generic;

namespace InventoryApp.ViewModels.Products
{
    public class ProductIndexViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
