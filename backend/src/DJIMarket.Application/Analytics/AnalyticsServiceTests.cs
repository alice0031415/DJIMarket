namespace DJIMarket.Application.Analytics;

public static class AnalyticsRules
{
    public static decimal Margin(decimal revenue, decimal grossProfit) => revenue == 0 ? 0 : grossProfit / revenue;
    public static decimal AverageCheck(decimal revenue, int salesCount) => salesCount == 0 ? 0 : revenue / salesCount;
    public static decimal ChangePct(decimal current, decimal previous) => previous == 0 ? (current == 0 ? 0 : 100) : (current - previous) / Math.Abs(previous) * 100m;
}
