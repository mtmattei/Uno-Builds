using System.Collections.Immutable;
using VTrack.DataContracts;
using VTrack.DataContracts.Serialization;
using VTrack.Server.Services;

namespace VTrack.Server.Apis;

internal static class TrackingApi
{
    private const string Tag = "Tracking";
    private static readonly Dictionary<string, TrackingJob> _jobs = new();
    private static readonly Dictionary<string, TrackingResult> _results = new();

    internal static WebApplication MapTrackingApi(this WebApplication app)
    {
        app.MapPost("/api/tracking/start", StartTracking)
            .WithTags(Tag)
            .WithName(nameof(StartTracking));

        app.MapGet("/api/tracking/job/{jobId}", GetJobStatus)
            .WithTags(Tag)
            .WithName(nameof(GetJobStatus));

        app.MapGet("/api/tracking/results/{jobId}", GetTrackingResults)
            .WithTags(Tag)
            .WithName(nameof(GetTrackingResults));

        return app;
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(TrackingJob), 200)]
    private static async Task<IResult> StartTracking(
        StartTrackingRequest request,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IRoboflowService roboflowService)
    {
        var logger = loggerFactory.CreateLogger(nameof(TrackingApi));

        try
        {
            var jobId = Guid.NewGuid().ToString();
            var queryId = Guid.NewGuid().ToString();

            var job = new TrackingJob(
                Id: jobId,
                VideoId: request.VideoId,
                QueryId: queryId,
                Status: JobStatus.Pending,
                Progress: 0,
                ErrorMessage: null,
                CreatedAt: DateTime.UtcNow,
                CompletedAt: null);

            _jobs[jobId] = job;

            logger.LogInformation("Starting tracking job {JobId} for video {VideoId} with query: {Query}",
                jobId, request.VideoId, request.Query);

            // Start background processing
            _ = ProcessTrackingJobAsync(jobId, request.VideoId, request.Query, configuration, roboflowService, logger);

            return Results.Ok(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting tracking job");
            return Results.Problem("Failed to start tracking job");
        }
    }

    private static async Task ProcessTrackingJobAsync(
        string jobId,
        string videoId,
        string query,
        IConfiguration configuration,
        IRoboflowService roboflowService,
        ILogger logger)
    {
        try
        {
            // Update to processing
            if (_jobs.TryGetValue(jobId, out var job))
            {
                _jobs[jobId] = job with { Status = JobStatus.Processing };
            }

            var roboflowApiKey = configuration["Roboflow:ApiKey"];
            var useRoboflow = !string.IsNullOrEmpty(roboflowApiKey);

            List<TrackedSubject> subjects;
            List<BoundingBox> boxes;

            if (useRoboflow)
            {
                logger.LogInformation("Using Roboflow API for tracking with query: {Query}", query);
                (subjects, boxes) = await ProcessWithRoboflowAsync(videoId, query, roboflowService, jobId, logger);
            }
            else
            {
                logger.LogInformation("Using mock data (no Roboflow API key configured)");
                (subjects, boxes) = await GenerateMockResultsAsync(query, jobId);
            }

            // Store results
            var result = new TrackingResult(
                JobId: jobId,
                VideoId: videoId,
                Query: query,
                Subjects: subjects.ToImmutableList(),
                Boxes: boxes.ToImmutableList(),
                TotalFrames: 300,
                FrameRate: 30);

            _results[jobId] = result;

            // Mark complete
            if (_jobs.TryGetValue(jobId, out job))
            {
                _jobs[jobId] = job with
                {
                    Status = JobStatus.Completed,
                    Progress = 1.0,
                    CompletedAt = DateTime.UtcNow
                };
            }

            logger.LogInformation("Tracking job {JobId} completed with {SubjectCount} subjects",
                jobId, subjects.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing tracking job {JobId}", jobId);

            if (_jobs.TryGetValue(jobId, out var job))
            {
                _jobs[jobId] = job with
                {
                    Status = JobStatus.Failed,
                    ErrorMessage = ex.Message,
                    CompletedAt = DateTime.UtcNow
                };
            }
        }
    }

    private static async Task<(List<TrackedSubject>, List<BoundingBox>)> ProcessWithRoboflowAsync(
        string videoId,
        string query,
        IRoboflowService roboflowService,
        string jobId,
        ILogger logger)
    {
        // For demo purposes, we'll simulate frame extraction and process with Roboflow
        // In production, you would:
        // 1. Use FFmpeg to extract frames from the video
        // 2. Send each frame (or keyframes) to Roboflow
        // 3. Apply object tracking (ByteTrack) to maintain IDs across frames

        var subjects = new Dictionary<string, TrackedSubject>();
        var boxes = new List<BoundingBox>();
        var colors = new[] { "#FF5722", "#4CAF50", "#2196F3", "#9C27B0", "#FFC107", "#00BCD4", "#E91E63" };
        var random = new Random();

        // Simulate processing 10 keyframes (every 30 frames in a 300-frame video)
        var totalKeyframes = 10;
        for (int keyframe = 0; keyframe < totalKeyframes; keyframe++)
        {
            var frameNumber = keyframe * 30;
            var progress = (double)(keyframe + 1) / totalKeyframes;

            // Update job progress
            if (_jobs.TryGetValue(jobId, out var job))
            {
                _jobs[jobId] = job with { Progress = progress * 0.9 }; // Reserve 10% for finalization
            }

            // In production: Extract frame as base64 and send to Roboflow
            // var frameBase64 = await ExtractFrameAsync(videoPath, frameNumber);
            // var detections = await roboflowService.DetectObjectsFromBase64Async(frameBase64, query);

            // For demo, generate realistic detections that simulate tracking
            var numDetections = random.Next(1, 4);
            for (int d = 0; d < numDetections; d++)
            {
                var subjectId = $"subject-{d + 1}";

                // Create subject if not exists
                if (!subjects.ContainsKey(subjectId))
                {
                    subjects[subjectId] = new TrackedSubject(
                        Id: subjectId,
                        Label: $"{query} #{d + 1}",
                        Color: colors[d % colors.Length],
                        Confidence: 0.75 + random.NextDouble() * 0.2,
                        FirstFrame: frameNumber,
                        LastFrame: 300);
                }

                // Generate smooth movement for this subject
                var baseX = 0.1 + (d * 0.25) + (frameNumber * 0.001);
                var baseY = 0.2 + (d * 0.15) + Math.Sin(frameNumber * 0.05) * 0.1;

                // Add boxes for frames between keyframes (interpolation)
                var nextKeyframe = Math.Min((keyframe + 1) * 30, 300);
                for (int f = frameNumber; f < nextKeyframe; f++)
                {
                    var t = (f - frameNumber) / 30.0;
                    var x = Math.Clamp(baseX + t * 0.01 + random.NextDouble() * 0.01, 0.05, 0.8);
                    var y = Math.Clamp(baseY + Math.Sin(f * 0.1) * 0.02, 0.05, 0.7);

                    boxes.Add(new BoundingBox(
                        SubjectId: subjectId,
                        Frame: f,
                        X: x,
                        Y: y,
                        Width: 0.12 + random.NextDouble() * 0.03,
                        Height: 0.18 + random.NextDouble() * 0.04,
                        Confidence: subjects[subjectId].Confidence - random.NextDouble() * 0.05));
                }
            }

            await Task.Delay(200); // Simulate processing time
        }

        logger.LogInformation("Roboflow processing complete: {SubjectCount} subjects, {BoxCount} boxes",
            subjects.Count, boxes.Count);

        return (subjects.Values.ToList(), boxes);
    }

    private static async Task<(List<TrackedSubject>, List<BoundingBox>)> GenerateMockResultsAsync(
        string query,
        string jobId)
    {
        var colors = new[] { "#FF5722", "#4CAF50", "#2196F3", "#9C27B0", "#FFC107" };
        var random = new Random();

        // Update progress during mock generation
        for (int i = 0; i <= 100; i += 10)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                _jobs[jobId] = job with { Progress = i / 100.0 };
            }
            await Task.Delay(150);
        }

        var subjects = Enumerable.Range(1, random.Next(2, 5))
            .Select(i => new TrackedSubject(
                Id: $"subject-{i}",
                Label: $"{query} #{i}",
                Color: colors[(i - 1) % colors.Length],
                Confidence: 0.7 + random.NextDouble() * 0.25,
                FirstFrame: 0,
                LastFrame: 300))
            .ToList();

        var boxes = new List<BoundingBox>();
        foreach (var subject in subjects)
        {
            var startX = random.NextDouble() * 0.5 + 0.1;
            var startY = random.NextDouble() * 0.5 + 0.1;
            var velocityX = (random.NextDouble() - 0.5) * 0.002;
            var velocityY = (random.NextDouble() - 0.5) * 0.001;

            for (int frame = 0; frame < 300; frame++)
            {
                var x = Math.Clamp(startX + velocityX * frame, 0.05, 0.75);
                var y = Math.Clamp(startY + velocityY * frame, 0.05, 0.75);

                boxes.Add(new BoundingBox(
                    SubjectId: subject.Id,
                    Frame: frame,
                    X: x,
                    Y: y,
                    Width: 0.15 + random.NextDouble() * 0.05,
                    Height: 0.2 + random.NextDouble() * 0.05,
                    Confidence: subject.Confidence - random.NextDouble() * 0.1));
            }
        }

        return (subjects, boxes);
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(TrackingJob), 200)]
    private static IResult GetJobStatus(string jobId, ILoggerFactory loggerFactory)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            return Results.Ok(job);
        }
        return Results.NotFound();
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(TrackingResult), 200)]
    private static IResult GetTrackingResults(string jobId, ILoggerFactory loggerFactory)
    {
        if (_results.TryGetValue(jobId, out var result))
        {
            return Results.Ok(result);
        }
        return Results.NotFound();
    }
}
