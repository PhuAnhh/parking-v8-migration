namespace Application_v6.Entities.v6.Device;

public class Computer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }

    public Gate Gate { get; set; }
    public ICollection<Camera> Cameras { get; set; }
    public ICollection<ControlUnit> ControlUnits { get; set; }
    public ICollection<Lane> Lanes { get; set; }
    public ICollection<Led> Leds { get; set; }
}