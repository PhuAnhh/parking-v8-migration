namespace Application_v6.Entities.v6.Parking;

public class EventInFile
{
    public Guid EventInId { get; set; }
    public Guid FileId { get; set; }
    public string ImageType { get; set; }
    
    public EventIn EventIn { get; set; }
    public File File { get; set; }
}