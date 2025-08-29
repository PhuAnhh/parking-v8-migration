namespace Application.Entities.AccessKey;

public class AccessKey
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public Guid? CollectionId { get; set; }
    public string Status { get; set; }
}