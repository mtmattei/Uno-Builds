using Windows.Storage;

namespace UnoVox.Services;

public interface IProjectFileService
{
    Task<(VoxelProject? project, StorageFile? file)> LoadProjectAsync();
    Task<StorageFile?> SaveProjectAsync(VoxelProject project, StorageFile? file = null);
    VoxelProject CreateProjectFromGrid(VoxelGrid grid, CameraController camera, List<string> palette, ProjectMetadata? existingMetadata = null);
    void LoadProjectIntoGrid(VoxelProject project, VoxelGrid grid, CameraController camera);
}
