namespace MiniShop.Core.Interfaces;

public record MonthlyRevenue(int Year, int Month, decimal Revenue, int Orders);

public interface IOrderReports
{
    Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync();
}