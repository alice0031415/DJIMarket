using DJIMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DJIMarket.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        // Sales are the completion marker for the seed. The previous implementation
        // saved reference data first and could leave a partially seeded database
        // (managers without sales) after an interrupted startup.
        if (await db.Sales.AnyAsync(cancellationToken))
            return;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // Recover cleanly from a partial seed created by an earlier version.
        if (await db.Managers.AnyAsync(cancellationToken) ||
            await db.Customers.AnyAsync(cancellationToken) ||
            await db.Products.AnyAsync(cancellationToken) ||
            await db.Categories.AnyAsync(cancellationToken))
        {
            db.SaleItems.RemoveRange(db.SaleItems);
            db.Sales.RemoveRange(db.Sales);
            db.Products.RemoveRange(db.Products);
            db.Categories.RemoveRange(db.Categories);
            db.Customers.RemoveRange(db.Customers);
            db.Managers.RemoveRange(db.Managers);
            await db.SaveChangesAsync(cancellationToken);
        }

        var random = new Random(20260924);
        var managers = new[]
        {
            ("Анна Соколова", "Enterprise", "Senior Manager", 1.30m),
            ("Иван Петров", "Enterprise", "Account Manager", 1.15m),
            ("Мария Волкова", "Enterprise", "Account Manager", 1.08m),
            ("Алексей Смирнов", "Mid-Market", "Sales Manager", 1.02m),
            ("Ольга Морозова", "Mid-Market", "Sales Manager", 0.96m),
            ("Дмитрий Кузнецов", "Mid-Market", "Sales Manager", 0.91m),
            ("Елена Орлова", "SMB", "Sales Manager", 0.87m),
            ("Сергей Лебедев", "SMB", "Sales Manager", 0.82m),
            ("Наталья Федорова", "SMB", "Sales Manager", 0.76m),
            ("Максим Егоров", "SMB", "Sales Manager", 0.70m),
            ("Татьяна Попова", "Enterprise", "Key Account Manager", 1.12m),
            ("Роман Васильев", "Enterprise", "Account Manager", 0.98m),
            ("Юлия Никитина", "Mid-Market", "Sales Manager", 1.04m),
            ("Павел Захаров", "Mid-Market", "Sales Manager", 0.89m),
            ("Ирина Крылова", "SMB", "Sales Manager", 0.73m),
            ("Виктор Макаров", "SMB", "Sales Manager", 0.66m),
            ("Ксения Белова", "SMB", "Sales Manager", 0.62m),
            ("Артём Тарасов", "Mid-Market", "Sales Manager", 0.94m)
        };

        var managerEntities = managers.Select(x => new Manager
        {
            Id = Guid.NewGuid(), Name = x.Item1, Team = x.Item2, Position = x.Item3, IsActive = true, AvatarSeed = x.Item1
        }).ToList();

        var categories = new[] { "DJI Drones", "Cameras", "Gimbals", "Accessories", "Batteries" }
            .Select(name => new Category { Id = Guid.NewGuid(), Name = name }).ToList();

        var productsByCategory = new Dictionary<string, string[]>
        {
            ["DJI Drones"] = ["DJI Air 3S", "DJI Mini 4 Pro", "DJI Mavic 4 Pro", "DJI Avata 2", "DJI Neo"],
            ["Cameras"] = ["DJI Osmo Pocket 4", "DJI Action 6", "DJI Ronin 4D", "DJI Zenmuse X9"],
            ["Gimbals"] = ["DJI RS 4", "DJI RS 4 Pro", "DJI Osmo Mobile 7", "DJI RS 3 Mini"],
            ["Accessories"] = ["DJI Mic 3", "Wide-Angle Lens", "ND Filter Set", "Propeller Guard", "Carrying Case", "Phone Holder"],
            ["Batteries"] = ["Intelligent Flight Battery", "Battery Charging Hub", "High-Capacity Battery", "Powerbank 2000"]
        };

        var products = categories.SelectMany(category => productsByCategory[category.Name].Select(name =>
            new Product { Id = Guid.NewGuid(), Name = name, CategoryId = category.Id, IsActive = true })).ToList();

        var customerNames = new[] { "СтройТех", "МедиаЛаб", "АэроСервис", "ГеоПроект", "Ритейл Плюс", "Фотостудия 24", "Квадро", "ТехноСфера", "ЛогистикПро", "Архитектура+" };
        var segments = new[] { "Enterprise", "Mid-Market", "SMB" };
        var customers = Enumerable.Range(1, 72).Select(i => new Customer
        {
            Id = Guid.NewGuid(),
            Name = $"Клиент {i:000}",
            Company = $"{customerNames[(i - 1) % customerNames.Length]} {i:000}",
            Segment = segments[(i + random.Next(segments.Length)) % segments.Length]
        }).ToList();

        db.AddRange(managerEntities);
        db.AddRange(categories);
        db.AddRange(products);
        db.AddRange(customers);

        var referenceStart = new DateOnly(2026, 1, 1);
        var referenceEnd = new DateOnly(2026, 9, 24);
        var sales = new List<Sale>(4200);
        var days = referenceEnd.DayNumber - referenceStart.DayNumber + 1;

        for (var i = 0; i < 3400; i++)
        {
            var dayNumber = referenceStart.DayNumber + random.Next(days);
            var date = DateOnly.FromDayNumber(dayNumber);
            var monthSeasonality = 1m + 0.16m * (decimal)Math.Sin((date.Month / 12.0) * Math.PI * 2) + (date.Month is 3 or 9 ? 0.22m : 0m);
            var managerIndex = random.Next(managerEntities.Count);
            var manager = managerEntities[managerIndex];
            var factor = managers[managerIndex].Item4;
            var statusRoll = random.NextDouble();
            var status = statusRoll < 0.075 ? SaleStatus.Cancelled : statusRoll < 0.115 ? SaleStatus.Refunded : SaleStatus.Paid;

            // Monday/Friday and month-end buying patterns make the data less uniform.
            var weekdayFactor = date.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Friday ? 1.12m : 0.96m;
            if (random.NextDouble() > (double)(monthSeasonality * weekdayFactor * factor / 1.2m))
            {
                i--;
                continue;
            }

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                ManagerId = manager.Id,
                CustomerId = customers[random.Next(customers.Count)].Id,
                SoldAt = new DateTimeOffset(date.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(9 + random.Next(10)))), TimeSpan.FromHours(3)),
                Status = status
            };

            var itemCount = random.NextDouble() < 0.74 ? 1 : random.Next(2, 5);
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var product = products[random.Next(products.Count)];
                var basePrice = product.Name switch
                {
                    "DJI Mavic 4 Pro" => 195_000m,
                    "DJI Air 3S" => 112_000m,
                    "DJI Ronin 4D" => 285_000m,
                    "DJI Zenmuse X9" => 210_000m,
                    "DJI RS 4 Pro" => 95_000m,
                    "DJI Mic 3" => 36_000m,
                    "DJI Mini 4 Pro" => 78_000m,
                    _ => 8_000m + random.Next(80_000)
                };
                var quantity = random.NextDouble() < 0.86 ? 1 : random.Next(2, 4);
                var price = Math.Round(basePrice * (0.90m + (decimal)random.NextDouble() * 0.25m), 2);
                var cost = Math.Round(price * (0.58m + (decimal)random.NextDouble() * 0.20m), 2);
                sale.Items.Add(new SaleItem { Id = Guid.NewGuid(), ProductId = product.Id, Quantity = quantity, SalePrice = price, CostPrice = cost });
            }
            sales.Add(sale);
        }

        db.AddRange(sales);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        Console.WriteLine($"Seed completed: {managerEntities.Count} managers, {customers.Count} customers, {categories.Count} categories, {products.Count} products, {sales.Count} sales.");
    }
}
