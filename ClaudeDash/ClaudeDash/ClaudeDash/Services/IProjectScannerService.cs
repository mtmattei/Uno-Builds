namespace ClaudeDash.Services;

public interface IProjectScannerService
{
    Task<List<ProjectInfo>> GetAllProjectsAsync();
}
