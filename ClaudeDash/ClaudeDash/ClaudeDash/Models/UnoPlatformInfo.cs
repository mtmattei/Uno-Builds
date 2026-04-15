namespace ClaudeDash.Models;

public class UnoPlatformInfo
{
    public string SdkVersion { get; set; } = "";
    public string DotNetSdkVersion { get; set; } = "";
    public string UnoWinUIVersion { get; set; } = "";
    public string RendererType { get; set; } = "";
    public string HotReloadStatus { get; set; } = "unknown";
    public List<string> UnoFeatures { get; set; } = new();
    public List<NuGetPackageInfo> Packages { get; set; } = new();
    public List<string> TargetFrameworks { get; set; } = new();
    public string UnoCheckStatus { get; set; } = "unknown";
    public string UnoCheckDetail { get; set; } = "";
    public List<DotNetWorkloadInfo> Workloads { get; set; } = new();
    public string DetectedIde { get; set; } = "";
    public string IdeVersion { get; set; } = "";
    public int ProjectCount { get; set; }
    public string ProjectName { get; set; } = "";
    public bool IsWasmAotEnabled { get; set; }
    public bool IsSingleProject { get; set; }
    public string HotDesignStatus { get; set; } = "unknown";
    public string LicenseTier { get; set; } = "unknown";
}

public class NuGetPackageInfo
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public bool IsUnoPackage { get; set; }
}

public class DotNetWorkloadInfo
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
}
