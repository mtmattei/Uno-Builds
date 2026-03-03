using System.Numerics;

namespace UnoVox.Models;

public record HandLandmark(int Index, float X, float Y, float Z)
{
    // X,Y are normalized [0,1] in image space, Z is relative depth
    
    /// <summary>
    /// Convert to Vector3 for math operations
    /// </summary>
    public Vector3 ToVector3() => new Vector3(X, Y, Z);
}

public class HandDetection
{
    public IReadOnlyList<HandLandmark> Landmarks { get; }
    public int HandId { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public HandDetection(int handId, IEnumerable<HandLandmark> landmarks)
    {
        HandId = handId;
        Landmarks = landmarks.ToList();
    }
    
    /// <summary>
    /// Get palm center position (average of wrist and MCP joints)
    /// </summary>
    public Vector3 GetPalmCenter()
    {
        if (Landmarks.Count < 21) return Vector3.Zero;
        
        var wrist = Landmarks[0].ToVector3();
        var indexMCP = Landmarks[5].ToVector3();
        var middleMCP = Landmarks[9].ToVector3();
        var ringMCP = Landmarks[13].ToVector3();
        var pinkyMCP = Landmarks[17].ToVector3();
        
        return (wrist + indexMCP + middleMCP + ringMCP + pinkyMCP) / 5.0f;
    }
    
    /// <summary>
    /// Get palm normal vector (perpendicular to palm surface)
    /// </summary>
    public Vector3 GetPalmNormal()
    {
        if (Landmarks.Count < 21) return Vector3.UnitZ;
        
        var wrist = Landmarks[0].ToVector3();
        var indexMCP = Landmarks[5].ToVector3();
        var pinkyMCP = Landmarks[17].ToVector3();
        
        Vector3 v1 = indexMCP - wrist;
        Vector3 v2 = pinkyMCP - wrist;
        
        return Vector3.Normalize(Vector3.Cross(v1, v2));
    }
}
