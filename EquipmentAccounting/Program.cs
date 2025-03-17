using EquipmentAccounting.Data;
using EquipmentAccounting.Forms;
using EquipmentAccounting.Models;

namespace EquipmentAccounting;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Создаём папку Data\DB, если её нет
        string dbPath = "Data\\DB\\equipment.db";
        string folder = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        using (var context = new AppDbContext())
        {
            // Создаём базу, если не создана
            context.Database.EnsureCreated();

            // Добавляем тестовых пользователей, если ещё нет
            if (!context.Users.Any())
            {
                context.Users.AddRange(new List<User>
                {
                    new User { Login = "admin", Password = "admin", Role = "Администратор" },
                    new User { Login = "user", Password = "user", Role = "Пользователь" }
                });
            }

            // 1) Тестовые данные "Беларусьфильм"
            if (!context.Belarusfilms.Any())
            {
                context.Belarusfilms.AddRange(new List<Belarusfilm>
                {
                    new Belarusfilm
                    {
                        Title = "В тумане",
                        DateAdded = new DateTime(2023, 03, 19),
                        AgeRestriction = "12+",
                        Duration = 130,
                        FilePath = "C:\\Movies\\V_tumane.mp4"
                    },
                    new Belarusfilm
                    {
                        Title = "Анастасия Слуцкая",
                        DateAdded = new DateTime(2023, 03, 20),
                        AgeRestriction = "16+",
                        Duration = 120,
                        FilePath = "C:\\Movies\\Anastasia_Sluckaya.mp4"
                    },
                    new Belarusfilm
                    {
                        Title = "Белые росы",
                        DateAdded = new DateTime(2023, 03, 21),
                        AgeRestriction = "0+",
                        Duration = 90,
                        FilePath = "C:\\Movies\\Belye_rosy.mp4"
                    },
                    new Belarusfilm
                    {
                        Title = "Купала",
                        DateAdded = new DateTime(2023, 03, 22),
                        AgeRestriction = "12+",
                        Duration = 100,
                        FilePath = "C:\\Movies\\Kupala.mp4"
                    },
                    new Belarusfilm
                    {
                        Title = "Днепр в огне",
                        DateAdded = new DateTime(2023, 03, 23),
                        AgeRestriction = "12+",
                        Duration = 85,
                        FilePath = "C:\\Movies\\Dnepr_v_ogne.mp4"
                    }
                });
            }

            // 2) Тестовые данные "Вольга"
            if (!context.Volgas.Any())
            {
                context.Volgas.AddRange(new List<Volga>
                {
                    new Volga
                    {
                        Title = "Властелин колец",
                        DateAdded = new DateTime(2023, 03, 19),
                        AgeRestriction = "16+",
                        Duration = 178,
                        FilePath = "C:\\Movies\\LotR.mp4"
                    },
                    new Volga
                    {
                        Title = "Хоббит",
                        DateAdded = new DateTime(2023, 03, 20),
                        AgeRestriction = "12+",
                        Duration = 169,
                        FilePath = "C:\\Movies\\Hobbit.mp4"
                    },
                    new Volga
                    {
                        Title = "Гарри Поттер",
                        DateAdded = new DateTime(2023, 03, 21),
                        AgeRestriction = "12+",
                        Duration = 152,
                        FilePath = "C:\\Movies\\HarryPotter.mp4"
                    },
                    new Volga
                    {
                        Title = "Фантастические твари",
                        DateAdded = new DateTime(2023, 03, 22),
                        AgeRestriction = "12+",
                        Duration = 133,
                        FilePath = "C:\\Movies\\FantasticBeasts.mp4"
                    },
                    new Volga
                    {
                        Title = "Шерлок Холмс",
                        DateAdded = new DateTime(2023, 03, 23),
                        AgeRestriction = "16+",
                        Duration = 128,
                        FilePath = "C:\\Movies\\SherlockHolmes.mp4"
                    }
                });
            }

            // 3) Тестовые данные "FPL"
            if (!context.FPLs.Any())
            {
                context.FPLs.AddRange(new List<FPL>
                {
                    new FPL
                    {
                        Title = "Титаник",
                        DateAdded = new DateTime(2023, 03, 19),
                        AgeRestriction = "12+",
                        Duration = 194,
                        FilePath = "C:\\Movies\\Titanic.mp4"
                    },
                    new FPL
                    {
                        Title = "Пила",
                        DateAdded = new DateTime(2023, 03, 20),
                        AgeRestriction = "18+",
                        Duration = 103,
                        FilePath = "C:\\Movies\\Saw.mp4"
                    },
                    new FPL
                    {
                        Title = "Головоломка",
                        DateAdded = new DateTime(2023, 03, 21),
                        AgeRestriction = "6+",
                        Duration = 95,
                        FilePath = "C:\\Movies\\InsideOut.mp4"
                    },
                    new FPL
                    {
                        Title = "Форрест Гамп",
                        DateAdded = new DateTime(2023, 03, 22),
                        AgeRestriction = "12+",
                        Duration = 142,
                        FilePath = "C:\\Movies\\ForrestGump.mp4"
                    },
                    new FPL
                    {
                        Title = "Пираты Карибского моря",
                        DateAdded = new DateTime(2023, 03, 23),
                        AgeRestriction = "12+",
                        Duration = 143,
                        FilePath = "C:\\Movies\\Pirates.mp4"
                    }
                });
            }

            // 4) Тестовые данные "Paramount"
            if (!context.Paramounts.Any())
            {
                context.Paramounts.AddRange(new List<Paramount>
                {
                    new Paramount
                    {
                        Title = "Top Gun",
                        DateAdded = new DateTime(2023, 03, 19),
                        AgeRestriction = "16+",
                        Duration = 110,
                        FilePath = "C:\\Movies\\TopGun.mp4"
                    },
                    new Paramount
                    {
                        Title = "Transformers",
                        DateAdded = new DateTime(2023, 03, 20),
                        AgeRestriction = "12+",
                        Duration = 144,
                        FilePath = "C:\\Movies\\Transformers.mp4"
                    },
                    new Paramount
                    {
                        Title = "Mission: Impossible",
                        DateAdded = new DateTime(2023, 03, 21),
                        AgeRestriction = "12+",
                        Duration = 110,
                        FilePath = "C:\\Movies\\MissionImpossible.mp4"
                    },
                    new Paramount
                    {
                        Title = "Sonic the Hedgehog",
                        DateAdded = new DateTime(2023, 03, 22),
                        AgeRestriction = "6+",
                        Duration = 99,
                        FilePath = "C:\\Movies\\Sonic.mp4"
                    },
                    new Paramount
                    {
                        Title = "Star Trek",
                        DateAdded = new DateTime(2023, 03, 23),
                        AgeRestriction = "12+",
                        Duration = 127,
                        FilePath = "C:\\Movies\\StarTrek.mp4"
                    }
                });
            }

            // 5) Тестовые данные "WarnerBros"
            if (!context.WarnerBrosSet.Any())
            {
                context.WarnerBrosSet.AddRange(new List<WarnerBros>
                {
                    new WarnerBros
                    {
                        Title = "Harry Potter",
                        DateAdded = new DateTime(2023, 03, 19),
                        AgeRestriction = "12+",
                        Duration = 152,
                        FilePath = "C:\\Movies\\HarryPotter.mp4"
                    },
                    new WarnerBros
                    {
                        Title = "The Matrix",
                        DateAdded = new DateTime(2023, 03, 20),
                        AgeRestriction = "16+",
                        Duration = 136,
                        FilePath = "C:\\Movies\\Matrix.mp4"
                    },
                    new WarnerBros
                    {
                        Title = "Inception",
                        DateAdded = new DateTime(2023, 03, 21),
                        AgeRestriction = "16+",
                        Duration = 148,
                        FilePath = "C:\\Movies\\Inception.mp4"
                    },
                    new WarnerBros
                    {
                        Title = "Tenet",
                        DateAdded = new DateTime(2023, 03, 22),
                        AgeRestriction = "16+",
                        Duration = 150,
                        FilePath = "C:\\Movies\\Tenet.mp4"
                    },
                    new WarnerBros
                    {
                        Title = "Batman v Superman",
                        DateAdded = new DateTime(2023, 03, 23),
                        AgeRestriction = "16+",
                        Duration = 151,
                        FilePath = "C:\\Movies\\BatmanVS.mp4"
                    }
                });
            }

            context.SaveChanges();
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LoginForm());
    }
}