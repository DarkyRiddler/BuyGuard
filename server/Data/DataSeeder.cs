using server.Models;
using server.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.User.Any())
        {
            // Usuń zależności w odpowiedniej kolejności
            context.Attachment.RemoveRange(context.Attachment);
            context.Note.RemoveRange(context.Note);
            context.Request.RemoveRange(context.Request);
            context.User.RemoveRange(context.User);
            context.SaveChanges();
        }
        
        // Admin
        var admin = new User
        {
            Email = "kinnaynni@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma1"),
            FirstName = "Piotr",
            LastName = "Szybiak",
            Role = "admin"
        };
        context.User.Add(admin);

        // Managers
       var managers = new List<User>
        {
            new() { Email = "qtqtmaia@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma2"), FirstName = "Dawid", LastName = "Wiśniowski", Role = "manager", ManagerLimitPln = 40000 }
        };
        context.User.AddRange(managers);
        context.SaveChanges();

        var managerIds = context.User.Where(u => u.Role == "manager").Select(m => m.Id).ToList();

        // Employees
        var employees = new List<User>
        {
            new() { Email = "earthcantbeflat@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234567890"), FirstName = "Marcin", LastName = "Wiśniowski", Role = "employee", ManagerId = managerIds[0] }
        };
        context.User.AddRange(employees);
        context.SaveChanges();

        var dbManagers = context.User.Where(u => u.Role == "manager").ToList();
        var dbEmployees = context.User.Where(u => u.Role == "employee").ToList();
        
        var requests = new List<Request>
        {
            new()
            {
                UserId = dbEmployees[0].Id,
                ManagerId = dbManagers[0].Id,
                Title = "Zakup nowego laptopa",
                Description = "Laptop machen.",
                AmountPln = 4500,
                Url = "https://example.com/laptop",
                Reason = "Stary laptop wylądował na ścianie po przegranej w lola.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            }
        };
        context.Request.AddRange(requests);
        context.SaveChanges();

        var attachments = new List<Attachment>
        {
            new() { RequestId = requests[0].Id, FileUrl = "https://example.com/invoice.pdf", MimeType = "application/pdf" },
            new() { RequestId = requests[0].Id, FileUrl = "https://example.com/specs.docx", MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
        };
        context.Attachment.AddRange(attachments);

        var notes = new List<Note>
        {
            new() {
                RequestId = requests[0].Id,
                AuthorId = dbEmployees[0].Id,
                Body = "Proszę o szybkie rozpatrzenie, bo nie moge grać w lola.",
                CreatedAt = DateTime.UtcNow.AddHours(-10) },
            new() {
                RequestId = requests[0].Id,
                AuthorId = dbManagers[0].Id,
                Body = "Nie ma kasy nigdy nie było",
                CreatedAt = DateTime.UtcNow.AddHours(-5) },
        };
        context.Note.AddRange(notes);

        context.SaveChanges();
    }
}
