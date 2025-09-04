namespace Application.Entities.v3.Models;

public class ExitCardEventDto
{
    public Guid Id { get; set; }
    public string EventCode { get; set; }
    public string CardNumber { get; set; }
    public DateTime DateTimeOut { get; set; }
    public string LaneIDOut { get; set; }
    public string UserIDOut { get; set; }
    public string PlateOut { get; set; }
    public decimal Moneys { get; set; }
    public string CustomerName { get; set; }
    public bool IsDelete { get; set; }
    public decimal ReducedMoney { get; set; }
}