namespace Application_TV.Entities.v6;

public class EventInFile
{
    public Guid EventInId { get; set; }
    public Guid FileId { get; set; }
    public string ImageType { get; set; }
    
    public EventIn EventIn { get; set; }
    public File File { get; set; }
}