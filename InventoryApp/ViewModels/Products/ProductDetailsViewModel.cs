using System;

namespace InventoryApp.ViewModels.Products
{
    public class ProductDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public Guid? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public bool IsLowStock => StockQuantity <= LowStockThreshold;
    }
}