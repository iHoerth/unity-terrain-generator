using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    [Header("World Parameters")]
    public const int height = 128;
    public const int width = 16;
    public Vector2Int coords;
    public WorldGenerator world;

    public BlockType[,,] blockData = new BlockType[width, height, width];
    public Dictionary<Direction, Vector2Int> neighbours = new();
    public Dictionary<Direction, BlockType[,,]> neighboursBlocks = new Dictionary<Direction, BlockType[,,]>();

    public bool meshReady = false;

    private List<Vector3> vertices =  new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>();

    // Chunkdata Array

    // public enum FaceDirection {Top, Bottom, Right, Left, Front, Back};
    public enum Direction {North, West, East, South};

    // Initializes the chunks & neighbours global position variables
    public void Init(Vector2Int coords, WorldGenerator world)
    {
        this.coords = coords;
        this.world = world;
        neighbours.Clear();

        neighbours.Add(Direction.East, new Vector2Int(coords.x + 1, coords.y));
        neighbours.Add(Direction.West, new Vector2Int(coords.x - 1, coords.y));
        neighbours.Add(Direction.North, new Vector2Int(coords.x, coords.y + 1));
        neighbours.Add(Direction.South, new Vector2Int(coords.x, coords.y - 1));


    }

    public void ClearData()
    {
        neighbours.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        blockData = new BlockType[width, height, width];
    }

    // public void CalculateNeighbours()
    // {
    //     foreach (Direction dir in neighbours.Keys)
    //     {
    //         Vector2Int neighbourCoord = neighbours[dir];
    //         // Tries to get neighbour chunk reference
    //         if (world.chunks.TryGetValue(neighbourCoord, out TerrainChunk neighbour))
    //             // Forces neighbour to "rebuild" mesh to clean duplicate faces. Will replace in the future with "rebuildBorder()"
    //             // neighbour.CalculateMeshData();
    //             neighbour.DrawMesh();
    //     }
    // }

    public void UpdateNeighbourBlocks()
    {
        neighboursBlocks.Clear();
        // foreach (var kv in neighbours)
        //     if (world.chunks.TryGetValue(kv.Value, out var neigh))
        //         neighboursBlocks[kv.Key] = neigh.blockData;
        neighboursBlocks.Add(Direction.East, this.world.chunks[neighbours[Direction.East]].blockData);
        neighboursBlocks.Add(Direction.West, this.world.chunks[neighbours[Direction.West]].blockData);
        neighboursBlocks.Add(Direction.North, this.world.chunks[neighbours[Direction.North]].blockData);
        neighboursBlocks.Add(Direction.South, this.world.chunks[neighbours[Direction.South]].blockData);
    }

    // Checks if the given global position is the chunk local limit x,z = 0 || x,z = 15.
    public bool IsChunkLimit(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x == 0 || localPos.x == width -1 || localPos.z == 0 || localPos.z == width - 1)
        {
            return true;
        }
        return false;
    }

    // Checks if the given global pos is outside the chunk local coords.
    public bool InsideChunk(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x < 0 || localPos.x > width -1)     
        {
            // Debug.Log("localPos.x Outside:" + localPos.x);
            return false;
        }   

        if(localPos.z < 0 || localPos.z > width -1)     
        {
            // Debug.Log("localPos.z Outside:" + localPos.z);
            return false;
        } 

        return true;
    }

    public Vector3Int GlobalToLocal(Vector3Int globalPos)
    {
        Vector3Int localPos = new Vector3Int();

        int xOffset = coords.x * width;
        int zOffset = coords.y * width; 

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
            blockData[localPos.x, localPos.y , localPos.z] = newType;
        }
    }

    public void DrawMesh(ChunkCalculator job)
    {
        // Logica de dibujar la malla
        Mesh mesh = new Mesh();

        mesh.Clear();
        mesh.vertices = job.vertices.ToArray();
        mesh.triangles = job.triangles.ToArray();
        mesh.uv = job.uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
    }
}