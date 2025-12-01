namespace Application_TV.Entities.v6;

public class EventOut
{
    public Guid Id { get; set; }
    public Guid? IdentityId { get; set; }
    public Guid LaneId { get; set; }
    public string? PlateNumber { get; set; }
    public string PhysicalFileIds { get; set; }
    public Guid? EventInIdentityId { get; set; }
    public Guid EventInLaneId { get; set; }
    public string? EventInPlateNumber { get; set; }
    public DateTime EventInCreatedUtc { get; set; }
    public Guid EventInCreatedBy { get; set; }
    public string EventInPhysicalFileIds { get; set; }
    public long Charge { get; set; }
    public long Discount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? IdentityGroupId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? EventInIdentityGroupId { get; set; }
}