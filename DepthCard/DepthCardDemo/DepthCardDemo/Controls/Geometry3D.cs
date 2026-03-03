using System;
using System.Collections.Generic;
using System.Numerics;

namespace DepthCardDemo.Controls;

/// <summary>
/// Represents a 3D vertex with position coordinates and UV texture coordinates.
/// </summary>
public struct Vertex3D
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float U { get; set; }
    public float V { get; set; }

    public Vertex3D(float x, float y, float z, float u = 0, float v = 0)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
    }

    public Vector3 ToVector3() => new Vector3(X, Y, Z);
}

/// <summary>
/// Represents a triangular face with three vertex indices.
/// </summary>
public struct Face3D
{
    public int V0 { get; set; }
    public int V1 { get; set; }
    public int V2 { get; set; }

    public Face3D(int v0, int v1, int v2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
    }
}

/// <summary>
/// Represents a 3D mesh with vertices and faces.
/// </summary>
public class Mesh3D
{
    public List<Vertex3D> Vertices { get; set; } = new();
    public List<Face3D> Faces { get; set; } = new();

    /// <summary>
    /// Creates a cube mesh centered at origin with specified size.
    /// </summary>
    public static Mesh3D CreateCube(float size = 1.0f)
    {
        var mesh = new Mesh3D();
        var half = size / 2;

        // 8 vertices of a cube
        mesh.Vertices.AddRange(new[]
        {
            new Vertex3D(-half, -half, -half), // 0: back-bottom-left
            new Vertex3D( half, -half, -half), // 1: back-bottom-right
            new Vertex3D( half,  half, -half), // 2: back-top-right
            new Vertex3D(-half,  half, -half), // 3: back-top-left
            new Vertex3D(-half, -half,  half), // 4: front-bottom-left
            new Vertex3D( half, -half,  half), // 5: front-bottom-right
            new Vertex3D( half,  half,  half), // 6: front-top-right
            new Vertex3D(-half,  half,  half), // 7: front-top-left
        });

        // 12 triangular faces (2 per cube face)
        mesh.Faces.AddRange(new[]
        {
            // Front face
            new Face3D(4, 5, 6),
            new Face3D(4, 6, 7),
            // Back face
            new Face3D(1, 0, 3),
            new Face3D(1, 3, 2),
            // Top face
            new Face3D(7, 6, 2),
            new Face3D(7, 2, 3),
            // Bottom face
            new Face3D(0, 1, 5),
            new Face3D(0, 5, 4),
            // Right face
            new Face3D(5, 1, 2),
            new Face3D(5, 2, 6),
            // Left face
            new Face3D(0, 4, 7),
            new Face3D(0, 7, 3),
        });

        return mesh;
    }

    /// <summary>
    /// Creates a pyramid mesh centered at origin with specified size.
    /// </summary>
    public static Mesh3D CreatePyramid(float size = 1.0f)
    {
        var mesh = new Mesh3D();
        var half = size / 2;

        // 5 vertices: 4 base corners + 1 apex
        mesh.Vertices.AddRange(new[]
        {
            new Vertex3D(-half, -half, -half), // 0: base back-left
            new Vertex3D( half, -half, -half), // 1: base back-right
            new Vertex3D( half, -half,  half), // 2: base front-right
            new Vertex3D(-half, -half,  half), // 3: base front-left
            new Vertex3D(0,      half,  0),    // 4: apex
        });

        // 6 triangular faces
        mesh.Faces.AddRange(new[]
        {
            // Base (2 triangles)
            new Face3D(0, 2, 1),
            new Face3D(0, 3, 2),
            // Side faces
            new Face3D(0, 1, 4), // back
            new Face3D(1, 2, 4), // right
            new Face3D(2, 3, 4), // front
            new Face3D(3, 0, 4), // left
        });

        return mesh;
    }

    /// <summary>
    /// Creates an octahedron (8-sided polyhedron) centered at origin.
    /// </summary>
    public static Mesh3D CreateOctahedron(float size = 1.0f)
    {
        var mesh = new Mesh3D();
        var r = size / 2;

        // 6 vertices (top, bottom, and 4 around the middle)
        mesh.Vertices.AddRange(new[]
        {
            new Vertex3D(0,   r,  0), // 0: top
            new Vertex3D(0,  -r,  0), // 1: bottom
            new Vertex3D( r,  0,  0), // 2: right
            new Vertex3D(-r,  0,  0), // 3: left
            new Vertex3D(0,   0,  r), // 4: front
            new Vertex3D(0,   0, -r), // 5: back
        });

        // 8 triangular faces
        mesh.Faces.AddRange(new[]
        {
            // Top pyramid
            new Face3D(0, 4, 2),
            new Face3D(0, 2, 5),
            new Face3D(0, 5, 3),
            new Face3D(0, 3, 4),
            // Bottom pyramid
            new Face3D(1, 2, 4),
            new Face3D(1, 5, 2),
            new Face3D(1, 3, 5),
            new Face3D(1, 4, 3),
        });

        return mesh;
    }

