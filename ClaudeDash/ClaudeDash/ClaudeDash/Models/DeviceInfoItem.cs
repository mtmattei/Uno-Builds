namespace ClaudeDash.Models;

public class DeviceInfoItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = "ok"; // ok, warn, error

    public DeviceInfoItem() { }

    public DeviceInfoItem(string label, string value, string status)
    {
        Label = label;
        Value = value;
        Status = status;
    }
}
