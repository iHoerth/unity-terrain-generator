using UnityEngine;
using System.Collections.Generic;

public class ChunkCalculator
{
    public Vector2Int chunkCoord;
    public BlockType[,,] chunkBlocks;
    public int chunkWidth;
    public int chunkHeight;
    public Dictionary<TerrainChunk.Direction, BlockType[,,]> neighboursBlocks;
    public bool meshReady = false;

    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();

    public enum FaceDirection {Top, Bottom, Right, Left, Front, Back};

    [Header("Noise Parameters")]
    public float noiseScale = 0.8f;
    public float noiseAmplitude = 10f;
    public float freq = 0.05f;
    public int seed = 42;
    public FastNoise noise = new FastNoise();

    public static Dictionary<FaceDirection, Vector3[]> FaceVertexMap = new Dictionary<FaceDirection, Vector3[]>()
    {
       {FaceDirection.Top, new Vector3[]{new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0)}},
       {FaceDirection.Bottom, new Vector3[]{new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1)}},
       {FaceDirection.Right, new Vector3[]{new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1)}},
       {FaceDirection.Left, new Vector3[]{new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0)}},
       {FaceDirection.Front, new Vector3[]{new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1)}},
       {FaceDirection.Back,  new Vector3[]{new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0)}}
    };
    
    public ChunkCalculator(
        Vector2Int chunkCoord,
        BlockType [,,] chunkBlocks,
        Dictionary<TerrainChunk.Direction, BlockType[,,]> neighboursBlocks,
        int chunkWidth,
        int chunkHeight)
    {
        this.chunkCoord = chunkCoord;
        this.chunkBlocks = chunkBlocks;
        this.neighboursBlocks = neighboursBlocks;
        this.chunkWidth = chunkWidth;
        this.chunkHeight = chunkHeight;
    }

    // Populates the chunkBlock 3D array with blocktypes
    public void PopulateChunk()
    {   
        // Calculate the chunk offset 
        int xOffset = chunkCoord.x * chunkWidth;
        int zOffset = chunkCoord.y * chunkWidth;

        for(int x = 0; x < chunkWidth; x++)
        for(int z = 0; z < chunkWidth; z++)
        for(int y = 0; y < chunkHeight; y++)
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

    public void CalculateMeshData()
    {
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
                    if(CheckNeighbourAir(TerrainChunk.Direction.East, border))
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
                    
                    if(CheckNeighbourAir(TerrainChunk.Direction.West, border))
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

                    if(CheckNeighbourAir(TerrainChunk.Direction.North, border))
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
                    
                    if(CheckNeighbourAir(TerrainChunk.Direction.South, border))
                    {
                        FaceDirection face = FaceDirection.Back;
                        AddFace(face, currentPos, chunkCoord, currentType);
                    }
                }
            }
        }
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

    public bool CheckNeighbourAir(TerrainChunk.Direction dir, Vector3Int pos)
    {
        if(this.neighboursBlocks.TryGetValue(dir, out BlockType[,,] neighBlocks))
        {
            if(neighBlocks[pos.x, pos.y, pos.z] == BlockType.Air)
                return true;
        }

        return false;
    }       
}