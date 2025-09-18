namespace Application_v6.Entities.v8;

public class CustomerCollection
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public bool Deleted { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}