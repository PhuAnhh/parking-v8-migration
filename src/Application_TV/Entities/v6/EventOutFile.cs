namespace Application_v6.Entities.v6.Parking;

public class EventOutFile
{
    public Guid EventOutId { get; set; }
    public Guid FileId { get; set; }
    public string ImageType { get; set; }
    
    public EventOut EventOut { get; set; }
    public File File { get; set; }
}