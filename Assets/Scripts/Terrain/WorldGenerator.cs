using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public TerrainChunk chunkPrefab;

    private int chunkRadius = 3;

    public Vector2Int chunkCoord = new Vector2Int(0,0);

    public Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

    void Start()
    {
        generateWorld();
    }

    void Update()
    {
        
    }

    public void generateWorld()
    {   
        for(int x = -chunkRadius; x <= chunkRadius; x++)
        for(int z = -chunkRadius; z <= chunkRadius; z++)
        {   
            chunkCoord = new Vector2Int(x,z);

            TerrainChunk newChunk = Instantiate(chunkPrefab);
            newChunk.name = $"Chunk_{x}_{z}";

            chunks.Add(chunkCoord,newChunk);

            newChunk.Init(chunkCoord, this);
            newChunk.populateChunk();
            newChunk.buildMesh();

        }

        foreach (var kvp in chunks)
            kvp.Value.Init(kvp.Key, this);
    }
}
