using System.Collections.Generic;
using UnityEngine;

// Keep this in the global namespace to avoid attach problems.
// File must be named DebugDrawer.cs and the class must be public DebugDrawer.
public class DebugDrawer : MonoBehaviour
{
    // How long lines stay on screen (0 = one frame)
    public static float duration = 0f;

    // Draw a connected polyline (for outlines, shorelines, borders)
    public static void DrawPolyline(List<Vector3> points, Color color, bool closed = false)
    {
        if (points == null || points.Count < 2) return;
        for (int i = 0; i < points.Count - 1; i++)
            Debug.DrawLine(points[i], points[i + 1], color, duration);
        if (closed)
            Debug.DrawLine(points[points.Count - 1], points[0], color, duration);
    }

    // Draw individual points as small cross markers
    public static void DrawPoints(List<Vector3> points, Color color, float size = 0.1f)
    {
        if (points == null) return;
        foreach (var p in points)
        {
            Debug.DrawLine(p + Vector3.up * size, p - Vector3.up * size, color, duration);
            Debug.DrawLine(p + Vector3.right * size, p - Vector3.right * size, color, duration);
            Debug.DrawLine(p + Vector3.forward * size, p - Vector3.forward * size, color, duration);
        }
    }

    // Draw a bounding box using min/max
    public static void DrawBoundingBox(Vector3 min, Vector3 max, Color color)
    {
        Vector3 p1 = new Vector3(min.x, min.y, min.z);
        Vector3 p2 = new Vector3(max.x, min.y, min.z);
        Vector3 p3 = new Vector3(max.x, min.y, max.z);
        Vector3 p4 = new Vector3(min.x, min.y, max.z);

        Vector3 p5 = new Vector3(min.x, max.y, min.z);
        Vector3 p6 = new Vector3(max.x, max.y, min.z);
        Vector3 p7 = new Vector3(max.x, max.y, max.z);
        Vector3 p8 = new Vector3(min.x, max.y, max.z);

        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(p2, p3, color, duration);
        Debug.DrawLine(p3, p4, color, duration);
        Debug.DrawLine(p4, p1, color, duration);

        Debug.DrawLine(p5, p6, color, duration);
        Debug.DrawLine(p6, p7, color, duration);
        Debug.DrawLine(p7, p8, color, duration);
        Debug.DrawLine(p8, p5, color, duration);

        Debug.DrawLine(p1, p5, color, duration);
        Debug.DrawLine(p2, p6, color, duration);
        Debug.DrawLine(p3, p7, color, duration);
        Debug.DrawLine(p4, p8, color, duration);
    }

    // Draw normals for debugging mesh shading
    public static void DrawNormals(Vector3[] vertices, Vector3[] normals, Color color, float length = 0.2f)
    {
        if (vertices == null || normals == null) return;
        int len = Mathf.Min(vertices.Length, normals.Length);
        for (int i = 0; i < len; i++)
            Debug.DrawLine(vertices[i], vertices[i] + normals[i] * length, color, duration);
    }

    // Draw a simple line
    public static void DrawLine(Vector3 a, Vector3 b, Color color)
    {
        Debug.DrawLine(a, b, color, duration);
    }
}
