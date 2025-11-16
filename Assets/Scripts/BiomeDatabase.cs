using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeDatabase", menuName = "WorldGen/Biome Database")]
public class BiomeDatabase : ScriptableObject
{
    public List<BiomeDefinition> biomes;
}
