using InventoryApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace InventoryApp.Services
{
    public class AIService : IAIService
    {
        // Database context used to read inventory products
        private readonly InventoryDbContext _db;

        // Used to read the Gemini API key from appsettings.json
        private readonly IConfiguration _configuration;

        // HttpClient used to send requests to Gemini API
        private readonly HttpClient _httpClient;

        public AIService(
            InventoryDbContext db,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _db = db;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> AnalyzeInventoryAsync()
        {
            try
            {
                // Get Gemini API key from configuration
                var apiKey = _configuration["Gemini:ApiKey"];

                // Make sure the API key exists
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "Gemini API Key is missing."
                    });
                }

                // Get the required product information from the database
                var products = await _db.Products
                    .Select(p => new
                    {
                        name = p.Name,
                        sku = p.SKU,
                        stockQuantity = p.StockQuantity,
                        lowStockThreshold = p.LowStockThreshold
                    })
                    .ToListAsync();

                // Handle empty inventory
                if (products.Count == 0)
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = true,
                        data = new
                        {
                            summary = new
                            {
                                totalProducts = 0,
                                healthyProducts = 0,
                                lowStockProducts = 0,
                                criticalProducts = 0,
                                productsNeedingRestock = 0,
                                overallStatus = "Healthy"
                            },

                            products = Array.Empty<object>(),
                            priorityActions = Array.Empty<string>(),
                            generalRecommendations = Array.Empty<string>()
                        }
                    });
                }

                // Calculate stock status on the backend.
                // This is deterministic and should NOT be changed by Gemini.
                var inventory = products.Select(p =>
                {
                    string status;

                    // No stock at all = Critical
                    if (p.stockQuantity <= 0)
                    {
                        status = "Critical";
                    }
                    // Stock is below the configured threshold = Low
                    else if (p.stockQuantity < p.lowStockThreshold)
                    {
                        status = "Low";
                    }
                    // Otherwise the stock is healthy
                    else
                    {
                        status = "Healthy";
                    }

                    return new
                    {
                        p.name,
                        p.sku,
                        p.stockQuantity,
                        p.lowStockThreshold,

                        // Backend-generated status
                        status,

                        // Backend decides whether restocking is needed
                        needsRestocking = p.stockQuantity < p.lowStockThreshold
                    };
                }).ToList();

                // Calculate inventory summary on the backend
                var totalProducts = inventory.Count;

                var healthyProducts =
                    inventory.Count(x => x.status == "Healthy");

                var lowStockProducts =
                    inventory.Count(x => x.status == "Low");

                var criticalProducts =
                    inventory.Count(x => x.status == "Critical");

                var productsNeedingRestock =
                    inventory.Count(x => x.needsRestocking);

                // Determine overall inventory status
                string overallStatus;

                if (criticalProducts > 0)
                {
                    overallStatus = "Critical";
                }
                else if (lowStockProducts > 0)
                {
                    overallStatus = "Low";
                }
                else
                {
                    overallStatus = "Healthy";
                }

                // Convert inventory data to JSON
                // This JSON will be sent to Gemini inside the prompt
                var inventoryJson = JsonSerializer.Serialize(
                    inventory,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                // Build the Gemini prompt
                // Important: we are NOT embedding a JSON object directly
                // inside the interpolated string, avoiding brace/interpolation issues.
                // Build the Gemini prompt (Smart Analytical Version)
                // Build the Gemini prompt (Smart Analytical Version)
                var prompt = $$"""
You are an Expert Supply Chain and Retail Business Analyst. 
Review the following inventory data carefully.

Inventory data:
{{inventoryJson}}

IMPORTANT RULES FOR YOUR ANALYSIS:
1. DO NOT give me basic logical statements like "Stock is low, reorder". The database already does that.
2. Provide STRATEGIC INSIGHTS: Identify potential "dead stock" (slow-moving items), suggest bundle offers, or recommend dynamic pricing/discounts to clear out excess inventory.
3. For products that need restocking, suggest a specific action (e.g., "Reorder 50 units to meet baseline demand").
4. Keep the exact "name", "sku", "stockQuantity", and "status" as provided. Do not invent products.

Return ONLY valid JSON.
Do NOT use Markdown fences like ```json.
Do NOT add any text outside the JSON object.

The response MUST perfectly match this JSON structure:
{
  "summary": {
    "totalProducts": {{totalProducts}},
    "healthyProducts": {{healthyProducts}},
    "lowStockProducts": {{lowStockProducts}},
    "criticalProducts": {{criticalProducts}}
  },
  "priorityActions": [
    "Write 2-3 URGENT business actions here. Be specific. (e.g., 'Expedite shipping for Product X to prevent revenue loss due to stockout.')"
  ],
  "products": [
    // Return all {{totalProducts}} products here.
    // Ensure each product has: name, sku, stockQuantity, status.
    // PLUS a "recommendation" field containing your smart business insight for that specific item.
  ],
  "generalRecommendations": [
    "Write 2-3 strategic long-term tips.",
    "Draft a short, professional Email template to a supplier requesting an urgent restock for the critical items."
  ]
}
""";          
                var requestBody = new
                {
                    model = "gemini-3.8-flash",
                    input = prompt
                };

                // Convert request body to JSON
                var requestJson = JsonSerializer.Serialize(requestBody);

                // Create POST request to Gemini Interactions API
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://generativelanguage.googleapis.com/v1beta/interactions"
                );

                // Add Gemini API key to request header
                request.Headers.Add("x-goog-api-key", apiKey);

                // Add JSON body
                request.Content = new StringContent(
                    requestJson,
                    Encoding.UTF8,
                    "application/json"
                );

                // Send request to Gemini
                using var response =
                    await _httpClient.SendAsync(request);

                // Read raw Gemini response
                var responseContent =
                    await response.Content.ReadAsStringAsync();

                // Handle Gemini API errors
                if (!response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error =
                            $"Gemini API Error ({(int)response.StatusCode}): {responseContent}"
                    });
                }

                // Parse Gemini response
                using var geminiDocument =
                    JsonDocument.Parse(responseContent);

                string? generatedText = null;

                // Gemini Interactions API returns model output
                // inside the "steps" array.
                if (geminiDocument.RootElement.TryGetProperty(
                    "steps",
                    out var steps))
                {
                    foreach (var step in steps.EnumerateArray())
                    {
                        // Make sure the step contains "content"
                        if (!step.TryGetProperty(
                            "content",
                            out var content))
                        {
                            continue;
                        }

                        // Search for text inside content
                        foreach (var contentItem in content.EnumerateArray())
                        {
                            if (contentItem.TryGetProperty(
                                "text",
                                out var text))
                            {
                                generatedText = text.GetString();

                                // Stop once we find the generated text
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(generatedText))
                        {
                            break;
                        }
                    }
                }

                // Gemini returned no usable text
                if (string.IsNullOrWhiteSpace(generatedText))
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "Gemini returned an empty response.",
                        rawResponse = responseContent
                    });
                }

                // Remove leading/trailing whitespace
                generatedText = generatedText.Trim();

                // Gemini sometimes returns JSON wrapped in Markdown fences.
                // Remove them just in case.
                if (generatedText.StartsWith("```json"))
                {
                    generatedText =
                        generatedText.Substring(7).Trim();
                }
                else if (generatedText.StartsWith("```"))
                {
                    generatedText =
                        generatedText.Substring(3).Trim();
                }

                // Remove closing Markdown fence
                if (generatedText.EndsWith("```"))
                {
                    generatedText = generatedText.Substring(
                        0,
                        generatedText.Length - 3
                    ).Trim();
                }

                // Validate that Gemini actually returned valid JSON
                using var resultJson =
                    JsonDocument.Parse(generatedText);

                // Return a clean wrapper to the controller/frontend
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    data = resultJson.RootElement.Clone()
                });
            }
            catch (TaskCanceledException)
            {
                // Usually happens when HttpClient times out
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "The Gemini request timed out."
                });
            }
            catch (HttpRequestException ex)
            {
                // Handle network / HTTP errors
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"HTTP Error: {ex.Message}"
                });
            }
            catch (JsonException ex)
            {
                // Handle invalid JSON from Gemini
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error =
                        $"Invalid JSON returned by Gemini: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"Unexpected Error: {ex.Message}"
                });
            }
        }
    }
}