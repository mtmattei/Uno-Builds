using System.Text.Json;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using InfiniteImage.Models;

namespace InfiniteImage.Services;

public class PhotoLibraryService
{
    private PhotoLibrary? _currentLibrary;
    private const string LibraryFileName = "photo-library.json";
    private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".heic" };

    public LibraryMode CurrentMode { get; private set; } = LibraryMode.Random;
    public PhotoLibrary? CurrentLibrary => _currentLibrary;

    public async Task<PhotoLibrary?> SelectAndScanFolderAsync(Window? window = null)
    {
        try
        {
            Console.WriteLine("SelectAndScanFolderAsync called");

            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.FileTypeFilter.Add("*");

            Console.WriteLine("FolderPicker created");

            // Initialize picker with window handle (required for Desktop/WinUI)
            if (window != null)
            {
                try
                {
                    Console.WriteLine("Attempting to get window handle...");
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    Console.WriteLine($"Window handle obtained: {hwnd}");

                    WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
                    Console.WriteLine("Folder picker initialized with window handle successfully");
                }
                catch (Exception initEx)
                {
                    Console.WriteLine($"ERROR initializing picker with window: {initEx.Message}");
                    Console.WriteLine($"Stack trace: {initEx.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("ERROR: No window provided for picker initialization!");
            }

            Console.WriteLine("Calling PickSingleFolderAsync...");
            var folder = await folderPicker.PickSingleFolderAsync();
            Console.WriteLine("PickSingleFolderAsync returned");

            if (folder == null)
            {
                Console.WriteLine("Folder picker cancelled (no folder selected)");
                return null;
            }

            Console.WriteLine($"Selected folder: {folder.Path}");
            return await ScanFolderAsync(folder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in SelectAndScanFolderAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private async Task<PhotoLibrary?> ScanFolderAsync(StorageFolder folder)
    {
        try
        {
            Console.WriteLine($"Scanning folder: {folder.Path}");
            var photos = new List<Photo>();

            // Recursively get all image files
            var files = await GetImageFilesRecursiveAsync(folder);
            Console.WriteLine($"Found {files.Count} image files");

            // Read EXIF data and create Photo objects
            var tasks = files.Select(async file =>
            {
                try
                {
                    DateTimeOffset dateTaken;
                    int width = 800;  // Default dimensions
                    int height = 600;

                    // Try to read EXIF date
                    var exifDate = await ReadExifDateAsync(file);
                    if (exifDate.HasValue)
                    {
                        dateTaken = exifDate.Value;
                    }
                    else
                    {
                        // Fall back to file date (parse from filename if possible, otherwise use modified date)
                        dateTaken = TryParseDateFromFilename(file.Name) ??
                                   (await file.GetBasicPropertiesAsync()).DateModified;
                    }

                    // Try to get image dimensions, but don't fail if not available
                    try
                    {
                        var properties = await file.Properties.GetImagePropertiesAsync();
                        if (properties.Width > 0) width = (int)properties.Width;
                        if (properties.Height > 0) height = (int)properties.Height;
                    }
                    catch
                    {
                        // Use default dimensions if reading fails
                    }

                    return new Photo
                    {
                        Id = Guid.NewGuid().ToString(),
                        FilePath = file.Path,
                        DateTaken = dateTaken,
                        Title = Path.GetFileNameWithoutExtension(file.Name),
                        Width = width,
                        Height = height,
                        ZCoordinate = 0 // Will be calculated after sorting
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {file.Name}: {ex.Message}");
                    return null;
                }
            });

            var photoResults = await Task.WhenAll(tasks);
            photos = photoResults.Where(p => p != null).ToList()!;

            if (photos.Count == 0)
            {
                Console.WriteLine("No valid photos found");
                return null;
            }

            // Sort by date
            photos = photos.OrderBy(p => p.DateTaken).ToList();

            // Calculate Z-coordinates
            var earliestDate = photos.First().DateTaken;
            var latestDate = photos.Last().DateTaken;

            foreach (var photo in photos)
            {
                photo.ZCoordinate = TimelineConfig.CalculateZForDate(photo.DateTaken, earliestDate);
            }

            var library = new PhotoLibrary
            {
                FolderPath = folder.Path,
                Photos = photos,
                EarliestDate = earliestDate,
                LatestDate = latestDate,
                TotalPhotos = photos.Count
            };

            // Save library
            await SaveLibraryAsync(library);

            // Set as current library
            _currentLibrary = library;
            CurrentMode = LibraryMode.Personal;

            Console.WriteLine($"Library created: {photos.Count} photos from {earliestDate:d} to {latestDate:d}");
            return library;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning folder: {ex.Message}");
            return null;
        }
    }

    private async Task<List<StorageFile>> GetImageFilesRecursiveAsync(StorageFolder folder)
    {
        var imageFiles = new List<StorageFile>();

        try
        {
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (_supportedExtensions.Contains(extension))
                {
                    imageFiles.Add(file);
                }
            }

            var subfolders = await folder.GetFoldersAsync();
            foreach (var subfolder in subfolders)
            {
                var subFiles = await GetImageFilesRecursiveAsync(subfolder);
                imageFiles.AddRange(subFiles);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading folder {folder.Path}: {ex.Message}");
        }

        return imageFiles;
    }

    private async Task<DateTimeOffset?> ReadExifDateAsync(StorageFile file)
    {
        try
        {
            using var stream = await file.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var properties = await decoder.BitmapProperties.GetPropertiesAsync(
                new[] { "System.Photo.DateTaken" });

            if (properties.TryGetValue("System.Photo.DateTaken", out var dateProp)
                && dateProp.Value != null)
            {
                if (DateTimeOffset.TryParse(dateProp.Value.ToString(), out var date))
                {
                    return date;
                }
            }
        }
        catch
        {
            // EXIF reading failed, will fall back to file date
        }

        return null;
    }

    private DateTimeOffset? TryParseDateFromFilename(string filename)
    {
        try
        {
            // Try to find date patterns in filename like "Screenshot 2025-03-17" or "IMG_20250317" or "2025-03-17"
            var patterns = new[]
            {
                @"(\d{4})-(\d{2})-(\d{2})",              // 2025-03-17
                @"(\d{4})(\d{2})(\d{2})",                // 20250317
                @"(\d{2})-(\d{2})-(\d{4})",              // 17-03-2025
                @"(\d{2})(\d{2})(\d{4})"                 // 17032025
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(filename, pattern);
                if (match.Success)
                {
                    // Try both year-first and day-first formats
                    if (match.Groups[1].Value.Length == 4) // Year first
                    {
                        if (DateTime.TryParse($"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}",
                            out var date))
                        {
                            return new DateTimeOffset(date);
                        }
                    }
                    else // Day first (assume DD-MM-YYYY)
                    {
                        if (DateTime.TryParse($"{match.Groups[3].Value}-{match.Groups[2].Value}-{match.Groups[1].Value}",
                            out var date))
                        {
                            return new DateTimeOffset(date);
                        }
                    }
                }
            }
        }
        catch
        {
            // Parsing failed
        }

        return null;
    }

    public async Task SaveLibraryAsync(PhotoLibrary library)
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.CreateFileAsync(LibraryFileName, CreationCollisionOption.ReplaceExisting);
            var json = JsonSerializer.Serialize(library, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(file, json);
            Console.WriteLine($"Library saved to {file.Path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving library: {ex.Message}");
        }
    }

    public async Task<PhotoLibrary?> LoadLibraryAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var file = await localFolder.TryGetItemAsync(LibraryFileName) as StorageFile;
            if (file == null)
                return null;

            var json = await FileIO.ReadTextAsync(file);
            var library = JsonSerializer.Deserialize<PhotoLibrary>(json);

            if (library != null && library.TotalPhotos > 0)
            {
                _currentLibrary = library;
                CurrentMode = LibraryMode.Personal;
                Console.WriteLine($"Library loaded: {library.TotalPhotos} photos");
                return library;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading library: {ex.Message}");
        }

        return null;
    }

    public Photo? GetPhotoById(string photoId)
    {
        return _currentLibrary?.Photos.FirstOrDefault(p => p.Id == photoId);
    }

    public Photo? GetPhotoForCoordinate(float z)
    {
        if (_currentLibrary == null || _currentLibrary.Photos.Count == 0)
            return null;

        // Find nearest photo to Z position
        return _currentLibrary.Photos
            .OrderBy(p => Math.Abs(p.ZCoordinate - z))
            .FirstOrDefault();
    }

    public List<Photo> GetPhotosInZRange(float zMin, float zMax)
    {
        if (_currentLibrary == null)
            return new List<Photo>();

        return _currentLibrary.Photos
            .Where(p => p.ZCoordinate >= zMin && p.ZCoordinate < zMax)
            .ToList();
    }

    public void ClearLibrary()
    {
        _currentLibrary = null;
        CurrentMode = LibraryMode.Random;
    }
}
