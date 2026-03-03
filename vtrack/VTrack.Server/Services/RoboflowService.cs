using System.Net.Http.Headers;
using System.Text.Json;
using VTrack.DataContracts;

namespace VTrack.Server.Services;

public interface IRoboflowService
{
    Task<List<RoboflowDetection>> DetectObjectsAsync(string imagePath, string query, CancellationToken ct = default);
    Task<List<RoboflowDetection>> DetectObjectsFromBase64Async(string base64Image, string query, CancellationToken ct = default);
}

public record RoboflowDetection(
    string Class,
    double Confidence,
    double X,
    double Y,
    double Width,
    double Height);

public class RoboflowService : IRoboflowService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RoboflowService> _logger;

    public RoboflowService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RoboflowService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<RoboflowDetection>> DetectObjectsAsync(
        string imagePath,
        string query,
        CancellationToken ct = default)
    {
        var imageBytes = await File.ReadAllBytesAsync(imagePath, ct);
        var base64Image = Convert.ToBase64String(imageBytes);
        return await DetectObjectsFromBase64Async(base64Image, query, ct);
    }

    public async Task<List<RoboflowDetection>> DetectObjectsFromBase64Async(
        string base64Image,
        string query,
        CancellationToken ct = default)
    {
        var apiKey = _configuration["Roboflow:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Roboflow API key not configured");
            return new List<RoboflowDetection>();
        }

        try
        {
            // Using Roboflow's YOLO-World for open-vocabulary detection
            // This allows natural language queries like "person wearing yellow jersey"
            var url = $"https://detect.roboflow.com/yolo-world/1?api_key={apiKey}&classes={Uri.EscapeDataString(query)}";

            var content = new StringContent(base64Image);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await _httpClient.PostAsync(url, content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Roboflow API error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                return new List<RoboflowDetection>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<RoboflowResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Predictions == null)
            {
                return new List<RoboflowDetection>();
            }

            return result.Predictions.Select(p => new RoboflowDetection(
                Class: p.Class,
                Confidence: p.Confidence,
                X: p.X / result.Image.Width,  // Normalize to 0-1
                Y: p.Y / result.Image.Height,
                Width: p.Width / result.Image.Width,
                Height: p.Height / result.Image.Height
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Roboflow API");
            return new List<RoboflowDetection>();
        }
    }
}

// Roboflow API response models
public class RoboflowResponse
{
    public RoboflowImage Image { get; set; } = new();
    public List<RoboflowPrediction> Predictions { get; set; } = new();
}

public class RoboflowImage
{
    public double Width { get; set; }
    public double Height { get; set; }
}

public class RoboflowPrediction
{
    public string Class { get; set; } = "";
    public double Confidence { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
