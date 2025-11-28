namespace Application_TV.Entities.v6;

public class EventOutFile
{
    public Guid EventOutId { get; set; }
    public Guid FileId { get; set; }
    public string ImageType { get; set; }
    
    public EventOut EventOut { get; set; }
    public File File { get; set; }
}