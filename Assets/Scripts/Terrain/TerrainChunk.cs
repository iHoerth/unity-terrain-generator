
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{

    
    // World variables
    public const int chunkHeight = 64;
    public const int chunkWidth = 64;

    public Vector2Int chunkCoord;
    public WorldGenerator world;

    // Noise variables
    public float noiseScale = 0.8f;
    public float noiseAmplitude = 10f;
    public float freq = 0.05f;
    public int seed = 42;
    FastNoise noise = new FastNoise();

    List<Vector3> vertices =  new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    public BlockType[,,] chunkBlocks = new BlockType[chunkWidth + 2, chunkHeight, chunkWidth + 2];

    public enum FaceDirection {Top, Bottom, Right, Left, Front, Back};

    public static Dictionary<FaceDirection, Vector3[]> FaceVertexMap = new Dictionary<FaceDirection, Vector3[]>()
    {
       {FaceDirection.Top, new Vector3[]{new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0)}},
        //010 011 111 110
       {FaceDirection.Bottom, new Vector3[]{new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1)}},
        //001 000 100 101
       {FaceDirection.Right, new Vector3[]{new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1)}},
        //100 110 111 101
       {FaceDirection.Left, new Vector3[]{new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0)}},
        //001 011 010 000
       {FaceDirection.Front, new Vector3[]{new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1)}},
        //101 111 011 001
       {FaceDirection.Back,  new Vector3[]{new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0)}}
    };

    public void Init(Vector2Int chunkCoord, WorldGenerator world)
    {
        this.chunkCoord = chunkCoord;
        this.world = world;
    }

    public void populateChunk()
    {   
        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth;

        for(int x = 1; x < chunkWidth + 1; x++)
        for(int z = 1; z < chunkWidth + 1; z++)
        for(int y = 0; y < chunkHeight; y++)
        {
            int xGlobal = x + xOffset;
            int zGlobal = z + zOffset;
            // float simplex1 = noise.GetSimplex(x * noiseScale, z * noiseScale) * noiseAmplitude;
            // float simplex2 = noise.GetSimplex(x * 3f, z * 3f) * 10 * (noise.GetSimplex(x*.3f, z*.3f)+.5f);
            // float noiseValue = simplex1 + simplex2 + 5;

            // Normalized noise
            float n = (noise.GetSimplex(xGlobal * noiseScale, zGlobal * noiseScale) + 1f) * 0.5f; // de [-1,1] → [0,1]
            float noiseValue = n * (chunkHeight - 1);
            int surfaceY = Mathf.FloorToInt(noiseValue);
            
            if (y <= surfaceY - 4)
                chunkBlocks[x, y, z] = BlockType.Stone;
            else if(y < surfaceY)
                chunkBlocks[x, y, z] = BlockType.Dirt;
            else if(y == surfaceY) // this is because y == noiseValue is never true due to int vs float. When noise == 22.8 and y 22 it fails
                chunkBlocks[x, y, z] = BlockType.Grass;
            else
                chunkBlocks[x, y, z] = BlockType.Air;
        }
    }

    public bool checkNeighbourAir(Vector2Int coords, int x, int y , int z)
    {
        if(world.chunks.TryGetValue(coords, out TerrainChunk neighbour))
        if(neighbour.chunkBlocks[x,y,z] == BlockType.Air)
            return true;

        return false;
    }   

    public void AddFace(FaceDirection face, Vector3Int basePos, Vector2Int chunkCoord, BlockType currentBlockType)
    {
        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth; 

        int currentIndex = vertices.Count;

        foreach (Vector3 vertexPosition in FaceVertexMap[face])
            vertices.Add(basePos + vertexPosition + new Vector3(xOffset, 0, zOffset));

        // Tris clockwise
        triangles.Add(currentIndex); // v0
        triangles.Add(currentIndex + 1); // v1
        triangles.Add(currentIndex + 2); // v2
        triangles.Add(currentIndex); // v0
        triangles.Add(currentIndex + 2); // v2
        triangles.Add(currentIndex + 3); // v3

        // UVs
        Block currentBlock = Block.blockData[currentBlockType];
        Vector2[] faceUV = currentBlock.topUV.GetUVs();
        uvs.AddRange(faceUV);
    }

    public void buildMesh()
    {
        Mesh mesh = new Mesh();

        // Neighbour Chunks coords
        Vector2Int eastNeighbourCoords = new Vector2Int(chunkCoord.x + 1, chunkCoord.y);
        Vector2Int westNeighbourCoords = new Vector2Int(chunkCoord.x - 1, chunkCoord.y);
        Vector2Int northNeighbourCoords= new Vector2Int(chunkCoord.x, chunkCoord.y + 1);
        Vector2Int southNeighbourCoords= new Vector2Int(chunkCoord.x, chunkCoord.y - 1);

        // Create Vertices, UVs & Triangles
        for(int x = 1; x < chunkWidth + 1; x++)
        for(int z = 1; z < chunkWidth + 1; z++)
        for(int y = 0; y < chunkHeight; y++)
        {   

            BlockType currentType = chunkBlocks[x, y, z];
            if(currentType != BlockType.Air)
            {
                // Upper face (normal facing +y)
                if(y == chunkHeight - 1 || chunkBlocks[x, y + 1, z] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Top;
                    AddFace(face, pos, chunkCoord, currentType);
                }

                // Bottom Face
                if(y > 0 && chunkBlocks[x, y - 1, z] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Bottom;
                    AddFace(face, pos, chunkCoord, currentType);
                }

                // Right Face x +
                // if(x < chunkWidth || checkNeighbourAir(eastNeighbourCoords,0,y,z))
                if(chunkBlocks[x + 1, y, z] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Right;
                    AddFace(face, pos, chunkCoord, currentType);
                }
                // Left Face (normal facing -x)
                if (chunkBlocks[x - 1, y, z] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Left;
                    AddFace(face, pos, chunkCoord, currentType);
                }

                // Back face z-
                if (chunkBlocks[x, y, z - 1] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Back;
                    AddFace(face, pos, chunkCoord, currentType);
                }

                // Front face z+
                if(chunkBlocks[x, y, z + 1] == BlockType.Air)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    FaceDirection face = FaceDirection.Front;
                    AddFace(face, pos, chunkCoord, currentType);
                }
            }
        }

        // Draw Mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
    }
}