namespace parking_v6_to_v8_migration.Entities.v6;

public class EventOut
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid LaneId { get; set; }
    public string PlateNumber { get; set; }
    public long Charge { get; set; }
    public long DiscountAmount { get; set; }
    public string Note { get; set; }
    public Guid EventInId { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string CreatedBy { get; set; }
}