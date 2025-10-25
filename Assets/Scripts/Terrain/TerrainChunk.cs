
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    // World variables
    public int chunkHeight = 64;
    public int chunkWidth = 64;
    [SerializeField] int size = 10; // número de vértices por lado

    // Noise variables
    public float noiseScale = 0.8f;
    public float noiseAmplitude = 10f;
    public float freq = 0.05f;
    public int seed = 42;

    void Start()
    {
        // Fast Noise
        FastNoise noise = new FastNoise();

        List<Vector3> vertices =  new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // World
        BlockType[,,] world = new BlockType[chunkWidth + 2, chunkHeight, chunkWidth + 2];

        // Populate world array
        for(int x = 1; x < chunkWidth + 1; x++)
            for(int z = 1; z < chunkWidth + 1; z++)
            {
                for(int y = 0; y < chunkHeight; y++)
                {
                    // float simplex1 = noise.GetSimplex(x * noiseScale, z * noiseScale) * noiseAmplitude;
                    // float simplex2 = noise.GetSimplex(x * 3f, z * 3f) * 10 * (noise.GetSimplex(x*.3f, z*.3f)+.5f);
                    // float noiseValue = simplex1 + simplex2 + 5;

                    // Normalized noise
                    float n = (noise.GetSimplex(x * noiseScale, z * noiseScale) + 1f) * 0.5f; // de [-1,1] → [0,1]
                    float noiseValue = n * (chunkHeight - 1);
                    int surfaceY = Mathf.FloorToInt(noiseValue);
                    
                    if (y <= surfaceY - 4)
                        world[x, y, z] = BlockType.Stone;
                    else if(y < surfaceY)
                        world[x, y, z] = BlockType.Dirt;
                    else if(y == surfaceY) // this is because y == noiseValue is never true due to types. When noise == 22.8 and y 22 it fails
                        world[x, y, z] = BlockType.Grass;
                    else
                        world[x, y, z] = BlockType.Air;
                }
            }

        // BuildMesh: Generate Vertices and Triangles
        for(int x = 1; x < chunkWidth + 1; x++)
        for(int z = 1; z < chunkWidth + 1; z++)
        for(int y = 0; y < chunkHeight; y++)
        {   
            BlockType currentType = world[x, y, z];
            if(currentType != BlockType.Air)
            {
                // Upper face (normal facing +y)
                if(y == chunkHeight - 1 || world[x, y + 1, z] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    // Add ver
                    vertices.Add(new Vector3(x, y + 1, z)); // 010 v0
                    vertices.Add(new Vector3(x, y + 1, z + 1)); // 011 v1
                    vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // 111 v2
                    vertices.Add(new Vector3(x + 1, y + 1, z)); // 110 v3

                    // Tris
                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v1
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v3

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.topUV.GetUV();
                    uvs.AddRange(faceUV);
                }
                // Bottom Face
                if(y == 0 || world[x, y - 1, z] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    // Add ver
                    vertices.Add(new Vector3(x, y, z)); // 000 v0
                    vertices.Add(new Vector3(x + 1, y, z)); // 100 v3
                    vertices.Add(new Vector3(x + 1, y, z + 1)); // 101 v2
                    vertices.Add(new Vector3(x, y, z + 1)); // 001 v1

                    // Tris
                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v1
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v3

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.bottomUV.GetUV();
                    uvs.AddRange(faceUV);
                }

                // Right Face (normal facing +x)
                if (world[x + 1, y, z] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    vertices.Add(new Vector3(x + 1, y, z)); // 100
                    vertices.Add(new Vector3(x + 1, y + 1, z)); // 110
                    vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // 111
                    vertices.Add(new Vector3(x + 1, y, z + 1)); // 101

                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v3 
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v1

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.sideUV.GetUV();
                    uvs.AddRange(faceUV);
                }
                // Left Face (normal facing -x)
                if (world[x - 1, y, z] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    vertices.Add(new Vector3(x, y, z + 1)); // 001   v0
                    vertices.Add(new Vector3(x, y + 1, z + 1)); // 011   v1
                    vertices.Add(new Vector3(x, y + 1, z)); // 010  v2
                    vertices.Add(new Vector3(x, y, z)); // 000  v3

                    // First triangle v0 v3 v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v1
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v3 

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.sideUV.GetUV();
                    uvs.AddRange(faceUV);
                }

                // Front face z-
                if (world[x, y, z - 1] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    vertices.Add(new Vector3(x, y, z)); // 000 v0
                    vertices.Add(new Vector3(x, y + 1, z)); // 010 v0
                    vertices.Add(new Vector3(x + 1, y + 1, z)); // 110 v0
                    vertices.Add(new Vector3(x + 1, y, z)); // 100 v0
                    // Tris
                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v1
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v3 

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.sideUV.GetUV();
                    uvs.AddRange(faceUV);

                }
                // Back face z+
                if(world[x, y, z + 1] == BlockType.Air)
                {
                    int i = vertices.Count; 

                    vertices.Add(new Vector3(x, y, z + 1)); // 001
                    vertices.Add(new Vector3(x + 1, y, z + 1)); // 101
                    vertices.Add(new Vector3(x + 1, y + 1, z + 1)); // 111
                    vertices.Add(new Vector3(x, y + 1, z + 1)); // 011
                    // Tris
                    triangles.Add(i); // v0
                    triangles.Add(i + 1); // v1
                    triangles.Add(i + 2); // v2
                    triangles.Add(i); // v0
                    triangles.Add(i + 2); // v2
                    triangles.Add(i + 3); // v3 

                    // UVs
                    Block currentBlock = Block.blockData[currentType];
                    Vector2[] faceUV = currentBlock.sideUV.GetUV();
                    uvs.AddRange(faceUV);
                }

            }

        }

        // Draw Mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}