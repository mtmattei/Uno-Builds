namespace ClaudeDash.Models;

public class MemoryFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ProjectContext { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public long SizeBytes { get; set; }
}
