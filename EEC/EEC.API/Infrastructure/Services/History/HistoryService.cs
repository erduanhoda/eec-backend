using EEC.API.Infrastructure.Services.History.Dtos;
using Newtonsoft.Json;

namespace EEC.API.Infrastructure.Services.History;

public class HistoryService : IHistoryService
{
    private readonly HttpClient _httpClient;

    private readonly string historyURL =
        "https://api.staging.eidaenergy.no/consumer/v1/historic/a117ce35-0538-4a38-b1be-db263117a548?limit=100&fromDate=2022-05-27&token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb25zdW1lciI6eyJpZCI6MywibmFtZSI6IkFuZGVyc0YiLCJjb250YWN0ZW1haWwiOiJhbmRlcnNAaG9tZXNvdXJjaW5nLm5vIiwidG9rZW4iOm51bGx9LCJpYXQiOjE2NTY2NjQ4MDB9.dlHyktJSXFYxCkmEDQB3K2YIZcAjIKemSDT08H2JDlI";
    public HistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<OverviewDto> GetOverview()
    {
        var responseString = await _httpClient.GetStringAsync(historyURL);
        var history = JsonConvert.DeserializeObject<List<Core.Entities.History>>(responseString);

        if (history == null) return new OverviewDto();

        return new OverviewDto()
        {
            TotalConsumptionToday = CalculateTotalConsumptionToday(history),
            TotalProductionToday = CalculateTotalProductionToday(history),
            TotalProductionMonth = CalculateTotalProductionMonth(history),
            TotalProductionYear = CalculateTotalProductionYear(history),
            TotalEnergySold = history.Sum(x => x.buy_energy_total)
        };
    }

    public async Task<TodayConsumptionDto> GetTodayConsumption()
    {
       var responseString = await _httpClient.GetStringAsync(historyURL);
       
       var history = JsonConvert.DeserializeObject<List<Core.Entities.History>>(responseString);

       if (history == null) return new TodayConsumptionDto();
       
       var todayHistory = FilterToday(history);

       var now = DateTime.Now;
       var todayDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
       DateTime endDate = todayDateTime.AddDays(1);

       var consumptions = new List<ConsumptionDto>();
       
       while (todayDateTime.Date != endDate.Date)
       {
           var filterConsumptionByHour = todayHistory.Where(x =>
               x.inverter_update_time.TimeOfDay >= todayDateTime.TimeOfDay && x.inverter_update_time.TimeOfDay <= todayDateTime.AddHours(1).TimeOfDay);
           
           consumptions.Add(new ConsumptionDto { Time = todayDateTime.ToString("HH:mm"), 
               Consumption = Math.Abs(filterConsumptionByHour.Sum(x => x.realtime_cons_W ?? 0)) });
           
           todayDateTime = todayDateTime.AddHours(1);
       }

       return new TodayConsumptionDto { Consumptions = consumptions };
    }

    public async Task<TotalEnergySoldWeeklyDto> GetTotalEnergySoldPerWeek()
    {
        var responseString = await _httpClient.GetStringAsync(historyURL);
        var history = JsonConvert.DeserializeObject<List<Core.Entities.History>>(responseString);
        
        var startOfWeek = DateTime.Now.AddDays(-4).StartOfWeek(DayOfWeek.Monday);
        var endOfWeek = startOfWeek.AddDays(7);

        var consumptions = new List<WeeklySoldDto>();
        
        while (startOfWeek.Day != endOfWeek.Day)
        {
            var filterConsumptionByDay = history.Where(x =>
                x.inverter_update_time.Day == startOfWeek.Day);
            consumptions.Add(new WeeklySoldDto
            {
                Day = startOfWeek.DayOfWeek.ToString(), 
                Sold = filterConsumptionByDay.Sum(x => x.total_sold_PV ?? 0)
            });
            startOfWeek = startOfWeek.AddDays(1);
        }

        return new TotalEnergySoldWeeklyDto { Consumptions = consumptions };
    }

    public async Task<BatteryInfoDto> GetBatteryInfo()
    {
        var responseString = await _httpClient.GetStringAsync(historyURL);
        var history = JsonConvert.DeserializeObject<List<Core.Entities.History>>(responseString);

        var batteryInfo = history.MaxBy(x => x.inverter_update_time);

        if (batteryInfo == null) return new BatteryInfoDto();

        return new BatteryInfoDto
        {
            Type = batteryInfo.battery_type,
            Temperature = batteryInfo.battery_temp,
            Flow = batteryInfo.status_battery_flow,
            Prc = batteryInfo.battery_prc
        };
    }

    private decimal CalculateTotalProductionYear(List<Core.Entities.History> history)
    {
        var filterByYear = FilterByYear(history);
        
        var sum = filterByYear.Sum(x=> x.consumption_daily);
        
        return Math.Round(sum, 2);
    }

    private decimal CalculateTotalProductionMonth(List<Core.Entities.History> history)
    {
        var filterByMonth = FilterByMonth(history);
        
        var sum = filterByMonth.Sum(x=> x.consumption_daily);
        
        return Math.Round(sum, 2);
    }

    private static decimal CalculateTotalConsumptionToday(List<Core.Entities.History> history)
    {
        if (history == null || history.Count == 0) return 0;
        
        var filterToday = FilterToday(history);

        var sum = filterToday.Sum(x=> x.consumption_daily);
        
        return Math.Round(sum, 2);
    }

    private static decimal CalculateTotalProductionToday(List<Core.Entities.History> history)
    {
        if (history == null || history.Count == 0) return 0;

        var filterToday = FilterToday(history);

        var sum = filterToday.Sum(x=> x.daily_prod_KWH);
        return Math.Round(sum, 2);
    }
    
    private static IEnumerable<Core.Entities.History> FilterToday(List<Core.Entities.History> history)
    {
        return history.Where(x =>
            x.inverter_update_time.ToShortDateString() == DateTime.Today.AddDays(-4).ToShortDateString());
    }
    
    private static IEnumerable<Core.Entities.History> FilterByMonth(List<Core.Entities.History> history)
    {
        return history.Where(x =>
            x.inverter_update_time.Month == DateTime.Today.Month && x.inverter_update_time.Year == DateTime.Today.Year);
    }
    
    private static IEnumerable<Core.Entities.History> FilterByYear(List<Core.Entities.History> history)
    {
        return history.Where(x => x.inverter_update_time.Year == DateTime.Today.Year);
    }
    
}

public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}
    