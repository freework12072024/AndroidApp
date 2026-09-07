public class UserDevice
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string DeviceId { get; set; } = "";

    public string PushIdentifier { get; set; } = "";

    public string Platform { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedAt { get; set; }
        = DateTime.UtcNow;
}