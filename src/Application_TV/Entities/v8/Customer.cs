namespace Application_TV.Entities.v8;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public Guid? CollectionId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}