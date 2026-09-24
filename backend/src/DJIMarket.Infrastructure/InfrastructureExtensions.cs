using DJIMarket.Application.Analytics;
using DJIMarket.Infrastructure.Analytics;
using DJIMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DJIMarket.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Default")));
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}
