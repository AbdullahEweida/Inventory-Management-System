namespace InventoryApp.Models.AI
{


    public class ProductAIData
    {
        //this new class data for the ai (used only the important data)
        public string ?Name { get; set; } 
        public string ?SKU { get; set; } 
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
    }
}