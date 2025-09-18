namespace Application_v6.Entities.v6;

public class Identity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code  { get; set; }
    public Guid IdentityGroupId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}