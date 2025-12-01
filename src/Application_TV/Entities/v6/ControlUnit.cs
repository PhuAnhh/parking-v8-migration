namespace Application_TV.Entities.v6;

public class ControlUnit
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}