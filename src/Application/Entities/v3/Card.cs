namespace Application.Entities.v3;

public class Card
{
    public Guid CardId { get; set; }
    public string CardNumber { get; set; }
    public string CardGroupID { get; set; }
    public bool IsLock { get; set; }
    public bool IsDelete { get; set; }
}