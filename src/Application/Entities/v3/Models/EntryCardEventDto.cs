namespace Application.Entities.v3.Models;

public class EntryCardEventDto
{
    public Guid Id { get; set; }
    public string EventCode { get; set; }
    public string CardNumber { get; set; }
    public DateTime DatetimeIn { get; set; }
    public string? PicDirIn { get; set; }
    public string LaneIDIn { get; set; }
    public string UserIDIn { get; set; }
    public string PlateIn  { get; set; }
    public decimal Moneys { get; set; }
    public string CustomerName { get; set; }
    public bool IsDelete { get; set; }
}