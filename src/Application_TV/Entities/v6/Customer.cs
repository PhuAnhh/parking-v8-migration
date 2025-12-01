namespace Application_TV.Entities.v6;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code  { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? CustomerGroupId { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}