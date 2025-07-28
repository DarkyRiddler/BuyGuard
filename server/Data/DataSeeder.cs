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
            Email = "pszybiak@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma1"),
            FirstName = "Piotr",
            LastName = "Szybiak",
            Role = "admin"
        };
        context.User.Add(admin);

        // Managers
       var managers = new List<User>
        {
            new() { Email = "dwisniowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma2"), FirstName = "Dawid", LastName = "Wiśniowski", Role = "manager", ManagerLimitPln = 40000 },
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
                Url = "https://example.com/laptop",
                Reason = "Stary laptop wylądował na ścianie po przegranej w lola.",
                Status = "czeka",
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
                Url = "https://example.com/szkolenie",
                Reason = "Podniesienie kwalifikacji zawodowych.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[2].Id,
                ManagerId = dbEmployees[2].ManagerId ?? dbManagers[2].Id,
                Title = "Monitor 27 cali",
                Description = "Do pracy z arkuszami Excela.",
                AmountPln = 1200,
                Url = "https://example.com/monitor",
                Reason = "Poprawa komfortu pracy.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[3].Id,
                ManagerId = dbEmployees[3].ManagerId ?? dbManagers[2].Id,
                Title = "Abonament Adobe",
                Description = "Pakiet Adobe do projektów graficznych.",
                AmountPln = 2400,
                Url = "https://example.com/adobe",
                Reason = "Potrzebne do tworzenia materiałów reklamowych.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[4].Id,
                ManagerId = dbEmployees[4].ManagerId ?? dbManagers[3].Id,
                Title = "Nowe biurko",
                Description = "Biurko regulowane do pracy stojącej.",
                AmountPln = 1800,
                Url = "https://example.com/biurko",
                Reason = "Zdrowie kręgosłupa.",
                Status = "potwierdzono",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[5].Id,
                ManagerId = dbEmployees[5].ManagerId ?? dbManagers[3].Id,
                Title = "Oprogramowanie księgowe",
                Description = "Licencja na program księgowy.",
                AmountPln = 3000,
                Url = "https://example.com/ksiegowe",
                Reason = "Automatyzacja procesów rozliczeń.",
                Status = "odrzucono",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-19),
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[6].Id,
                ManagerId = dbEmployees[6].ManagerId ?? dbManagers[3].Id,
                Title = "Szkolenie z bezpieczeństwa IT",
                Description = "Zewnętrzne szkolenie z cyberbezpieczeństwa.",
                AmountPln = 950,
                Url = "https://example.com/szkolenie-it",
                Reason = "Podniesienie bezpieczeństwa danych.",
                Status = "zakupione",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[7].Id,
                ManagerId = dbEmployees[7].ManagerId ?? dbManagers[4].Id,
                Title = "Router sieciowy",
                Description = "Router do poprawy jakości sieci w biurze.",
                AmountPln = 700,
                Url = "https://example.com/router",
                Reason = "Zrywanie połączenia.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[8].Id,
                ManagerId = dbEmployees[8].ManagerId ?? dbManagers[4].Id,
                Title = "Materiały biurowe",
                Description = "Papier, długopisy, segregatory.",
                AmountPln = 300,
                Url = "https://example.com/materialy-biurowe",
                Reason = "Wyposażenie stanowiska.",
                Status = "potwierdzono",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-11),
                Attachments = [],
                Notes = []
            },
            new()
            {
                UserId = dbEmployees[9].Id,
                ManagerId = dbEmployees[9].ManagerId ?? dbManagers[4].Id,
                Title = "Nowy telefon służbowy",
                Description = "Smartfon do kontaktu z klientami.",
                AmountPln = 2000,
                Url = "https://example.com/telefon",
                Reason = "Stary telefon przestał działać.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null,
                Attachments = [],
                Notes = []
            }
        };
        context.Request.AddRange(requests);
        context.SaveChanges();

       var attachments = new List<Attachment>
        {
            new() { RequestId = requests[0].Id, FileUrl = "/uploads/attachments/zdjecie1.png", MimeType = "image/png" },
            new() { RequestId = requests[0].Id, FileUrl = "/uploads/attachments/zdjecie2.jpeg", MimeType = "image/jpeg" }
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
