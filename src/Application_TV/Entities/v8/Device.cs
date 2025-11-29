namespace Application_TV.Entities.v8;

public class Device
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Code  { get; set; }
    public string Type { get; set; }
    public Guid? ParentId { get; set; }
    public bool Enabled { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}