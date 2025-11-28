namespace Application_TV.Entities.v8.Resource;

public class Entry
{
    public Guid Id { get; set; }
    public string? PlateNumber { get; set; }
    public Guid AccessKeyId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid? CustomerId { get; set; }
    public bool Exited { get; set; }
    public long Amount { get; set; }
    public string? Note { get; set; }
    public bool Deleted { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}