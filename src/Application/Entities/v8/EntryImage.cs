namespace Application.Entities.v8;

public class EntryImage
{
    public Guid EntryId { get; set; }
    public string ObjectKey { get; set; }
    public string Type { get; set; }
    public Entry Entry { get; set; }
}