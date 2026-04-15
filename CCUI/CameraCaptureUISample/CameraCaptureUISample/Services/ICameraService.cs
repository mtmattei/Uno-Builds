using Windows.Storage;

namespace CameraCaptureUISample.Services;

public interface ICameraService
{
	Task<StorageFile?> CapturePhotoAsync(CancellationToken ct);
}
