namespace Application_v6.Entities.v6;

public class CustomerGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code  { get; set; }
    public Guid? ParentId { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}