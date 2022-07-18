using EEC.API.Infrastructure.Services.History.Dtos;

namespace EEC.API.Infrastructure.Services.History;

public interface IHistoryService
{
    Task<OverviewDto> GetOverview();
    Task<TodayConsumptionDto> GetTodayConsumption();
    Task<TotalEnergySoldWeeklyDto> GetTotalEnergySoldPerWeek();
    Task<BatteryInfoDto> GetBatteryInfo();
}