using server.Models;
using server.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.User.Any()) return;

        // Admin
        context.User.Add(new User
        {
            Email = "pszybiak@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma"),
            FirstName = "Piotr",
            LastName = "Szybiak",
            Role = "admin",
            ManagerLimitPln = null
        });

        // Managers
        var managers = new List<User>
        {
            new User { Email = "dwisniowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("sigma"), FirstName = "Dawid", LastName = "Wiśniowski", Role = "manager", ManagerLimitPln = 100000 },
            new User { Email = "jnowak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1"), FirstName = "Jan", LastName = "Nowak", Role = "manager", ManagerLimitPln = 75000 },
            new User { Email = "amalinowska@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2"), FirstName = "Anna", LastName = "Malinowska", Role = "manager", ManagerLimitPln = 50000 },
            new User { Email = "kzielinski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password3"), FirstName = "Krzysztof", LastName = "Zieliński", Role = "manager", ManagerLimitPln = 80000 },
            new User { Email = "pnowicka@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password4"), FirstName = "Paulina", LastName = "Nowicka", Role = "manager", ManagerLimitPln = 65000 }
        };
        context.User.AddRange(managers);

        // User
        var employees = new List<User>
        {
            new User { Email = "mwisniowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234567890"), FirstName = "Marcin", LastName = "Wiśniowski", Role = "employee" },
            new User { Email = "kwegrzyn@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("asthurfirullah"), FirstName = "Kamil", LastName = "Węgrzyn", Role = "employee" },
            new User { Email = "nslupska@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("kwiatki"), FirstName = "Nikola", LastName = "Słupska", Role = "employee" },
            new User { Email = "tgrabowski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass234"), FirstName = "Tomasz", LastName = "Grabowski", Role = "employee" },
            new User { Email = "mkowalski@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass345"), FirstName = "Michał", LastName = "Kowalski", Role = "employee" },
            new User { Email = "aszewczyk@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass456"), FirstName = "Agnieszka", LastName = "Szewczyk", Role = "employee" },
            new User { Email = "jkrzak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass567"), FirstName = "Jakub", LastName = "Krzak", Role = "employee" },
            new User { Email = "bchmiel@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass678"), FirstName = "Bartłomiej", LastName = "Chmiel", Role = "employee" },
            new User { Email = "swojcik@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass789"), FirstName = "Sylwia", LastName = "Wójcik", Role = "employee" },
            new User { Email = "rnowak@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass890"), FirstName = "Robert", LastName = "Nowak", Role = "employee" }
        };
        context.User.AddRange(employees);
        context.SaveChanges();

        var managers_list = context.User.Where(u => u.Role == "manager").ToList();
        var employees_list = context.User.Where(u => u.Role == "employee").ToList();
        
        var requests = new List<Request>
        {
            new Request
            {
                UserId = employees[0].Id,
                ManagerId = managers[0].Id,
                Title = "Zakup nowego laptopa",
                Description = "Laptop machen.",
                AmountPln = 4500,
                Reason = "Stary laptop wylądował na ścianie po przegranej w lola.",
                Status = "czeka",
                AiScore = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Attachments = new List<Attachment>(),
                Notes = new List<Note>()
            },
            new Request
            {
                UserId = employees[1].Id,
                ManagerId = managers[1].Id,
                Title = "Szkolenie z zarządzania projektami",
                Description = "Chciałbym wziąć udział w szkoleniu.",
                AmountPln = 1500,
                Reason = "Podniesienie kwalifikacji zawodowych.",
                Status = "potwierdzono",
                AiScore = null,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
                Attachments = new List<Attachment>(),
                Notes = new List<Note>()
            }
        };
        context.Request.AddRange(requests);
        context.SaveChanges();
        
        // Attachments - do pierwszego requestu
        var attachments = new List<Attachment>
        {
            new Attachment
            {
                RequestId = requests[0].Id,
                FileUrl = "https://example.com/invoice.pdf",
                MimeType = "application/pdf",
                Request = requests[0]
            },
            new Attachment
            {
                RequestId = requests[0].Id,
                FileUrl = "https://example.com/specs.docx",
                MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                Request = requests[0]
            }
        };
        context.Attachment.AddRange(attachments);
        
        var notes = new List<Note>
        {
            new Note
            {
                RequestId = requests[0].Id,
                AuthorId = employees[0].Id,
                Body = "Proszę o szybkie rozpatrzenie, bo nie moge graz w lola.",
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                Request = requests[0],
                Author = employees[0]
            },
            new Note
            {
                RequestId = requests[0].Id,
                AuthorId = managers[0].Id,
                Body = "Nie ma kasy niggdy nie bylo",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                Request = requests[0],
                Author = managers[0]
            },
            new Note
            {
                RequestId = requests[1].Id,
                AuthorId = employees[1].Id,
                Body = "Czekam na akceptację.",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                Request = requests[1],
                Author = employees[1]
            }
        };
        context.Note.AddRange(notes);

        context.SaveChanges();
    }
}