    /// <summary>
    /// Creates a diamond/gem shape with multiple facets.
    /// </summary>
    public static Mesh3D CreateDiamond(float size = 1.0f)
    {
        var mesh = new Mesh3D();
        var r = size / 2;

        // Vertices for a diamond shape
        mesh.Vertices.AddRange(new[]
        {
            new Vertex3D(0,      r * 1.4f,  0),      // 0: top apex
            new Vertex3D(0,     -r * 1.4f,  0),      // 1: bottom apex
            new Vertex3D( r,     r * 0.3f,  0),      // 2: upper-right
            new Vertex3D(-r,     r * 0.3f,  0),      // 3: upper-left
            new Vertex3D(0,      r * 0.3f,  r),      // 4: upper-front
            new Vertex3D(0,      r * 0.3f, -r),      // 5: upper-back
            new Vertex3D( r * 0.6f, -r * 0.2f,  0),  // 6: lower-right
            new Vertex3D(-r * 0.6f, -r * 0.2f,  0),  // 7: lower-left
            new Vertex3D(0,     -r * 0.2f,  r * 0.6f), // 8: lower-front
            new Vertex3D(0,     -r * 0.2f, -r * 0.6f), // 9: lower-back
        });

        // Faces
        mesh.Faces.AddRange(new[]
        {
            // Top crown
            new Face3D(0, 4, 2),
            new Face3D(0, 2, 5),
            new Face3D(0, 5, 3),
            new Face3D(0, 3, 4),
            // Upper pavilion
            new Face3D(2, 4, 6),
            new Face3D(4, 8, 6),
            new Face3D(4, 3, 8),
            new Face3D(3, 7, 8),
            new Face3D(3, 5, 7),
            new Face3D(5, 9, 7),
            new Face3D(5, 2, 9),
            new Face3D(2, 6, 9),
            // Bottom pavilion
            new Face3D(1, 6, 8),
            new Face3D(1, 8, 7),
            new Face3D(1, 7, 9),
            new Face3D(1, 9, 6),
        });

        return mesh;
    }

    /// <summary>
    /// Creates an icosahedron (20-sided polyhedron with 12 vertices).
    /// Used for the luxury invitation card design.
    /// </summary>
    public static Mesh3D CreateIcosahedron(float size = 1.0f)
    {
        var mesh = new Mesh3D();

        // Golden ratio constant for icosahedron
        var phi = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;
        var scale = size / MathF.Sqrt(1 + phi * phi);

        // 12 vertices of an icosahedron
        // Arranged in 3 groups: top vertex, upper ring, lower ring, bottom vertex
        mesh.Vertices.AddRange(new[]
        {
            // Top vertex
            new Vertex3D(0, phi * scale, scale),                    // 0

            // Upper ring (5 vertices)
            new Vertex3D(phi * scale, scale, 0),                    // 1
            new Vertex3D(phi * scale, -scale, 0),                   // 2
            new Vertex3D(0, -phi * scale, scale),                   // 3
            new Vertex3D(-phi * scale, -scale, 0),                  // 4
            new Vertex3D(-phi * scale, scale, 0),                   // 5

            // Lower ring (5 vertices)
            new Vertex3D(0, phi * scale, -scale),                   // 6
            new Vertex3D(scale, 0, -phi * scale),                   // 7
            new Vertex3D(-scale, 0, -phi * scale),                  // 8
            new Vertex3D(-scale, 0, phi * scale),                   // 9
            new Vertex3D(scale, 0, phi * scale),                    // 10

            // Bottom vertex
            new Vertex3D(0, -phi * scale, -scale),                  // 11
        });

        // 20 triangular faces
        mesh.Faces.AddRange(new[]
        {
            // Top cap (5 faces around vertex 0)
            new Face3D(0, 10, 1),
            new Face3D(0, 1, 6),
            new Face3D(0, 6, 5),
            new Face3D(0, 5, 9),
            new Face3D(0, 9, 10),

            // Upper belt (5 faces)
            new Face3D(1, 10, 2),
            new Face3D(6, 1, 7),
            new Face3D(5, 6, 8),
            new Face3D(9, 5, 4),
            new Face3D(10, 9, 3),

            // Lower belt (5 faces)
            new Face3D(2, 10, 3),
            new Face3D(7, 1, 2),
            new Face3D(8, 6, 7),
            new Face3D(4, 5, 8),
            new Face3D(3, 9, 4),

            // Bottom cap (5 faces around vertex 11)
            new Face3D(11, 2, 3),
            new Face3D(11, 7, 2),
            new Face3D(11, 8, 7),
            new Face3D(11, 4, 8),
            new Face3D(11, 3, 4),
        });

        return mesh;
    }

    /// <summary>
    /// Applies a transformation matrix to all vertices in the mesh.
    /// </summary>
    public void Transform(Matrix4x4 matrix)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            var v = Vertices[i];
            var transformed = Vector3.Transform(v.ToVector3(), matrix);
            Vertices[i] = new Vertex3D(transformed.X, transformed.Y, transformed.Z);
        }
    }

    /// <summary>
    /// Creates a deep copy of the mesh.
    /// </summary>
    public Mesh3D Clone()
    {
        var clone = new Mesh3D();
        clone.Vertices.AddRange(Vertices);
        clone.Faces.AddRange(Faces);
        return clone;
    }
}
