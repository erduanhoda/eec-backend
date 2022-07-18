namespace EEC.API.Infrastructure.Services.History.Dtos;

public class OverviewDto
{
    public decimal TotalConsumptionToday { get; set; }
    public decimal TotalProductionToday { get; set; }
    public double TotalProductionWeek { get; set; }
    public decimal TotalProductionMonth { get; set; }
    public decimal ForecastedConsumption { get; set; }
    public decimal ForecastedProduction { get; set; }
    public decimal TotalProductionYear { get; set; }
    public decimal? TotalEnergySold { get; set; }
}