using UnityEngine;

public struct HeightmapData
{
    public float[,] height;
    public float[,] beach;
    public float[,] cliff;
}

public class IslandHeightmapGenerator : MonoBehaviour
{
    [Header("Heightmap Settings")]
    public int resolution = 256;            // Size of the heightmap (resolution x resolution)
    public float noiseScale = 50f;          // Controls the zoom of the noise

    [Header("Noise Octaves")]
    public int octaves = 4;
    public float persistence = 0.5f;        // Amplitude multiplier per octave
    public float lacunarity = 2.0f;         // Frequency multiplier per octave

    [Header("Island Shape Generator")]
    public IslandShapeGenerator shape = new IslandShapeGenerator();

    [Header("Height Controls")]
    public float heightMultiplier = 1f;     // Overall height scale
    public float peakSharpness = 1.5f;      // Higher = pointier mountains
    public float baseElevation = 0f;        // Lifts or lowers entire island

    [Header("Shoreline Settings")]
    public float waterLevel = 0.25f;           // Where the ocean surface is
    public float beachWidth = 0.05f;           // Width of beach transition zone (0–1)
    public float cliffSlopeThreshold = 0.75f;  // Slope needed to trigger cliff mask

    [Header("Debug")]
    public bool autoGenerate = true;
    public Texture2D debugPreview;             // Shows heightmap as a texture


    // UPDATED: returns multiple maps (height, beach, cliff)
    public HeightmapData GenerateHeightmap(int seed = 0)
    {
        float[,] heightMap = new float[resolution, resolution];
        float[,] beachMap = new float[resolution, resolution];
        float[,] cliffMap = new float[resolution, resolution];

        // Seed randomness for different islands
        Random.InitState(seed);

        float offsetX = Random.Range(-10000f, 10000f);
        float offsetY = Random.Range(-10000f, 10000f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // -------------------------
                // 1. Base Layered Noise
                // -------------------------
                float noiseValue = 0f;
                float amplitude = 1f;
                float frequency = 1f;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x + offsetX) / noiseScale * frequency;
                    float sampleY = (y + offsetY) / noiseScale * frequency;

                    float perlin = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    noiseValue += perlin * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseValue = Mathf.InverseLerp(-1f, 1f, noiseValue);   // normalize to 0–1


                // -------------------------
                // 2. Island Shape Mask
                // -------------------------
                float nx = (float)x / (resolution - 1) * 2f - 1f;  // normalize to -1..1
                float ny = (float)y / (resolution - 1) * 2f - 1f;

                float shapeMask = Mathf.Clamp01(shape.GetMask(nx, ny));


                // -------------------------
                // 3. Peak Sharpness
                // -------------------------
                float shaped = Mathf.Pow(noiseValue, peakSharpness);


                // -------------------------
                // 4. Final Height
                // -------------------------
                float finalHeight = shaped * shapeMask * heightMultiplier + baseElevation;
                finalHeight = Mathf.Clamp01(finalHeight);

                heightMap[x, y] = finalHeight;


                // -------------------------
                // 5. Beach Mask
                // -------------------------
                // How close the height is to water level (0 = underwater)
                float waterDist = Mathf.Clamp01((finalHeight - waterLevel) / beachWidth);
                float beachMask = 1f - waterDist; // 1 near shore

                beachMap[x, y] = beachMask;


                // -------------------------
                // 6. Cliff Mask (Slope-Based)
                // -------------------------
                float heightL = (x > 0) ? heightMap[x - 1, y] : finalHeight;
                float heightR = (x < resolution - 1) ? heightMap[x + 1, y] : finalHeight;
                float heightD = (y > 0) ? heightMap[x, y - 1] : finalHeight;
                float heightU = (y < resolution - 1) ? heightMap[x, y + 1] : finalHeight;

                float dx = heightR - heightL;
                float dy = heightU - heightD;
                float slope = Mathf.Sqrt(dx * dx + dy * dy);

                float cliffMask = Mathf.InverseLerp(cliffSlopeThreshold, 1f, slope);
                cliffMap[x, y] = cliffMask;
            }
        }

        return new HeightmapData()
        {
            height = heightMap,
            beach = beachMap,
            cliff = cliffMap
        };
    }


    // -------------------------
    // Editor Preview Generator
    // -------------------------
    public void GeneratePreview()
    {
        HeightmapData data = GenerateHeightmap();

        debugPreview = new Texture2D(resolution, resolution);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float v = data.height[x, y];
                debugPreview.SetPixel(x, y, new Color(v, v, v));
            }
        }

        debugPreview.Apply();
    }


    // -------------------------
    // Auto-Regenerate in Editor
    // -------------------------
    private void OnValidate()
    {
        if (autoGenerate)
        {
            GeneratePreview();
            if (Application.isEditor && !Application.isPlaying)
            {
                var meshGen = GetComponent<IslandMeshGenerator>();
                if (meshGen != null && meshGen.autoGenerate)
                    meshGen.GenerateIsland();
            }
        }
    }
}
