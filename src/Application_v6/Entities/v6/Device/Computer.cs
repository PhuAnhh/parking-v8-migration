namespace Application_v6.Entities.v6.Device;
public class Computer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}