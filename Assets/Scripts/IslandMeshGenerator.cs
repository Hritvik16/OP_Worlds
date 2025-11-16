using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class IslandMeshGenerator : MonoBehaviour
{
    [Header("References")]
    public IslandHeightmapGenerator heightmapGenerator;

    [Header("Mesh Settings")]
    public float islandSize = 512f;
    public float maxHeight = 100f;
    public bool autoGenerate = true;

    private MeshFilter meshFilter;
    private Mesh islandMesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (islandMesh == null)
        {
            islandMesh = new Mesh();
            islandMesh.name = "IslandMesh";
        }

        meshFilter.sharedMesh = islandMesh;
    }

    private void OnValidate()
    {
        // Ensure MeshFilter exists
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        // Ensure mesh exists in editor
        if (islandMesh == null)
        {
            islandMesh = new Mesh();
            islandMesh.name = "IslandMesh";
            meshFilter.sharedMesh = islandMesh;
        }

        if (autoGenerate && heightmapGenerator != null)
        {
            GenerateIsland();
        }
    }

    public void GenerateIsland()
    {
        float[,] heightmap = heightmapGenerator.GenerateHeightmap();
        BuildMeshFromHeightmap(heightmap);
    }

    private void BuildMeshFromHeightmap(float[,] heightmap)
    {
        int vertsX = heightmap.GetLength(0);
        int vertsY = heightmap.GetLength(1);

        int vertexCount = vertsX * vertsY;
        int quadX = vertsX - 1;
        int quadY = vertsY - 1;
        int triCount = quadX * quadY * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triCount * 3];

        float stepX = islandSize / (vertsX - 1);
        float stepY = islandSize / (vertsY - 1);

        float offsetX = -islandSize * 0.5f;
        float offsetZ = -islandSize * 0.5f;

        int v = 0;
        for (int y = 0; y < vertsY; y++)
        {
            for (int x = 0; x < vertsX; x++)
            {
                float height01 = Mathf.Clamp01(heightmap[x, y]);
                vertices[v] = new Vector3(offsetX + x * stepX, height01 * maxHeight, offsetZ + y * stepY);
                uvs[v] = new Vector2((float)x / (vertsX - 1), (float)y / (vertsY - 1));
                normals[v] = Vector3.up;
                v++;
            }
        }

        int t = 0;
        for (int y = 0; y < quadY; y++)
        {
            for (int x = 0; x < quadX; x++)
            {
                int i0 = y * vertsX + x;
                int i1 = i0 + 1;
                int i2 = i0 + vertsX;
                int i3 = i2 + 1;

                triangles[t++] = i0;
                triangles[t++] = i2;
                triangles[t++] = i1;

                triangles[t++] = i1;
                triangles[t++] = i2;
                triangles[t++] = i3;
            }
        }

        islandMesh.Clear();
        islandMesh.vertices = vertices;
        islandMesh.triangles = triangles;
        islandMesh.uv = uvs;

        islandMesh.RecalculateNormals();
        islandMesh.RecalculateBounds();
        islandMesh.RecalculateTangents();

        meshFilter.sharedMesh = islandMesh;
    }
}
