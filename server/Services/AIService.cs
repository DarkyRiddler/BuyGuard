using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using server.Data;
using Microsoft.EntityFrameworkCore;

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
            return $@"
            You are an AI assistant helping evaluate the usefulness of product purchase requests for a company.

            Company Context: {companyContext}

            Product Request:
            - Title: {title}
            - Description: {description}
            - Reason: {reason}
            - Amount: {amount} PLN

            Please evaluate this product request and provide a usefulness score from 0.0 to 10.0 based on:
            1. Business alignment with company goals
            2. Cost-benefit analysis
            3. Necessity for operations
            4. Potential productivity impact
            5. Strategic value

            Respond with ONLY a number between 0.0 and 10.0 (e.g., 7.5).
            ";
        }

        private async Task<double> CallLLMService(string prompt)
        {
            var requestBody = new
            {
                model = "deepseek/deepseek-r1-0528:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://yourdomain.com");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "YourAppName");

            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return 5.0;
            }
            
            var aiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);
            var resultText = aiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "5.0";

            return ParseScoreFromResponse(resultText);
        }

        private double ParseScoreFromResponse(string response)
        {
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(response, @"\d+\.?\d*");
                if (match.Success && double.TryParse(match.Value, out var score))
                {
                    return score;
                }
                return 5.0;
            }
            catch
            {
                return 5.0;
            }
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