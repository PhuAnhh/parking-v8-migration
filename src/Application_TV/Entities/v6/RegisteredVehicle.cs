namespace Application_TV.Entities.v6;

public class RegisteredVehicle
{
    public Guid Id { get; set; }
    public string Name { get; set; }   
    public string PlateNumber { get; set; }
    public int VehicleTypeId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime ExpireUtc { get; set; }
    public DateTime LastActivatedUtc { get; set; }
    public bool Enabled { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}