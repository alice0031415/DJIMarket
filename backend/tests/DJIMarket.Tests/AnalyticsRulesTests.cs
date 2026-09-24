using DJIMarket.Application.Analytics;

namespace DJIMarket.Tests;

public sealed class AnalyticsRulesTests
{
    [Fact]
    public void Margin_is_zero_when_revenue_is_zero() => Assert.Equal(0m, AnalyticsRules.Margin(0, 100));

    [Fact]
    public void Margin_is_profit_over_revenue() => Assert.Equal(0.25m, AnalyticsRules.Margin(1000, 250));

    [Fact]
    public void AverageCheck_uses_paid_sale_count() => Assert.Equal(1250m, AnalyticsRules.AverageCheck(5000, 4));

    [Fact]
    public void ChangePct_handles_zero_previous_period() => Assert.Equal(100m, AnalyticsRules.ChangePct(100, 0));

    [Fact]
    public void ChangePct_returns_negative_for_decline() => Assert.Equal(-20m, AnalyticsRules.ChangePct(80, 100));
}
