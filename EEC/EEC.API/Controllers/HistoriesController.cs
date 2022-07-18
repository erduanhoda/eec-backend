using EEC.API.Infrastructure.Services.History;
using Microsoft.AspNetCore.Mvc;

namespace EEC.API.Controllers;

[ApiController]
[Route("history")]
public class HistoriesController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoriesController(IHistoryService historyService)
    {
        _historyService = historyService;
    }
    
    [HttpGet(Name = "overview")]
    public async Task<IActionResult> Get()
    {
        var overview = await _historyService.GetOverview();
        return Ok(overview);
    }

    [HttpGet("consumption/today")]
    public async Task<IActionResult> GetToday()
    {
        var todayConsumption = await _historyService.GetTodayConsumption();
        return Ok(todayConsumption.Consumptions);
    }
    
    [HttpGet("energy/sold")]
    public async Task<IActionResult> GetTotalEnergySold()
    {
        var todayConsumption = await _historyService.GetTotalEnergySoldPerWeek();
        return Ok(todayConsumption.Consumptions);
    }
    
    [HttpGet("battery/info")]
    public async Task<IActionResult> GetBatteryInfo()
    {
        var batteryInfo = await _historyService.GetBatteryInfo();
        return Ok(batteryInfo);
    }
}