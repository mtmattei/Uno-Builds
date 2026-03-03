using VTrack.DataContracts;

namespace VTrack.Server.Apis;

internal static class VideoApi
{
    private const string Tag = "Video";
    private static readonly Dictionary<string, VideoFile> _videos = new();

    internal static WebApplication MapVideoApi(this WebApplication app)
    {
        app.MapPost("/api/videos/upload", UploadVideo)
            .WithTags(Tag)
            .WithName(nameof(UploadVideo))
            .DisableAntiforgery();

        app.MapGet("/api/videos", GetVideos)
            .WithTags(Tag)
            .WithName(nameof(GetVideos));

        app.MapGet("/api/videos/{id}", GetVideo)
            .WithTags(Tag)
            .WithName(nameof(GetVideo));

        return app;
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(VideoFile), 200)]
    private static async Task<IResult> UploadVideo(
        HttpRequest request,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(VideoApi));

        try
        {
            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("No file uploaded");
            }

            if (!file.ContentType.StartsWith("video/"))
            {
                return Results.BadRequest("Only video files are allowed");
            }

            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadsDir);

            // Save file
            var fileId = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(file.FileName);
            var savedFileName = $"{fileId}{extension}";
            var filePath = Path.Combine(uploadsDir, savedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var videoFile = new VideoFile(
                Id: fileId,
                Name: file.FileName,
                Duration: 0, // Would be extracted from video metadata
                ThumbnailUrl: null,
                VideoUrl: $"/uploads/{savedFileName}",
                UploadedAt: DateTime.UtcNow);

            _videos[fileId] = videoFile;

            logger.LogInformation("Video uploaded: {FileName} -> {Id}", file.FileName, fileId);

            return Results.Ok(videoFile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading video");
            return Results.Problem("Failed to upload video");
        }
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<VideoFile>), 200)]
    private static IResult GetVideos(ILoggerFactory loggerFactory)
    {
        return Results.Ok(_videos.Values.OrderByDescending(v => v.UploadedAt));
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(VideoFile), 200)]
    private static IResult GetVideo(string id, ILoggerFactory loggerFactory)
    {
        if (_videos.TryGetValue(id, out var video))
        {
            return Results.Ok(video);
        }
        return Results.NotFound();
    }
}
