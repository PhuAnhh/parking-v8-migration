namespace Application_TV.Entities.v6;

public class EventIn
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid LaneId { get; set; }
    public string PlateNumber { get; set; }
    public long TotalPaid { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string CreatedBy { get; set; }
    
    public ICollection<EventInFile>  EventInFiles { get; set; }
}