namespace Application.Entities.v8;

public class MinioSettings
{
    public string Bucket { get; set; }
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}