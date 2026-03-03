using Refit;
using VTrack.DataContracts;
using VTrack.DataContracts.Serialization;

namespace VTrack.Services.Endpoints;

[Headers("Content-Type: application/json")]
public interface IApiClient
{
    // Video endpoints
    [Get("/api/videos")]
    Task<ApiResponse<IImmutableList<VideoFile>>> GetVideos(CancellationToken cancellationToken = default);

    [Get("/api/videos/{id}")]
    Task<ApiResponse<VideoFile>> GetVideo(string id, CancellationToken cancellationToken = default);

    // Tracking endpoints
    [Post("/api/tracking/start")]
    Task<ApiResponse<TrackingJob>> StartTracking([Body] StartTrackingRequest request, CancellationToken cancellationToken = default);

    [Get("/api/tracking/job/{jobId}")]
    Task<ApiResponse<TrackingJob>> GetJobStatus(string jobId, CancellationToken cancellationToken = default);

    [Get("/api/tracking/results/{jobId}")]
    Task<ApiResponse<TrackingResult>> GetTrackingResults(string jobId, CancellationToken cancellationToken = default);
}

