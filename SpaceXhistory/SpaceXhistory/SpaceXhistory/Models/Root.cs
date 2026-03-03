using System.Text.Json.Serialization;

namespace SpaceXhistory.Models;

public class Root
{
    public Links? links { get; set; }
    public bool? success { get; set; }
    public string? name { get; set; }
    public DateTime date_utc { get; set; }
    public DateTime date_local { get; set; }
    public bool upcoming { get; set; }

    [JsonIgnore]
    public string Status
    {
        get
        {
            if (upcoming)
            {
                return "upcoming";
            }

            if (success == true)
            {
                return "successful";
            }

            return "failed";
        }
    }

    [JsonIgnore]
    public string StatusColor
    {
        get
        {
            if (upcoming)
            {
                return "#3498db"; // Blue for upcoming
            }

            if (success == true)
            {
                return "#2ecc71"; // Green for successful
            }

            return "#e74c3c"; // Red for failed
        }
    }
}
