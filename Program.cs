using EquipmentAccounting.Data;
using EquipmentAccounting.Forms;
using EquipmentAccounting.Models;

namespace EquipmentAccounting;

static class Program
{
    [STAThread]
    static void Main()
    {
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated();
            SeedData(context);
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LoginForm());
    }

    private static void SeedData(AppDbContext context)
    {
        // Seed Roles
        if (!context.Roles.Any())
        {
            var adminRole = new Role
            {
                Name = "Администратор",
                Description = "Полный доступ к системе",
                CanManageUsers = true,
                CanManageRoles = true,
                CanCreateRightsOwners = true,
                CanEditRightsOwners = true,
                CanDeleteRightsOwners = true,
                CanCreateFilms = true,
                CanEditFilmBasicInfo = true,
                CanEditFilmRightsInfo = true,
                CanDeleteFilms = true,
                CanManageContacts = true,
                CanManageSchedule = true,
                CanViewContent = true,
                CanViewSchedule = true,
                CanViewContacts = true
            };

            var videoEditorRole = new Role
            {
                Name = "Инженер видеомонтажа",
                Description = "Создание правообладателей и внесение основной информации о фильмах",
                CanManageUsers = false,
                CanManageRoles = false,
                CanCreateRightsOwners = true,
                CanEditRightsOwners = true,
                CanDeleteRightsOwners = false,
                CanCreateFilms = true,
                CanEditFilmBasicInfo = true,
                CanEditFilmRightsInfo = false,
                CanDeleteFilms = false,
                CanManageContacts = false,
                CanManageSchedule = false,
                CanViewContent = true,
                CanViewSchedule = true,
                CanViewContacts = false
            };

            var specialistRole = new Role
            {
                Name = "Специалист",
                Description = "Управление правами на показ и контактами правообладателей",
                CanManageUsers = false,
                CanManageRoles = false,
                CanCreateRightsOwners = false,
                CanEditRightsOwners = false,
                CanDeleteRightsOwners = false,
                CanCreateFilms = false,
                CanEditFilmBasicInfo = false,
                CanEditFilmRightsInfo = true,
                CanDeleteFilms = false,
                CanManageContacts = true,
                CanManageSchedule = false,
                CanViewContent = true,
                CanViewSchedule = true,
                CanViewContacts = true
            };

            var managerRole = new Role
            {
                Name = "Руководитель",
                Description = "Только просмотр информации",
                CanManageUsers = false,
                CanManageRoles = false,
                CanCreateRightsOwners = false,
                CanEditRightsOwners = false,
                CanDeleteRightsOwners = false,
                CanCreateFilms = false,
                CanEditFilmBasicInfo = false,
                CanEditFilmRightsInfo = false,
                CanDeleteFilms = false,
                CanManageContacts = false,
                CanManageSchedule = false,
                CanViewContent = true,
                CanViewSchedule = true,
                CanViewContacts = true
            };

            context.Roles.AddRange(adminRole, videoEditorRole, specialistRole, managerRole);
            context.SaveChanges();
        }

        // Seed Users
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Администратор");
            var videoEditorRole = context.Roles.First(r => r.Name == "Инженер видеомонтажа");
            var specialistRole = context.Roles.First(r => r.Name == "Специалист");
            var managerRole = context.Roles.First(r => r.Name == "Руководитель");

            context.Users.AddRange(
                new User { Login = "admin", Password = "admin", RoleId = adminRole.Id },
                new User { Login = "editor", Password = "editor", RoleId = videoEditorRole.Id },
                new User { Login = "specialist", Password = "specialist", RoleId = specialistRole.Id },
                new User { Login = "manager", Password = "manager", RoleId = managerRole.Id }
            );
            context.SaveChanges();
        }

        // Seed Contacts
        if (!context.Contacts.Any())
        {
            context.Contacts.AddRange(
                new Contact
                {
                    CompanyName = "Беларусьфильм Дистрибуция",
                    Phone = "+375 17 123-45-67",
                    Email = "distribution@belarusfilm.by",
                    Address = "г. Минск, ул. Некрасова, 5",
                    ContactPerson = "Иванов Иван Иванович",
                    Notes = "Официальный дистрибьютор"
                },
                new Contact
                {
                    CompanyName = "Вольга Медиа",
                    Phone = "+7 495 987-65-43",
                    Email = "rights@volga.ru",
                    Address = "г. Москва, ул. Тверская, 10",
                    ContactPerson = "Петров Пётр Петрович",
                    Notes = "Дистрибьютор российского кино"
                },
                new Contact
                {
                    CompanyName = "FPL Distribution",
                    Phone = "+7 495 111-22-33",
                    Email = "info@fpl-dist.ru",
                    Address = "г. Москва, Пресненская наб., 12",
                    ContactPerson = "Сидорова Мария Александровна",
                    Notes = "Международный дистрибьютор"
                },
                new Contact
                {
                    CompanyName = "Paramount CIS",
                    Phone = "+7 495 777-88-99",
                    Email = "licensing@paramount.ru",
                    Address = "г. Москва, Павелецкая пл., 2",
                    ContactPerson = "Козлов Андрей Викторович",
                    Notes = "Региональный офис Paramount"
                },
                new Contact
                {
                    CompanyName = "Warner Bros. СНГ",
                    Phone = "+7 495 555-66-77",
                    Email = "distribution@wb-cis.ru",
                    Address = "г. Москва, Ленинградский пр-т, 31",
                    ContactPerson = "Николаева Елена Сергеевна",
                    Notes = "Региональный офис Warner Bros."
                }
            );
            context.SaveChanges();
        }

        // Seed RightsOwners
        if (!context.RightsOwners.Any())
        {
            var contacts = context.Contacts.ToList();

            context.RightsOwners.AddRange(
                new RightsOwner
                {
                    Name = "Беларусьфильм",
                    Description = "Белорусская киностудия",
                    DateAdded = new DateTime(2023, 01, 15, 0, 0, 0, DateTimeKind.Utc),
                    ContactId = contacts.First(c => c.CompanyName.Contains("Беларусьфильм")).Id
                },
                new RightsOwner
                {
                    Name = "Вольга",
                    Description = "Российский кинодистрибьютор",
                    DateAdded = new DateTime(2023, 01, 20, 0, 0, 0, DateTimeKind.Utc),
                    ContactId = contacts.First(c => c.CompanyName.Contains("Вольга")).Id
                },
                new RightsOwner
                {
                    Name = "FPL",
                    Description = "Международный дистрибьютор",
                    DateAdded = new DateTime(2023, 02, 01, 0, 0, 0, DateTimeKind.Utc),
                    ContactId = contacts.First(c => c.CompanyName.Contains("FPL")).Id
                },
                new RightsOwner
                {
                    Name = "Paramount",
                    Description = "Американская кинокомпания",
                    DateAdded = new DateTime(2023, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                    ContactId = contacts.First(c => c.CompanyName.Contains("Paramount")).Id
                },
                new RightsOwner
                {
                    Name = "Warner Bros.",
                    Description = "Американская кинокомпания",
                    DateAdded = new DateTime(2023, 02, 15, 0, 0, 0, DateTimeKind.Utc),
                    ContactId = contacts.First(c => c.CompanyName.Contains("Warner")).Id
                }
            );
            context.SaveChanges();
        }

        // Seed Films
        if (!context.Films.Any())
        {
            var rightsOwners = context.RightsOwners.ToList();
            var belarusfilm = rightsOwners.First(r => r.Name == "Беларусьфильм");
            var volga = rightsOwners.First(r => r.Name == "Вольга");
            var fpl = rightsOwners.First(r => r.Name == "FPL");
            var paramount = rightsOwners.First(r => r.Name == "Paramount");
            var warner = rightsOwners.First(r => r.Name == "Warner Bros.");

            context.Films.AddRange(
                // Беларусьфильм
                new Film
                {
                    Title = "В тумане",
                    AgeRestriction = "12+",
                    Duration = 130,
                    FilePath = "C:\\Movies\\V_tumane.mp4",
                    PurchaseDate = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 50,
                    DateAdded = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = belarusfilm.Id
                },
                new Film
                {
                    Title = "Анастасия Слуцкая",
                    AgeRestriction = "16+",
                    Duration = 120,
                    FilePath = "C:\\Movies\\Anastasia_Sluckaya.mp4",
                    PurchaseDate = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 30,
                    DateAdded = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = belarusfilm.Id
                },
                new Film
                {
                    Title = "Белые росы",
                    AgeRestriction = "0+",
                    Duration = 90,
                    FilePath = "C:\\Movies\\Belye_rosy.mp4",
                    PurchaseDate = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2027, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 100,
                    DateAdded = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = belarusfilm.Id
                },
                // Вольга
                new Film
                {
                    Title = "Властелин колец: Братство кольца",
                    AgeRestriction = "16+",
                    Duration = 178,
                    FilePath = "C:\\Movies\\LotR.mp4",
                    PurchaseDate = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 20,
                    DateAdded = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = volga.Id
                },
                new Film
                {
                    Title = "Хоббит: Нежданное путешествие",
                    AgeRestriction = "12+",
                    Duration = 169,
                    FilePath = "C:\\Movies\\Hobbit.mp4",
                    PurchaseDate = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 25,
                    DateAdded = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = volga.Id
                },
                // FPL
                new Film
                {
                    Title = "Титаник",
                    AgeRestriction = "12+",
                    Duration = 194,
                    FilePath = "C:\\Movies\\Titanic.mp4",
                    PurchaseDate = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 40,
                    DateAdded = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = fpl.Id
                },
                new Film
                {
                    Title = "Форрест Гамп",
                    AgeRestriction = "12+",
                    Duration = 142,
                    FilePath = "C:\\Movies\\ForrestGump.mp4",
                    PurchaseDate = new DateTime(2023, 03, 22, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 06, 30, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 35,
                    DateAdded = new DateTime(2023, 03, 22, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = fpl.Id
                },
                // Paramount
                new Film
                {
                    Title = "Top Gun: Maverick",
                    AgeRestriction = "16+",
                    Duration = 131,
                    FilePath = "C:\\Movies\\TopGun.mp4",
                    PurchaseDate = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2025, 09, 30, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 15,
                    DateAdded = new DateTime(2023, 03, 19, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = paramount.Id
                },
                new Film
                {
                    Title = "Миссия невыполнима: Смертельная расплата",
                    AgeRestriction = "12+",
                    Duration = 163,
                    FilePath = "C:\\Movies\\MissionImpossible.mp4",
                    PurchaseDate = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2025, 09, 30, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 18,
                    DateAdded = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = paramount.Id
                },
                // Warner Bros.
                new Film
                {
                    Title = "Матрица",
                    AgeRestriction = "16+",
                    Duration = 136,
                    FilePath = "C:\\Movies\\Matrix.mp4",
                    PurchaseDate = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 45,
                    DateAdded = new DateTime(2023, 03, 20, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = warner.Id
                },
                new Film
                {
                    Title = "Начало",
                    AgeRestriction = "16+",
                    Duration = 148,
                    FilePath = "C:\\Movies\\Inception.mp4",
                    PurchaseDate = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsExpirationDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    ShowCount = 50,
                    DateAdded = new DateTime(2023, 03, 21, 0, 0, 0, DateTimeKind.Utc),
                    RightsOwnerId = warner.Id
                }
            );
            context.SaveChanges();
        }
    }
}
