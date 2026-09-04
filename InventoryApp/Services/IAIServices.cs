using InventoryApp.Models.AI;

namespace InventoryApp.Services
{
    public interface IAIService
    {
        Task<string> AnalyzeInventoryAsync();
    }
}