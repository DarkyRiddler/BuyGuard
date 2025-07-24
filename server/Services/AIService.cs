using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using server.Data;
using System.Text.Json.Serialization;

namespace server.Services
{
    public interface IAIService
    {
        Task<double> EvaluateProductUsefulness(string title, string description, string reason, decimal amount, string companyContext);
        Task GenerateAIScoreForRequest(int requestId, ApplicationDbContext context);
        Task<int> GenerateMissingAIScores(ApplicationDbContext context);
    }

    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;   
		private const string ApiKey = "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";

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<double> EvaluateProductUsefulness(string title, string description, string reason, decimal amount, string companyContext)
        {
            try
            {
                var prompt = BuildPrompt(title, description, reason, amount, companyContext);
                var score = await CallLLMService(prompt);
                return Math.Clamp(score, 0.0, 10.0);
            }
            catch (Exception ex)
            {
                return 5.0;
            }
        }

        private async Task<double> CallLLMService(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = "google/gemma-3-27b-it:free",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
                request.Content = content;
        
                request.Headers.Add("Authorization", $"Bearer {ApiKey}");
                request.Headers.Add("HTTP-Referer", "http://localhost:5000");
                request.Headers.Add("X-Title", "BuyGuard");
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return 5.0;
                }
                
                
                try
                {
                    var aiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
                    var resultText = aiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "5.0";
                    var parsedScore = ParseScoreFromResponse(resultText);
                    return parsedScore;
                }
                catch (JsonException jsonEx)
                {
                    return 5.0;
                }
            }
            catch (HttpRequestException httpEx)
            {
                return 5.0;
            }
            catch (Exception ex)
            {
                return 5.0;
            }
        }

        private double ParseScoreFromResponse(string response)
        {
            try
            {
                var cleaned = response.Trim();

                if (double.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var score))
                {
                    return score;
                }
                return 5.0;
            }
            catch (Exception ex)
            {
                return 5.0;
            }
        }
        
        public async Task GenerateAIScoreForRequest(int requestId, ApplicationDbContext context)
        {
            try
            {
                
                var request = await context.Request.FindAsync(requestId);
                if (request == null)
                {
                    return;
                }
                
                if (request.AiScore.HasValue)
                {
                    return;
                }

                var companySettings = await context.CompanySettings.FirstOrDefaultAsync();
                var companyContext = companySettings?.CompanyDescription ?? GetDefaultCompanyContext();

                var aiScore = await EvaluateProductUsefulness(
                    request.Title,
                    request.Description,
                    request.Reason,
                    request.AmountPln,
                    companyContext
                );

                request.AiScore = aiScore;
                request.AiScoreGeneratedAt = DateTime.UtcNow;

                context.Update(request);
                await context.SaveChangesAsync();
                
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<int> GenerateMissingAIScores(ApplicationDbContext context)
        {
            try
            {
                var requestsWithoutAI = await context.Request
                    .Where(r => r.AiScore == null)
                    .ToListAsync();
                

                if (requestsWithoutAI.Count == 0)
                {
                    return 0;
                }

                var companySettings = await context.CompanySettings.FirstOrDefaultAsync();
                var companyContext = companySettings?.CompanyDescription ?? GetDefaultCompanyContext();

                var successCount = 0;

                foreach (var request in requestsWithoutAI)
                {
                    try
                    {
                        
                        var aiScore = await EvaluateProductUsefulness(
                            request.Title,
                            request.Description,
                            request.Reason,
                            request.AmountPln,
                            companyContext
                        );

                        request.AiScore = aiScore;
                        request.AiScoreGeneratedAt = DateTime.UtcNow;
                        successCount++;
                        
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                await context.SaveChangesAsync();

                return successCount;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private string GetDefaultCompanyContext()
        {
            return "No context provided, evaluate with random score :33";
        }

        private string BuildPrompt(string title, string description, string reason, decimal amount, string companyContext)
        {
            var prompt = $@"
            You are an AI assistant helping evaluate the usefulness of product purchase requests for a company.

            Company Context: {companyContext}

            Product Request:
            - Title: {title}
            - Description: {description}
            - Reason: {reason}
            - Amount: {amount} PLN

            Evaluate this product request and provide a usefulness score from (0.0 to 10.0) based on the following criteria:
            1. **Business alignment** – Does it support company goals or mission?
			2. **Cost-benefit analysis** – Are the expected benefits worth the cost? Are there better alternatives?
			3. **Necessity** – Is it essential for the requester’s work or operations?
			4. **Productivity impact** – Will it significantly enhance work efficiency or output?
			5. **Strategic value** – Does it contribute to long-term goals, innovation, or competitive advantage?

            Respond with ONLY a number between 0.0 and 10.0 (e.g., 7.5), where:
			- 0.0 = not useful at all
			- 5.0 =  moderately useful
			- 10.0 = extremely useful
            ";
            
            return prompt;
        }

        private class OpenAIResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();
        }

        private class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; } = new();
        }

        private class Message
        {
            [JsonPropertyName("content")]
            public string Content { get; set; } = "";
        }
    }
}