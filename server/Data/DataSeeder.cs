using server.Models;
using server.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.User.Any()) return;
        
        // Admin
        var admin = new User
        {
            Email = "pszybiak@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma"),
            FirstName = "Piotr",
            LastName = "Szybiak",
            Role = "admin"
        };
        context.User.Add(admin);

        // Managers
       var managers = new List<User>
        {
            new() { Email = "dwisniowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma"), FirstName = "Dawid", LastName = "Wiśniowski", Role = "manager", ManagerLimitPln = 40000 },
            new() { Email = "jnowak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1"), FirstName = "Jan", LastName = "Nowak", Role = "manager", ManagerLimitPln = 60000 },
            new() { Email = "amalinowska@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2"), FirstName = "Anna", LastName = "Malinowska", Role = "manager", ManagerLimitPln = 140000 },
            new() { Email = "kzielinski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password3"), FirstName = "Krzysztof", LastName = "Zieliński", Role = "manager", ManagerLimitPln = 70000 },
            new() { Email = "pnowicka@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password4"), FirstName = "Paulina", LastName = "Nowicka", Role = "manager", ManagerLimitPln = 50000 }
        };
        context.User.AddRange(managers);
        context.SaveChanges();

        var managerIds = context.User.Where(u => u.Role == "manager").Select(m => m.Id).ToList();

        // Employees
        var employees = new List<User>
        {
            new() { Email = "mwisniowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234567890"), FirstName = "Marcin", LastName = "Wiśniowski", Role = "employee", ManagerId = managerIds[0] },
            new() { Email = "kwegrzyn@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("asthurfirullah"), FirstName = "Kamil", LastName = "Węgrzyn", Role = "employee", ManagerId = managerIds[1] },
            new() { Email = "nslupska@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("genshin"), FirstName = "Nikola", LastName = "Słupska", Role = "employee", ManagerId = managerIds[2] },
            new() { Email = "tgrabowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass234"), FirstName = "Tomasz", LastName = "Grabowski", Role = "employee", ManagerId = managerIds[2] },
            new() { Email = "mkowalski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass345"), FirstName = "Michał", LastName = "Kowalski", Role = "employee", ManagerId = managerIds[3] },
            new() { Email = "aszewczyk@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass456"), FirstName = "Agnieszka", LastName = "Szewczyk", Role = "employee", ManagerId = managerIds[3] },
            new() { Email = "jkrzak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass567"), FirstName = "Jakub", LastName = "Krzak", Role = "employee", ManagerId = managerIds[3] },
            new() { Email = "bchmiel@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass678"), FirstName = "Bartłomiej", LastName = "Chmiel", Role = "employee", ManagerId = managerIds[4] },
            new() { Email = "swojcik@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass789"), FirstName = "Sylwia", LastName = "Wójcik", Role = "employee", ManagerId = managerIds[4] },
            new() { Email = "rnowak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass890"), FirstName = "Robert", LastName = "Nowak", Role = "employee", ManagerId = managerIds[4] }
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
                Reason = "Stary laptop wylądował na ścianie po przegranej w lola.",
                Status = "pending",
                AiScore = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[1].Id,
                ManagerId = dbManagers[1].Id,
                Title = "Szkolenie z zarządzania projektami",
                Description = "Chciałbym wziąć udział w szkoleniu.",
                AmountPln = 1500,
                Reason = "Podniesienie kwalifikacji zawodowych.",
                Status = "pending",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
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
            new() {
                RequestId = requests[1].Id,
                AuthorId = dbEmployees[1].Id,
                Body = "Czekam na akceptację.",
                CreatedAt = DateTime.UtcNow.AddDays(-6) }
        };
        context.Note.AddRange(notes);

        context.SaveChanges();
    }
}
