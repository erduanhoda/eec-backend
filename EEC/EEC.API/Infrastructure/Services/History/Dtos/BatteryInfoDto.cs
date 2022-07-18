namespace EEC.API.Infrastructure.Services.History.Dtos;

public class BatteryInfoDto
{
	public string Type { get; set; }
	public decimal Temperature { get; set; }
	public string Flow { get; set; }
	public int Prc { get; set; }
}