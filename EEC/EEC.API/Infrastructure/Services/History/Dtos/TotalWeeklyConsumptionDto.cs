namespace EEC.API.Infrastructure.Services.History.Dtos;

public class TotalEnergySoldWeeklyDto
{
	public IEnumerable<WeeklySoldDto> Consumptions { get; set; }
}