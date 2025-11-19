    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;


    public class ChunkData
    {   
        WorldGenerator world;

        // Mesh variables
        List<Vector3> vertices =  new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        FastNoise noise = new FastNoise();
        
        public Dictionary<Vector3Int, BlockType> blockData = new();
        public Dictionary<Direction, Vector2Int> neighbours = new();

        public int chunkWidth = TerrainChunk.chunkWidth;
        public int chunkHeight = TerrainChunk.chunkHeight;
        public Vector2Int chunkCoord;

        public Dictionary<Direction, BlockType[,,]> neighboursBlocks;

        // Chunkdata Array
        public BlockType[,,] blocks = new BlockType[TerrainChunk.chunkWidth, TerrainChunk.chunkHeight, TerrainChunk.chunkWidth];

        public static Dictionary<FaceDirection, Vector3[]> FaceVertexMap = new Dictionary<FaceDirection, Vector3[]>()
        {
        {FaceDirection.Top, new Vector3[]{new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0)}},
        {FaceDirection.Bottom, new Vector3[]{new Vector3(0,0,1), new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1)}},
        {FaceDirection.Right, new Vector3[]{new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1)}},
        {FaceDirection.Left, new Vector3[]{new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0)}},
        {FaceDirection.Front, new Vector3[]{new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1)}},
        {FaceDirection.Back,  new Vector3[]{new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0)}}
        };

        // Constructor
        public ChunkData(
            Vector2Int chunkCoord,
            BlockType [,,] blocks,
            WorldGenerator world
            )
        {
            this.chunkCoord = chunkCoord;
            this.blocks = blocks;
            this.world = world;
            // this.neighbours = neighbours;
        }

        public void PopulateChunkData()
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

                float amp = world.amplitude;
                float freq = world.frequency;

                float noiseHeight = 0;

                for(int i = 0; i < world.octaves; i++)
                {

                    float noiseValue = (noise.GetSimplex(xGlobal / world.scale * freq, zGlobal / world.scale * freq));
                    noiseHeight += noiseValue * amp;

                    freq *= world.lacunarity;
                    amp *= world.persistance;
                }

                float normalizedHeight = Mathf.InverseLerp(world.globalMinNoise, world.globalMaxNoise, noiseHeight);
                // float normalizedHeight = (noiseHeight + 1) / 2f;
                normalizedHeight = world.meshHeightCurve.Evaluate(normalizedHeight);

                float groundHeight = Mathf.Lerp(40, 115, normalizedHeight);
                groundHeight = Mathf.FloorToInt(groundHeight);
                // float groundHeight = Mathf.FloorToInt(TerrainChunk.chunkHeight * 1/2 + normalizedHeight);

                for(int y = 0; y < chunkHeight; y++)
                {   
                    int caveLevel = Mathf.FloorToInt(groundHeight) - 5;
                    float caveTreshold = 0.28f;

                    if(y == 0)
                    {
                        blocks[x, y, z] = BlockType.Stone;
                        continue;
                    }   
                    
                    // Assign block types depending on distance to the noise groundHeight
                    if(y <= caveLevel)
                    {
                        float caveNoise = (noise.GetPerlinFractal(xGlobal * world.caveScaleW, y * world.caveScaleH, zGlobal * world.caveScaleW ));

                        if (caveNoise > caveTreshold)
                            blocks[x, y, z] = BlockType.Air;
                        else
                            blocks[x, y, z] = BlockType.Stone;   
                    }
                    else if (y <= groundHeight - 5)
                        blocks[x, y, z] = BlockType.Stone;
                    else if(y < groundHeight)
                        blocks[x, y, z] = BlockType.Dirt;
                    else if(y == groundHeight)
                        blocks[x, y, z] = BlockType.Grass;
                    else
                        blocks[x, y, z] = BlockType.Air;
                }
            }
            // return blocks;
            // world.WorldData[chunkCoord] = blocks;
        }

        public MeshData GenerateMeshData()
        {   
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();

            // Create Vertices, UVs & Triangles
            for(int x = 0; x < chunkWidth; x++)
            for(int z = 0; z < chunkWidth; z++)
            for(int y = 0; y < chunkHeight; y++)
            {   
                BlockType currentType = blocks[x, y, z];
                Vector3Int currentPos = new Vector3Int(x, y, z);

                if(currentType != BlockType.Air)
                {
                    // Top face y+
                    if(y == chunkHeight - 1 || blocks[x, y + 1, z] == BlockType.Air)
                        AddFace(FaceDirection.Top, currentPos, chunkCoord, currentType);

                    // Bottom Face y-
                    if(y > 0 && blocks[x, y - 1, z] == BlockType.Air)
                        AddFace(FaceDirection.Bottom, currentPos, chunkCoord, currentType);

                    // Right Face x+
                    if(x < chunkWidth - 1)
                    {
                        if(blocks[x + 1, y, z] == BlockType.Air)
                            AddFace(FaceDirection.Right, currentPos, chunkCoord, currentType);
                    }
                    // else
                    // {
                    //     if(checkNeighbourAir(neighbours[Direction.East], new Vector3Int(0, y, z)))
                    //         AddFace(FaceDirection.Right, currentPos, chunkCoord, currentType);
                    // }

                    // Left Face x-
                    if(x > 0)
                    {
                        if (blocks[x - 1, y, z] == BlockType.Air)
                            AddFace(FaceDirection.Left, currentPos, chunkCoord, currentType);
                    }
                    // else
                    // {
                    // if(checkNeighbourAir(neighbours[Direction.West], new Vector3Int(chunkWidth - 1, y, z)))
                    //         AddFace(FaceDirection.Left, currentPos, chunkCoord, currentType);
                    // }

                    // Front Face z+
                    if(z < chunkWidth - 1)
                    {
                        if(blocks[x, y, z + 1] == BlockType.Air)
                            AddFace(FaceDirection.Front, currentPos, chunkCoord, currentType);
                    }
                    // else
                    // {
                    // if(checkNeighbourAir(neighbours[Direction.North], new Vector3Int(x, y, 0)))
                    //         AddFace(FaceDirection.Front, currentPos, chunkCoord, currentType);
                    // }

                    // Back Face z-
                    if(z > 0)
                    {
                        if (blocks[x, y, z - 1] == BlockType.Air)
                            AddFace(FaceDirection.Back, currentPos, chunkCoord, currentType);
                    }
                    // else
                    // {
                    // if(checkNeighbourAir(neighbours[Direction.South], new Vector3Int(x, y, chunkWidth - 1)))
                    //         AddFace(FaceDirection.Back, currentPos, chunkCoord, currentType);
                    // }
                }
            }

            return new MeshData(triangles, uvs, vertices);
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

        // public bool checkNeighbourAir(Vector2Int chunkCoords, Vector3Int currentPos)
        // {
        //     int x = currentPos.x;
        //     int y = currentPos.y;
        //     int z = currentPos.z;

        //     if(world.activeChunks.TryGetValue(chunkCoords, out TerrainChunk neighbour))
        //     if(neighbour.blocks[x, y, z] == BlockType.Air)
        //         return true;

        //     return false;
        // }   
    }