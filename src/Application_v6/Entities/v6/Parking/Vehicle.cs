namespace Application_v6.Entities.v6.Parking;

public class Vehicle
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PlateNumber { get; set; }
    public string VehicleType { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime? ExpireUtc { get; set; }
    public DateTime? LastActivatedUtc { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}