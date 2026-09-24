using DJIMarket.Application.Analytics;
using DJIMarket.Domain.Entities;
using DJIMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DJIMarket.Infrastructure.Analytics;

public sealed class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public async Task<DashboardResponse> GetDashboardAsync(DashboardQuery query, CancellationToken cancellationToken)
    {
        if (query.From >= query.To) throw new ArgumentException("From must be before To.");
        var span = query.To.DayNumber - query.From.DayNumber;
        var previousTo = query.From;
        var previousFrom = query.From.AddDays(-span);

        var currentKpis = await QueryKpisAsync(query.From, query.To, cancellationToken);
        var previousKpis = await QueryKpisAsync(previousFrom, previousTo, cancellationToken);
        var ranking = await QueryRankingAsync(query.From, query.To, previousFrom, previousTo, query.RankBy, cancellationToken);
        var trend = await QueryTrendAsync(query.From, query.To, cancellationToken);
        var categories = await QueryCategoriesAsync(query.From, query.To, cancellationToken);
        var products = await QueryProductsAsync(query.From, query.To, cancellationToken);
        var recent = await QueryRecentSalesAsync(query.From, query.To, cancellationToken);
        var bestManager = ranking.OrderByDescending(x => x.GrossProfit).FirstOrDefault()?.Manager;

        return new DashboardResponse(
            query.From, query.To.AddDays(-1), previousFrom, previousTo.AddDays(-1),
            new KpiDto(
                currentKpis.Revenue, currentKpis.GrossProfit, currentKpis.Revenue == 0 ? 0 : currentKpis.GrossProfit / currentKpis.Revenue,
                currentKpis.SalesCount, currentKpis.SalesCount == 0 ? 0 : currentKpis.Revenue / currentKpis.SalesCount,
                bestManager,
                ChangePct(currentKpis.Revenue, previousKpis.Revenue),
                ChangePct(currentKpis.GrossProfit, previousKpis.GrossProfit),
                ChangePct(currentKpis.SalesCount, previousKpis.SalesCount),
                ChangePct(currentKpis.SalesCount == 0 ? 0 : currentKpis.Revenue / currentKpis.SalesCount,
                          previousKpis.SalesCount == 0 ? 0 : previousKpis.Revenue / previousKpis.SalesCount)),
            ranking, trend, categories, products, recent);
    }

    private async Task<(decimal Revenue, decimal GrossProfit, int SalesCount)> QueryKpisAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var aggregate = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(from) && i.Sale.SoldAt < Start(to))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity),
                SalesCount = g.Select(i => i.SaleId).Distinct().Count()
            })
            .SingleOrDefaultAsync(ct);

        return aggregate is null ? (0, 0, 0) : (aggregate.Revenue, aggregate.GrossProfit, aggregate.SalesCount);
    }

    private async Task<IReadOnlyList<ManagerRankingDto>> QueryRankingAsync(DateOnly from, DateOnly to, DateOnly prevFrom, DateOnly prevTo, RankingMetric rankBy, CancellationToken ct)
    {
        var current = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(from) && i.Sale.SoldAt < Start(to))
            .GroupBy(i => new { i.Sale.ManagerId, Manager = i.Sale.Manager.Name, i.Sale.Manager.Team })
            .Select(g => new
            {
                g.Key.ManagerId,
                g.Key.Manager,
                g.Key.Team,
                SalesCount = g.Select(i => i.SaleId).Distinct().Count(),
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity)
            })
            .ToListAsync(ct);

        var previous = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(prevFrom) && i.Sale.SoldAt < Start(prevTo))
            .GroupBy(i => i.Sale.ManagerId)
            .Select(g => new
            {
                ManagerId = g.Key,
                SalesCount = g.Select(i => i.SaleId).Distinct().Count(),
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity)
            })
            .ToListAsync(ct);

        var currentByManager = current.ToDictionary(x => x.ManagerId);
        var previousByManager = previous.ToDictionary(x => x.ManagerId);
        var managers = await db.Managers.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

        var rows = managers.Select(manager =>
        {
            currentByManager.TryGetValue(manager.Id, out var c);
            previousByManager.TryGetValue(manager.Id, out var p);
            var salesCount = c?.SalesCount ?? 0;
            var revenue = c?.Revenue ?? 0;
            var profit = c?.GrossProfit ?? 0;
            var previousMetric = rankBy == RankingMetric.AverageCheck
                ? (p is null || p.SalesCount == 0 ? 0 : p.Revenue / p.SalesCount)
                : (p?.GrossProfit ?? 0);
            var currentMetric = rankBy == RankingMetric.AverageCheck
                ? (salesCount == 0 ? 0 : revenue / salesCount)
                : profit;
            return new
            {
                manager.Id,
                manager.Name,
                manager.Team,
                SalesCount = salesCount,
                Revenue = revenue,
                GrossProfit = profit,
                AverageCheck = salesCount == 0 ? 0 : revenue / salesCount,
                Margin = revenue == 0 ? 0 : profit / revenue,
                ChangePct = ChangePct(currentMetric, previousMetric)
            };
        });

        var ranked = rows
            .OrderByDescending(x => rankBy == RankingMetric.AverageCheck ? x.AverageCheck : x.GrossProfit)
            .ThenBy(x => x.Name)
            .ToList();

        return ranked.Select((x, index) => new ManagerRankingDto(
            index + 1, x.Id, x.Name, x.Team, x.SalesCount, x.Revenue, x.GrossProfit,
            x.AverageCheck, x.Margin, x.ChangePct)).ToList();
    }

    private async Task<IReadOnlyList<TrendPointDto>> QueryTrendAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        // Keep DateTime arithmetic out of the SQL projection. PostgreSQL returns the
        // grouped date parts and DateOnly is constructed after materialization.
        var rows = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(from) && i.Sale.SoldAt < Start(to))
            .GroupBy(i => new { i.Sale.SoldAt.Year, i.Sale.SoldAt.Month, i.Sale.SoldAt.Day })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Day = g.Key.Day,
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity),
                SalesCount = g.Select(i => i.SaleId).Distinct().Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
            .ToListAsync(ct);

        return rows.Select(x => new TrendPointDto(
            new DateOnly(x.Year, x.Month, x.Day), x.Revenue, x.GrossProfit, x.SalesCount)).ToList();
    }

    private async Task<IReadOnlyList<CategoryMetricDto>> QueryCategoriesAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var rows = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(from) && i.Sale.SoldAt < Start(to))
            .GroupBy(i => i.Product.Category.Name)
            .Select(g => new
            {
                Category = g.Key,
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity)
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync(ct);

        var totalRevenue = rows.Sum(x => x.Revenue);
        return rows.Select(x => new CategoryMetricDto(
            x.Category, x.Revenue, x.GrossProfit,
            totalRevenue == 0 ? 0 : x.Revenue / totalRevenue)).ToList();
    }

    private async Task<IReadOnlyList<ProductMetricDto>> QueryProductsAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        // Project to an anonymous SQL-translatable shape first. EF Core/Npgsql
        // can struggle to translate constructor calls inside GroupBy projections.
        var rows = await db.SaleItems.AsNoTracking()
            .Where(i => i.Sale.Status == SaleStatus.Paid &&
                        i.Sale.SoldAt >= Start(from) && i.Sale.SoldAt < Start(to))
            .GroupBy(i => new { Product = i.Product.Name, Category = i.Product.Category.Name })
            .Select(g => new
            {
                Product = g.Key.Product,
                Category = g.Key.Category,
                Units = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.SalePrice * i.Quantity),
                GrossProfit = g.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity)
            })
            .OrderByDescending(x => x.GrossProfit)
            .Take(5)
            .ToListAsync(ct);

        return rows.Select(x => new ProductMetricDto(
            x.Product, x.Category, x.Units, x.Revenue, x.GrossProfit)).ToList();
    }

    private async Task<IReadOnlyList<RecentSaleDto>> QueryRecentSalesAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var rows = await db.Sales.AsNoTracking()
            .Where(s => s.SoldAt >= Start(from) && s.SoldAt < Start(to))
            .Include(s => s.Manager)
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .OrderByDescending(s => s.SoldAt)
            .Take(15)
            .ToListAsync(ct);

        return rows.Select(s => new RecentSaleDto(
            s.SoldAt,
            s.Manager.Name,
            s.Customer.Company,
            string.Join(", ", s.Items.Select(i => i.Quantity > 1 ? $"{i.Product.Name} ×{i.Quantity}" : i.Product.Name)),
            s.Status.ToString(),
            s.Items.Sum(i => i.SalePrice * i.Quantity),
            s.Status == SaleStatus.Paid ? s.Items.Sum(i => (i.SalePrice - i.CostPrice) * i.Quantity) : 0)).ToList();
    }

    private static DateTimeOffset Start(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    private static decimal ChangePct(decimal current, decimal previous)
    {
        if (previous == 0) return current == 0 ? 0 : 100;
        return (current - previous) / Math.Abs(previous) * 100m;
    }
}
