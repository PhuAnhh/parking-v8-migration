namespace parking_v6_to_v8_migration.Entities.v8;

public class AccessKeyCollection
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code  { get; set; }
    public string VehicleType { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}