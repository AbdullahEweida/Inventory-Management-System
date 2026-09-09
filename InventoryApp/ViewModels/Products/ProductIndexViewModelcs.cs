using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace InventoryApp.ViewModels.Products
{
    public class ProductIndexViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }





        public string? CurrentSearch { get; set; }
        public Guid? CurrentCategory { get; set; }
        public string? CurrentStockStatus { get; set; }
        public List<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

    }
}
