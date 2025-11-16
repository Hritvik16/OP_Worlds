using UnityEngine;

public enum IslandShapeType
{
    Circle,
    Crescent,
    Donut,
    Archipelago,
    NoiseWarped
}

[System.Serializable]
public class IslandShapeGenerator
{
    public IslandShapeType shapeType = IslandShapeType.Circle;

    [Header("General Settings")]
    public float radius = 1f;
    public float innerRadius = 0.4f;     // for donut & crescent
    public float bendAmount = 0.5f;      // for crescent
    public int islandCount = 3;          // for archipelago
    public float warpStrength = 0.2f;    // for noise warping
    public float warpFrequency = 3f;

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
        }
    }

    private float CircleMask(float x, float y)
    {
        float dist = Mathf.Sqrt(x * x + y * y);
        return Mathf.Clamp01(1f - dist / radius);
    }

    private float DonutMask(float x, float y)
    {
        float dist = Mathf.Sqrt(x * x + y * y);
        float outer = Mathf.Clamp01(1f - dist / radius);
        float inner = Mathf.Clamp01(1f - dist / innerRadius);
        return Mathf.Clamp01(outer - inner);
    }

    private float CrescentMask(float x, float y)
    {
        float main = CircleMask(x, y);
        float shifted = CircleMask(x - bendAmount, y);
        return Mathf.Clamp01(main - shifted);
    }

    private float ArchipelagoMask(float x, float y)
    {
        float mask = 0f;
        for (int i = 0; i < islandCount; i++)
        {
            float angle = i * Mathf.PI * 2f / islandCount;
            float offsetX = Mathf.Cos(angle) * 0.5f;
            float offsetY = Mathf.Sin(angle) * 0.5f;

            float dx = x - offsetX;
            float dy = y - offsetY;

            float island = CircleMask(dx, dy);
            mask = Mathf.Max(mask, island);
        }
        return mask;
    }

    private float NoiseWarpedMask(float x, float y)
    {
        float nx = x + Mathf.PerlinNoise(x * warpFrequency, y * warpFrequency) * warpStrength;
        float ny = y + Mathf.PerlinNoise(y * warpFrequency, x * warpFrequency) * warpStrength;
        return CircleMask(nx, ny);
    }
}
