using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class MailerService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "mlsn.552e7867fcd52e4fec83a5b1995e2e5fe12a9f98a36537f4840c707ddca7b2d9"; // Zamień na swój API token
    private const string SenderEmail = "test-3m5jgrod8kdgdpyo.mlsender.net"; // zweryfikowany sender

    public MailerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.mailersend.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string text)
    {
        var payload = new
        {
            from = new { email = SenderEmail },
            to = new[] { new { email = toEmail } },
            subject,
            text
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("email", content);
        response.EnsureSuccessStatusCode(); // rzuci wyjątek jak status != 2xx
    }
}
