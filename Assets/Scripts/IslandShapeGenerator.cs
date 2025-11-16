using UnityEngine;

public enum IslandShapeType
{
    Circle,
    Crescent,
    Donut,
    Archipelago,
    NoiseWarped,
    Irregular      // NEW: natural random outline
}

[System.Serializable]
public class IslandShapeGenerator
{
    public IslandShapeType shapeType = IslandShapeType.Circle;

    [Header("General Settings")]
    [Tooltip("Base radius of the island (normalized 0–1 space).")]
    public float radius = 1f;

    [Tooltip("Inner radius for donut / crescent shapes.")]
    public float innerRadius = 0.4f;

    [Tooltip("Controls the opening gap of crescent shapes.")]
    public float bendAmount = 0.5f;

    [Tooltip("Recenters crescent so it's not pushed to one side.")]
    public float crescentCenterShift = 0.25f;

    [Header("Archipelago Settings")]
    [Tooltip("Number of sub-islands.")]
    public int islandCount = 3;

    [Tooltip("How far apart sub-islands are placed.")]
    public float archipelagoSpread = 0.3f; // 0.3 fits nicely in the grid

    [Header("Noise Warping")]
    public float warpStrength = 0.2f;
    public float warpFrequency = 3f;

    [Header("Irregular Outline Settings")]
    [Tooltip("How much the coastline deviates.")]
    public float irregularAmount = 0.2f;

    [Tooltip("Noise scale for irregular outline distortion.")]
    public float irregularFrequency = 2.0f;

    public float GetMask(float x, float y)
    {
        switch (shapeType)
        {
            default:
            case IslandShapeType.Circle:
                return CircleMask(x, y);

            case IslandShapeType.Crescent:
                return CrescentMask(x, y);

            case IslandShapeType.Donut:
                return DonutMask(x, y);

            case IslandShapeType.Archipelago:
                return ArchipelagoMask(x, y);

            case IslandShapeType.NoiseWarped:
                return NoiseWarpedMask(x, y);

            case IslandShapeType.Irregular:
                return IrregularMask(x, y);
        }
    }

    // -----------------------------
    // Base Circle
    // -----------------------------
    private float CircleMask(float x, float y)
    {
        float dist = Mathf.Sqrt(x * x + y * y);
        return Mathf.Clamp01(1f - dist / radius);
    }

    // -----------------------------
    // Donut / Ring Island
    // -----------------------------
    private float DonutMask(float x, float y)
    {
        float dist = Mathf.Sqrt(x * x + y * y);

        float outer = Mathf.Clamp01(1f - dist / radius);
        float inner = Mathf.Clamp01(1f - dist / innerRadius);

        float ring = Mathf.Clamp01(outer - inner);

        // Boost the ring edges to make donut shape clearer
        ring *= Mathf.SmoothStep(0.5f, 1f, ring);

        return ring;
    }

    // -----------------------------
    // Crescent Shape
    // -----------------------------
    private float CrescentMask(float x, float y)
    {
        // Recenter so the crescent doesn't push to one side
        float cx = x + crescentCenterShift;

        float main = CircleMask(cx, y);
        float shifted = CircleMask(cx - bendAmount, y);

        return Mathf.Clamp01(main - shifted);
    }

    // -----------------------------
    // Archipelago (Multiple islands)
    // -----------------------------
    private float ArchipelagoMask(float x, float y)
    {
        float mask = 0f;

        for (int i = 0; i < islandCount; i++)
        {
            float angle = i * Mathf.PI * 2f / islandCount;

            float offsetX = Mathf.Cos(angle) * archipelagoSpread;
            float offsetY = Mathf.Sin(angle) * archipelagoSpread;

            float dx = x - offsetX;
            float dy = y - offsetY;

            float islandShape = CircleMask(dx, dy);

            mask = Mathf.Max(mask, islandShape);
        }

        return mask;
    }

    // -----------------------------
    // Noise Warped Circle (Stylized)
    // -----------------------------
    private float NoiseWarpedMask(float x, float y)
    {
        float warpX = x + (Mathf.PerlinNoise(x * warpFrequency, y * warpFrequency) * 2f - 1f) * warpStrength;
        float warpY = y + (Mathf.PerlinNoise(y * warpFrequency, x * warpFrequency) * 2f - 1f) * warpStrength;

        return CircleMask(warpX, warpY);
    }

    // -----------------------------
    // Natural Irregular Island
    // (Like real coastlines)
    // -----------------------------
    private float IrregularMask(float x, float y)
    {
        // Base distance
        float dist = Mathf.Sqrt(x * x + y * y);

        // Noise-driven radius variation
        float noise = Mathf.PerlinNoise(x * irregularFrequency, y * irregularFrequency);
        float radiusVariation = irregularAmount * (noise - 0.5f);

        float effectiveRadius = radius + radiusVariation;

        return Mathf.Clamp01(1f - dist / effectiveRadius);
    }
}
