
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Debug
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    // World variables
    public const int chunkHeight = 128;
    public const int chunkWidth = 16;
    public Vector2Int chunkCoord;
    public WorldGenerator world;
    public Dictionary<Direction, Vector2Int> neighbours = new();

    // Noise variables
    public float noiseScale = 0.8f;
    public float noiseAmplitude = 10f;
    public float freq = 0.05f;
    public int seed = 42;
    FastNoise noise = new FastNoise();

    // Mesh variables
    List<Vector3> vertices =  new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    // Chunkdata Array
    public BlockType[,,] chunkBlocks = new BlockType[chunkWidth, chunkHeight, chunkWidth];

    public enum FaceDirection {Top, Bottom, Right, Left, Front, Back};
    public enum Direction {North, West, East, South};

    public static Dictionary<FaceDirection, Vector3[]> FaceVertexMap = new Dictionary<FaceDirection, Vector3[]>()
    {
       {FaceDirection.Top, new Vector3[]{new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0)}},
       {FaceDirection.Bottom, new Vector3[]{new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1)}},
       {FaceDirection.Right, new Vector3[]{new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1)}},
       {FaceDirection.Left, new Vector3[]{new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0)}},
       {FaceDirection.Front, new Vector3[]{new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1)}},
       {FaceDirection.Back,  new Vector3[]{new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0)}}
    };

    public void Start()
    {
        // Iterates neighbour dictionary
        foreach (Direction dir in neighbours.Keys)
        {
            Vector2Int neighbourCoord = neighbours[dir];
            // Tries to get neighbour chunk reference
            if (world.chunks.TryGetValue(neighbourCoord, out TerrainChunk neighbour))
                // Forces neighbour to "rebuild" mesh to clean duplicate faces. Will replace in the future with "rebuildBorder()"
                neighbour.buildMesh();
        }
    }
    public void ClearData()
    {
        neighbours.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        chunkBlocks = new BlockType[chunkWidth, chunkHeight, chunkWidth];
    }
    // Initializes the chunks & neighbours global position variables
    public void Init(Vector2Int chunkCoord, WorldGenerator world)
    {
        this.chunkCoord = chunkCoord;
        this.world = world;
        neighbours.Clear();

        neighbours.Add(Direction.East, new Vector2Int(chunkCoord.x + 1, chunkCoord.y));
        neighbours.Add(Direction.West, new Vector2Int(chunkCoord.x - 1, chunkCoord.y));
        neighbours.Add(Direction.North, new Vector2Int(chunkCoord.x, chunkCoord.y + 1));
        neighbours.Add(Direction.South, new Vector2Int(chunkCoord.x, chunkCoord.y - 1));
     }

    // Populates the chunkBlock 3D array with blocktypes
    public void populateChunk()
    {   
        // Calculate the chunk offset 
        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth;

        for(int x = 0; x < chunkWidth; x++)
        for(int z = 0; z < chunkWidth; z++)
        {
            // Calculate global coords of the chunk
            int xGlobal = x + xOffset;
            int zGlobal = z + zOffset;

            // Normalized noise
            float n = (noise.GetSimplex(xGlobal * noiseScale, zGlobal * noiseScale) + 1f) * 0.5f; // de [-1,1] → [0,1]
            float noiseValue = n * (chunkHeight/4 - 1);
            
            int surfaceY = Mathf.FloorToInt(noiseValue);

            // Old noise
            // float simplex1 = noise.GetSimplex(x * noiseScale, z * noiseScale) * noiseAmplitude;
            // float simplex2 = noise.GetSimplex(x * 3f, z * 3f) * 10 * (noise.GetSimplex(x*.3f, z*.3f)+.5f);
            // float noiseValue = simplex1 + simplex2 + 5;

            for(int y = 0; y < chunkHeight; y++)
            {
                // Assign block types depending on distance to the noise surface
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
    }

    // Checks if neighbour chunk has air in the border at that position
    public bool checkNeighbourAir(Vector2Int chunkCoords, Vector3Int currentPos)
    {
        int x = currentPos.x;
        int y = currentPos.y;
        int z = currentPos.z;

        if(world.chunks.TryGetValue(chunkCoords, out TerrainChunk neighbour))
        if(neighbour.chunkBlocks[x, y, z] == BlockType.Air)
            return true;

        return false;
    }   
    // Checks if the given global position is the chunk local limit x,z = 0 || x,z = 15.
    public bool isChunkLimit(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x == 0 || localPos.x == chunkWidth -1 || localPos.z == 0 || localPos.z == chunkWidth - 1)
        {
            return true;
        }
        return false;
    }

    // Checks if the given global pos is outside the chunk local coords.
    public bool InsideChunk(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x < 0 || localPos.x > chunkWidth -1)     
        {
            // Debug.Log("localPos.x Outside:" + localPos.x);
            return false;
        }   

        if(localPos.z < 0 || localPos.z > chunkWidth -1)     
        {
            // Debug.Log("localPos.z Outside:" + localPos.z);
            return false;
        } 

        return true;
    }

    public void AddFace(FaceDirection face, Vector3Int basePos, Vector2Int chunkCoord, BlockType currentBlockType)
    {   
        // Gets world offset to obtain global coords
        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth; 

        // Get current index count in the veritces lsit
        int currentIndex = vertices.Count;

        // Sum current vertex pos in chunk with face offset to obtain required vertices to draw current face
        foreach (Vector3 vertexPosition in FaceVertexMap[face])
            vertices.Add(basePos + vertexPosition + new Vector3(xOffset, 0, zOffset));

        // Add indices - clockwise
        triangles.Add(currentIndex); // v0
        triangles.Add(currentIndex + 1); // v1
        triangles.Add(currentIndex + 2); // v2
        triangles.Add(currentIndex); // v0
        triangles.Add(currentIndex + 2); // v2
        triangles.Add(currentIndex + 3); // v3

        // UVs
        Block currentBlock = Block.blockData[currentBlockType];
        Vector2[] faceUV;
        
        switch(face)
        {
            case FaceDirection.Top:
                faceUV = currentBlock.topUV.GetUVs();
                break;
            case FaceDirection.Bottom:
                faceUV = currentBlock.bottomUV.GetUVs();
                break;
            default:
                faceUV = currentBlock.sideUV.GetUVs();
                break;
        } 

        uvs.AddRange(faceUV);
    }
    
    public Vector3Int GlobalToLocal(Vector3Int globalPos)
    {
        Vector3Int localPos = new Vector3Int();

        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth; 

        localPos.x = globalPos.x - xOffset;
        localPos.y = globalPos.y;
        localPos.z = globalPos.z - zOffset;

        return localPos;
    } 

    public void ConvertBlock(Vector3Int globalPos, BlockType newType)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.y > 0)
        {
            chunkBlocks[localPos.x, localPos.y , localPos.z] = newType;
        }
    }

    public void buildMesh()
    {   
        
        Mesh mesh = new Mesh();

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        // Create Vertices, UVs & Triangles
        for(int x = 0; x < chunkWidth; x++)
        for(int z = 0; z < chunkWidth; z++)
        for(int y = 0; y < chunkHeight; y++)
        {   
            BlockType currentType = chunkBlocks[x, y, z];
            Vector3Int currentPos = new Vector3Int(x, y, z);

            if(currentType != BlockType.Air)
            {
                // Top face y+
                if(y == chunkHeight - 1 || chunkBlocks[x, y + 1, z] == BlockType.Air)
                {
                    FaceDirection face = FaceDirection.Top;
                    AddFace(face, currentPos, chunkCoord, currentType);
                }

                // Bottom Face y-
                if(y > 0 && chunkBlocks[x, y - 1, z] == BlockType.Air)
                {
                    FaceDirection face = FaceDirection.Bottom;
                    AddFace(face, currentPos, chunkCoord, currentType);
                }

                // Right Face x+
                // Checks if current x is in the border or somewhere in the middle
                if(x < chunkWidth - 1)
                {
                    if(chunkBlocks[x + 1, y, z] == BlockType.Air)
                    {
                        FaceDirection face = FaceDirection.Right;
                        AddFace(face, currentPos, chunkCoord, currentType);
                    }
                } else {
                // Current X in the limit, so need to check for neighbour blocktype
                   Vector3Int border =  new Vector3Int(0, y, z);
                   if(checkNeighbourAir(neighbours[Direction.East], border))
                   {
                        FaceDirection face = FaceDirection.Right;
                        AddFace(face, currentPos, chunkCoord, currentType);
                   }
                }

                // Left Face x-
                if(x > 0)
                {
                    if (chunkBlocks[x - 1, y, z] == BlockType.Air)
                    {
                        FaceDirection face = FaceDirection.Left;
                        AddFace(face, currentPos, chunkCoord, currentType);
                    }
                } else {
                   Vector3Int border =  new Vector3Int(chunkWidth - 1, y, z);
                   
                   if(checkNeighbourAir(neighbours[Direction.West], border))
                   {
                        FaceDirection face = FaceDirection.Left;
                        AddFace(face, currentPos, chunkCoord, currentType);
                   }
                }

                // Front Face z+
                if(z < chunkWidth - 1)
                {
                    if(chunkBlocks[x, y, z + 1] == BlockType.Air)
                    {
                        FaceDirection face = FaceDirection.Front;
                        AddFace(face, currentPos, chunkCoord, currentType);
                    }
                } else {
                   Vector3Int border =  new Vector3Int(x, y, 0);

                   if(checkNeighbourAir(neighbours[Direction.North], border))
                   {
                        FaceDirection face = FaceDirection.Front;
                        AddFace(face, currentPos, chunkCoord, currentType);
                   }
                }

                // Back Face z-
                if(z > 0)
                {
                    if (chunkBlocks[x, y, z - 1] == BlockType.Air)
                    {
                        FaceDirection face = FaceDirection.Back;
                        AddFace(face, currentPos, chunkCoord, currentType);
                    }
                } else {
                   Vector3Int border =  new Vector3Int(x, y, chunkWidth - 1);
                   
                   if(checkNeighbourAir(neighbours[Direction.South], border))
                   {
                        FaceDirection face = FaceDirection.Back;
                        AddFace(face, currentPos, chunkCoord, currentType);
                   }
                }
            }
        }

        // Draw Mesh
        mesh.Clear();
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