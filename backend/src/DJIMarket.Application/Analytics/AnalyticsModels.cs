namespace DJIMarket.Application.Analytics;

public enum RankingMetric
{
    GrossProfit,
    AverageCheck
}

public sealed record DashboardQuery(DateOnly From, DateOnly To, RankingMetric RankBy);

public sealed record KpiDto(
    decimal Revenue,
    decimal GrossProfit,
    decimal Margin,
    int SalesCount,
    decimal AverageCheck,
    string? BestManager,
    decimal RevenueChangePct,
    decimal ProfitChangePct,
    decimal SalesCountChangePct,
    decimal AverageCheckChangePct);

public sealed record ManagerRankingDto(
    int Rank,
    Guid ManagerId,
    string Manager,
    string Team,
    int SalesCount,
    decimal Revenue,
    decimal GrossProfit,
    decimal AverageCheck,
    decimal Margin,
    decimal ChangePct);

public sealed record TrendPointDto(DateOnly Date, decimal Revenue, decimal GrossProfit, int SalesCount);
public sealed record CategoryMetricDto(string Category, decimal Revenue, decimal GrossProfit, decimal SharePct);
public sealed record ProductMetricDto(string Product, string Category, int Units, decimal Revenue, decimal GrossProfit);
public sealed record RecentSaleDto(DateTimeOffset Date, string Manager, string Customer, string Products, string Status, decimal Amount, decimal GrossProfit);

public sealed record DashboardResponse(
    DateOnly From,
    DateOnly To,
    DateOnly PreviousFrom,
    DateOnly PreviousTo,
    KpiDto Kpis,
    IReadOnlyList<ManagerRankingDto> ManagerRanking,
    IReadOnlyList<TrendPointDto> Trend,
    IReadOnlyList<CategoryMetricDto> Categories,
    IReadOnlyList<ProductMetricDto> TopProducts,
    IReadOnlyList<RecentSaleDto> RecentSales);

public interface IAnalyticsService
{
    Task<DashboardResponse> GetDashboardAsync(DashboardQuery query, CancellationToken cancellationToken);
}
