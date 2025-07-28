using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace server.Services;
public class MailerService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "mlsn.0036ab0e09377a97fcf4ff6d74183e93eb9a4cf729ea484fd94503ca5901f007"; // Zamień na swój API token
    private const string SenderEmail = "noreply@test-3m5jgrodk3xgdpyo.mlsender.net"; // zweryfikowany sender

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
    public async Task SendStatusChangeNotificationAsync(string userEmail, string userName, string requestTitle, string newStatus, string? reason = null)
    {
        var subject = $"Zmiana statusu zgłoszenia: {requestTitle}";
        var text = $"Witaj {userName},\n\n" +
                   $"Status Twojego zgłoszenia \"{requestTitle}\" został zmieniony na: {newStatus}\n" +
                   (string.IsNullOrEmpty(reason) ? "" : $"Powód: {reason}\n") +
                   $"\nZespół BuyGuard";

        await SendEmailAsync(userEmail, subject, text);
    }

    public async Task SendNewRequestNotificationAsync(string managerEmail, string managerName, string requestTitle, string userName, decimal amount, string description)
    {
        var subject = $"Nowe zgłoszenie do zatwierdzenia: {requestTitle}";
        var text = $"Witaj {managerName},\n\n" +
                   $"Otrzymałeś nowe zgłoszenie do zatwierdzenia:\n" +
                   $"Tytuł: {requestTitle}\n" +
                   $"Od: {userName}\n" +
                   $"Kwota: {amount} PLN\n" +
                   $"Opis: {description}\n\n" +
                   $"Zaloguj się do systemu BuyGuard aby je przejrzeć.\n\n" +
                   $"Zespół BuyGuard";

        await SendEmailAsync(managerEmail, subject, text);
    }

    public async Task SendNoteAddedNotificationAsync(string recipientEmail, string recipientName, string requestTitle, string noteAuthor, string noteContent)
    {
        var subject = $"Nowa notatka w zgłoszeniu: {requestTitle}";
        var text = $"Witaj {recipientName},\n\n" +
                   $"Dodano nową notatkę do zgłoszenia \"{requestTitle}\":\n" +
                   $"Autor: {noteAuthor}\n" +
                   $"Treść: {noteContent}\n\n" +
                   $"Zaloguj się do systemu BuyGuard aby zobaczyć szczegóły.\n\n" +
                   $"Zespół BuyGuard";

        await SendEmailAsync(recipientEmail, subject, text);
    }
}
