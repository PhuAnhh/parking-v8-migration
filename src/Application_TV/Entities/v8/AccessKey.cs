namespace Application_TV.Entities.v8.Resource;

public class AccessKey
{
    public Guid Id { get; set; }
    public string Name  { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public Guid CollectionId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? ExpiredUtc { get; set; }
    public string Status { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}