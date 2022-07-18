namespace EEC.API.Core.Entities;

public class History
{
    public int id { get; set; }
    public DateTime inverter_update_time { get; set; }
    public int battery_prc { get; set; }
    public decimal daily_prod_KWH { get; set; }
    public decimal? total_sold_PV { get; set; }
    public decimal consumption_daily { get; set; }
    public decimal? realtime_cons_W { get; set; }
    public string battery_type { get; set; }
    public decimal battery_temp { get; set; }
    public decimal? buy_energy_total { get; set; }
    public string status_battery_flow { get; set; }
}