using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
// Debug
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainChunk : MonoBehaviour
{
    public enum NormalizeMode {Local, Global};

    // World variables
    public const int chunkHeight = 128;
    public const int chunkWidth = 16;
    public Vector2Int chunkCoord;
    public WorldGenerator world;
    public Dictionary<Direction, Vector2Int> neighbours = new();

    // Noise Variables
    public float minNoiseValue;
    public float maxNoiseValue;   

    // Mesh variables
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;

    // Chunkdata Variables
    public BlockType[,,] blocks = new BlockType[chunkWidth, chunkHeight, chunkWidth];
    public Dictionary<Vector3Int, BlockType> blockData = new();

    void Start()
    {
        // Iterates neighbour dictionary
        // foreach (Direction dir in neighbours.Keys)
        // {
        //     Vector2Int neighbourCoord = neighbours[dir];
        //     // Tries to get neighbour chunk reference
        //     if (world.activeChunks.TryGetValue(neighbourCoord, out TerrainChunk neighbour))
        //         // Forces neighbour to "rebuild" mesh to clean duplicate faces.
        //         neighbour.GenerateMeshData();
        //         neighbour.DrawMesh();
        // }
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
    
    public void ClearData()
    {
        // vertices.Clear();
        // uvs.Clear();
        // triangles.Clear();
        // meshFilter.sharedMesh = null;
        neighbours.Clear();
        meshCollider.sharedMesh = null;
        mesh.Clear();
        Array.Clear(blocks, 0, blocks.Length);
    }
    
    // Initializes the activeChunks & neighbours global position variables
    public void Init(Vector2Int chunkCoord, WorldGenerator world, float globalMinNoise, float globalMaxNoise)
    {
        this.chunkCoord = chunkCoord;
        this.world = world;
        this.minNoiseValue = globalMinNoise;
        this.maxNoiseValue = globalMaxNoise;

        neighbours.Clear();

        neighbours.Add(Direction.East, new Vector2Int(chunkCoord.x + 1, chunkCoord.y));
        neighbours.Add(Direction.West, new Vector2Int(chunkCoord.x - 1, chunkCoord.y));
        neighbours.Add(Direction.North, new Vector2Int(chunkCoord.x, chunkCoord.y + 1));
        neighbours.Add(Direction.South, new Vector2Int(chunkCoord.x, chunkCoord.y - 1));
     }

    public void DrawMesh(MeshData meshData)
    {
        // Mesh mesh = new Mesh();
        mesh.Clear();

        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.SetUVs(0, meshData.uvs);
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;

        // mesh.RecalculateTangents();
        // mesh.RecalculateBounds();

        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;  
    }

    // Checks if neighbour chunk has air in the border at that position
    public bool checkNeighbourAir(Vector2Int chunkCoords, Vector3Int currentPos)
    {
        int x = currentPos.x;
        int y = currentPos.y;
        int z = currentPos.z;

        if(world.activeChunks.TryGetValue(chunkCoords, out TerrainChunk neighbour))
        if(neighbour.blocks[x, y, z] == BlockType.Air)
            return true;

        return false;
    }   

    // Checks if the given global position is the chunk local limit x,z = 0 || x,z = 15.
    public bool isChunkLimit(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x == 0 || localPos.x == chunkWidth -1 || localPos.z == 0 || localPos.z == chunkWidth - 1)
            return true;

        return false;
    }

    // Checks if the given global pos is outside the chunk local coords.
    public bool InsideChunk(Vector3Int globalPos)
    {
        Vector3Int localPos = GlobalToLocal(globalPos);

        if(localPos.x < 0 || localPos.x > chunkWidth -1)     
            return false;

        if(localPos.z < 0 || localPos.z > chunkWidth -1)     
            return false;

        return true;
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
            blocks[localPos.x, localPos.y , localPos.z] = newType;
    }


}