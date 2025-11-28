namespace Application_v6.Entities.v6.Parking;

public class File
{
    public Guid Id { get; set; }
    public string Bucket { get; set; }
    public string ObjectKey { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedUtc { get; set; }
    
    public ICollection<EventInFile> EventInImages { get; set; }
    public ICollection<EventOutFile> EventOutImages { get; set; }
}