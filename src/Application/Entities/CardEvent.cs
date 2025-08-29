namespace Application.Entities.CardEvent;

public class CardEvent
{
    public Guid Id { get; set; }
    public string EventCode { get; set; }
    public string CardNumber { get; set; }
    public DateTime DatetimeIn { get; set; }
    public string LaneIDIn { get; set; }
    public string UserIDIn { get; set; }
    public string PlateIn  { get; set; }
    public long Moneys { get; set; }
    public string CustomerName { get; set; }
    public bool IsDelete { get; set; }
}