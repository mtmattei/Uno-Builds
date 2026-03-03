using System.Text.Json;
using UnoVox.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace UnoVox.Services;

/// <summary>
/// Handles saving and loading voxel projects to/from JSON files
/// </summary>
public class ProjectFileService : IProjectFileService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ProjectFileService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Serializes a voxel grid to a VoxelProject
    /// </summary>
    public VoxelProject CreateProjectFromGrid(VoxelGrid grid, CameraController camera, 
        List<string> palette, ProjectMetadata? existingMetadata = null)
    {
        var project = new VoxelProject
        {
            GridSize = grid.Size,
            Voxels = grid.ActiveVoxels.Select(v => new VoxelData
            {
                X = v.X,
                Y = v.Y,
                Z = v.Z,
                Color = v.Color
            }).ToList(),
            Palette = palette,
            Camera = new CameraData
            {
                RotationX = camera.RotationX,
                RotationY = camera.RotationY,
                Zoom = camera.Zoom,
                PanX = camera.PanX,
                PanY = camera.PanY
            },
            Metadata = existingMetadata ?? new ProjectMetadata
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Author = Environment.UserName
            }
        };

        // Update modified time
        project.Metadata.Modified = DateTime.UtcNow;

        return project;
    }

    /// <summary>
    /// Loads a VoxelProject into a voxel grid and camera
    /// </summary>
    public void LoadProjectIntoGrid(VoxelProject project, VoxelGrid grid, CameraController camera)
    {
        // Clear existing grid
        grid.Clear();

        // Load voxels
        foreach (var voxelData in project.Voxels)
        {
            grid.PlaceVoxel(voxelData.X, voxelData.Y, voxelData.Z, voxelData.Color);
        }

        // Restore camera state if available
        if (project.Camera != null)
        {
            camera.RotationX = project.Camera.RotationX;
            camera.RotationY = project.Camera.RotationY;
            camera.Zoom = project.Camera.Zoom;
            camera.PanX = project.Camera.PanX;
            camera.PanY = project.Camera.PanY;
        }
    }

    /// <summary>
    /// Saves project to JSON file
    /// </summary>
    public async Task<StorageFile?> SaveProjectAsync(VoxelProject project, StorageFile? file = null)
    {
        try
        {
            // If no file provided, show save file picker
            if (file == null)
            {
                var savePicker = new FileSavePicker();
                
                // Get the window handle for the picker
                if (App.MainWindow != null)
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                }

                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Voxel Project", new List<string> { ".vox" });
                savePicker.SuggestedFileName = $"VoxelProject_{DateTime.Now:yyyyMMdd_HHmmss}";

                file = await savePicker.PickSaveFileAsync();
                if (file == null)
                    return null; // User cancelled
            }

            // Serialize to JSON
            var json = JsonSerializer.Serialize(project, _jsonOptions);

            // Write to file
            await FileIO.WriteTextAsync(file, json);

            return file;
        }
        catch (Exception ex)
        {
            // Log error (in real app, use proper logging)
            System.Diagnostics.Debug.WriteLine($"Error saving project: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads project from JSON file
    /// </summary>
    public async Task<(VoxelProject? project, StorageFile? file)> LoadProjectAsync()
    {
        try
        {
            var openPicker = new FileOpenPicker();
            
            // Get the window handle for the picker
            if (App.MainWindow != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
            }

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".vox");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null)
                return (null, null); // User cancelled

            // Read file
            var json = await FileIO.ReadTextAsync(file);

            // Deserialize
            var project = JsonSerializer.Deserialize<VoxelProject>(json, _jsonOptions);

            return (project, file);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading project: {ex.Message}");
            throw;
        }
    }
}
