using UnityEngine;

public class IslandHeightmapGenerator : MonoBehaviour
{
    [Header("Heightmap Settings")]
    public int resolution = 256;            // Size of the heightmap (resolution x resolution)
    public float noiseScale = 50f;          // Controls the zoom of the noise

    [Header("Noise Octaves")]
    public int octaves = 4;
    public float persistence = 0.5f;        // Amplitude multiplier per octave
    public float lacunarity = 2.0f;         // Frequency multiplier per octave

    [Header("Island Shape Controls")]
    public float falloffStrength = 3f;      // Higher = smaller island, stronger edges
    public float falloffPower = 2f;         // Controls softness of the falloff curve

    [Header("Height Controls")]
    public float heightMultiplier = 1f;     // Overall height scale
    public float peakSharpness = 1.5f;      // Higher = pointier mountains
    public float baseElevation = 0f;        // Lifts or lowers entire island

    [Header("Debug")]
    public bool autoGenerate = true;
    public Texture2D debugPreview;          // Shows heightmap as a texture

    public float[,] GenerateHeightmap(int seed = 0)
    {
        float[,] map = new float[resolution, resolution];

        // Seed randomness for different islands
        Random.InitState(seed);

        float offsetX = Random.Range(-10000f, 10000f);
        float offsetY = Random.Range(-10000f, 10000f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // 1. Base layered noise
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

                noiseValue = Mathf.InverseLerp(-1f, 1f, noiseValue);   // normalize

                // 2. Island radial falloff
                float fx = (float)x / resolution * 2f - 1f;
                float fy = (float)y / resolution * 2f - 1f;

                float distance = Mathf.Sqrt(fx * fx + fy * fy);
                float falloff = Mathf.Pow(Mathf.Clamp01(distance), falloffPower);

                float islandMask = Mathf.Clamp01(1f - falloff * falloffStrength);

                // 3. Peak sharpness
                float shaped = Mathf.Pow(noiseValue, peakSharpness);

                // 4. Combine height components
                float finalHeight = shaped * islandMask * heightMultiplier + baseElevation;

                map[x, y] = Mathf.Clamp01(finalHeight);
            }
        }

        return map;
    }

    // Editor Preview Generator
    public void GeneratePreview()
    {
        float[,] map = GenerateHeightmap();

        debugPreview = new Texture2D(resolution, resolution);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float v = map[x, y];
                debugPreview.SetPixel(x, y, new Color(v, v, v));
            }
        }

        debugPreview.Apply();
    }

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
