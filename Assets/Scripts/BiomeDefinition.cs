using UnityEngine;

[System.Serializable]
public class BiomeDefinition
{
    public BiomeType type;

    [Header("Basic Settings")]
    public string biomeName;

    [Range(0f, 1f)]
    public float minHeight;
    [Range(0f, 1f)]
    public float maxHeight;

    [Range(0f, 1f)]
    public float minMoisture;
    [Range(0f, 1f)]
    public float maxMoisture;

    [Header("Color Gradient")]
    public Color lowColor;
    public Color midColor;
    public Color highColor;
}
