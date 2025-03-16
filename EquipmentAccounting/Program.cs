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

            if (context.Users.Count() < 6)
            {
                context.Users.AddRange(new List<User>
                {
                    new User { Login = "user1", Password = "111111", Role = "Администратор" },
                    new User { Login = "user2", Password = "222222", Role = "Пользователь" },
                    new User { Login = "user3", Password = "333333", Role = "Администратор" },
                    new User { Login = "user4", Password = "444444", Role = "Пользователь" },
                    new User { Login = "user5", Password = "555555", Role = "Пользователь" },
                    new User { Login = "user6", Password = "666666", Role = "Администратор" }
                });
            }

            if (!context.ActAcceptances.Any())
            {
                context.ActAcceptances.AddRange(new List<ActAcceptance>
                {
                    new ActAcceptance { ActNumber = "AA-001", Date = new DateTime(2023, 03, 19) },
                    new ActAcceptance { ActNumber = "AA-002", Date = new DateTime(2023, 03, 20) },
                    new ActAcceptance { ActNumber = "AA-003", Date = new DateTime(2023, 03, 21) },
                    new ActAcceptance { ActNumber = "AA-004", Date = new DateTime(2023, 03, 22) },
                    new ActAcceptance { ActNumber = "AA-005", Date = new DateTime(2023, 03, 23) }
                });
            }

            if (!context.Locations.Any())
            {
                context.Locations.AddRange(new List<Location>
                {
                    new Location { Name = "Лаборатория" },
                    new Location { Name = "Офис" },
                    new Location { Name = "Склад" },
                    new Location { Name = "Цех" },
                    new Location { Name = "Главный офис" }
                });
            }

            if (!context.Responsibles.Any())
            {
                context.Responsibles.AddRange(new List<Responsible>
                {
                    new Responsible
                        { FullName = "Иванов Иван Иванович", Position = "Менеджер", Phone = "+375291234567" },
                    new Responsible
                        { FullName = "Петров Петр Петрович", Position = "Инженер", Phone = "+375291234568" },
                    new Responsible
                        { FullName = "Сидоров Сидор Сидорович", Position = "Техник", Phone = "+375291234569" },
                    new Responsible
                        { FullName = "Кузнецов Кузьма Кузьмич", Position = "Администратор", Phone = "+375291234570" },
                    new Responsible
                        { FullName = "Васильев Василий Васильевич", Position = "Бухгалтер", Phone = "+375291234571" },
                    new Responsible
                        { FullName = "Федоров Федор Федорович", Position = "Директор", Phone = "+375291234572" }
                });
            }

            if (!context.FixedAssets.Any())
            {
                context.FixedAssets.AddRange(new List<FixedAsset>
                {
                    new FixedAsset
                    {
                        InventoryNumber = "FA-001",
                        Year = 2020,
                        ActNumber = "AA-001",
                        Location = "Лаборатория",
                        Cost = 1000m,
                        DepreciationRate = 15,
                        Description = "Монитор 24\"",
                        ResponsibleName = "Иванов Иван Иванович"
                    },
                    new FixedAsset
                    {
                        InventoryNumber = "FA-002",
                        Year = 2021,
                        ActNumber = "AA-002",
                        Location = "Офис",
                        Cost = 2000m,
                        DepreciationRate = 10,
                        Description = "Принтер",
                        ResponsibleName = "Петров Петр Петрович"
                    },
                    new FixedAsset
                    {
                        InventoryNumber = "FA-003",
                        Year = 2019,
                        ActNumber = "AA-003",
                        Location = "Склад",
                        Cost = 1500m,
                        DepreciationRate = 12,
                        Description = "Сканер",
                        ResponsibleName = "Сидоров Сидор Сидорович"
                    },
                    new FixedAsset
                    {
                        InventoryNumber = "FA-004",
                        Year = 2022,
                        ActNumber = "AA-004",
                        Location = "Цех",
                        Cost = 3000m,
                        DepreciationRate = 8,
                        Description = "Компьютер",
                        ResponsibleName = "Кузнецов Кузьма Кузьмич"
                    },
                    new FixedAsset
                    {
                        InventoryNumber = "FA-005",
                        Year = 2020,
                        ActNumber = "AA-005",
                        Location = "Главный офис",
                        Cost = 500m,
                        DepreciationRate = 20,
                        Description = "Клавиатура",
                        ResponsibleName = "Васильев Василий Васильевич"
                    }
                });
            }

            if (!context.Repairs.Any())
            {
                context.Repairs.AddRange(new List<Repair>
                {
                    new Repair
                    {
                        InventoryNumber = "FA-001", Date = new DateTime(2023, 03, 19), Description = "Замена матрицы"
                    },
                    new Repair
                    {
                        InventoryNumber = "FA-002", Date = new DateTime(2023, 03, 20), Description = "Ремонт принтера"
                    },
                    new Repair
                    {
                        InventoryNumber = "FA-003", Date = new DateTime(2023, 03, 21), Description = "Чистка сканера"
                    },
                    new Repair
                    {
                        InventoryNumber = "FA-004", Date = new DateTime(2023, 03, 22), Description = "Обновление ПО"
                    },
                    new Repair
                    {
                        InventoryNumber = "FA-005", Date = new DateTime(2023, 03, 23), Description = "Замена клавиатуры"
                    }
                });
            }

            if (!context.Transfers.Any())
            {
                context.Transfers.AddRange(new List<Transfer>
                {
                    new Transfer
                        { InventoryNumber = "FA-001", Date = new DateTime(2023, 03, 19), NewLocation = "Офис" },
                    new Transfer
                        { InventoryNumber = "FA-002", Date = new DateTime(2023, 03, 20), NewLocation = "Лаборатория" },
                    new Transfer { InventoryNumber = "FA-003", Date = new DateTime(2023, 03, 21), NewLocation = "Цех" },
                    new Transfer
                        { InventoryNumber = "FA-004", Date = new DateTime(2023, 03, 22), NewLocation = "Склад" },
                    new Transfer
                        { InventoryNumber = "FA-005", Date = new DateTime(2023, 03, 23), NewLocation = "Главный офис" }
                });
            }

            context.SaveChanges();
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LoginForm());
    }
}