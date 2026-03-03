using System.Collections.Generic;

namespace ZaraApp.Models;

public class Product
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string ColorIndicator { get; set; } = string.Empty;
    public List<SizeOption> AvailableSizes { get; set; } = new();
}

public class SizeOption
{
    public string Size { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsComingSoon { get; set; } = false;
}
