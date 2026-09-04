using InventoryApp.ViewModels;
using System.Collections.Generic;

namespace InventoryApp.ViewModels
{
    public class CategoryIndexViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}