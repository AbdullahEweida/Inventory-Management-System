using InventoryApp.Models;

namespace InventoryApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStockQuantity { get; set; }
        public int LowStockProductsCount { get; set; }

        public int TotalPurchasesCount { get; set; }
        public decimal TotalPurchasesAmount { get; set; }

        public int TotalSalesCount { get; set; }
        public decimal TotalSalesRevenue { get; set; }

        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
        public List<MostSoldProductViewModel> MostSoldProducts { get; set; } = new();
    }

    public class RecentActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Details { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class MostSoldProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}