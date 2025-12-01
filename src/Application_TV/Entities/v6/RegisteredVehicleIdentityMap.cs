namespace Application_TV.Entities.v6;

public class RegisteredVehicleIdentityMap
{
    public int Id { get; set; }
    public Guid RegisteredVehicleId { get; set; }
    public Guid IdentityId { get; set; }
}