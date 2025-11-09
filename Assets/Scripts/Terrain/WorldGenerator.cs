using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    public TerrainChunk chunkPrefab;
    public PlayerMotor playerController;
    private int chunkRadius = 3;

    public Vector3Int playerCurrentChunkPos;
    public Vector3Int playerLastChunkPos;


    public Vector2Int chunkCoord = new Vector2Int(0,0);

    public Queue<TerrainChunk> inactiveChunks;
    public Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

    void Start()
    {
        Vector3Int playerChunkPos = new Vector3Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.y),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;
        this.playerLastChunkPos = playerChunkPos;
        this.inactiveChunks = new Queue<TerrainChunk>();

        generateWorld();
    }

    void Update()
    {
        this.playerCurrentChunkPos = new Vector3Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.y),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        if(playerCurrentChunkPos != playerLastChunkPos)
        {
            playerLastChunkPos = playerCurrentChunkPos;
            RefreshWorld();
        }
    }

    public void generateWorld()
    {   
        // Get current player position in world and conver to chunk coordinates
        Vector3Int playerChunkPos = new Vector3Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.y),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

        // Itarate through all chunks in radius
        for(int x = playerChunkPos.x - chunkRadius; x <= playerChunkPos.x + chunkRadius; x++)
        for(int z = playerChunkPos.z - chunkRadius; z <= playerChunkPos.z + chunkRadius; z++)
        {   
            chunkCoord = new Vector2Int(x, z);

            TerrainChunk newChunk = Instantiate(chunkPrefab);
            newChunk.name = $"Chunk_{x}_{z}";

            chunks.Add(chunkCoord,newChunk);

            newChunk.Init(chunkCoord, this);
            newChunk.populateChunk();
            newChunk.buildMesh();

        }

        foreach (Vector2Int pos in chunks.Keys)
            chunks[pos].Init(pos, this);
    }

    public void RefreshWorld()
    {   
        // Get current player position in world and conver to chunk coordinates
        Vector3Int playerChunkPos = new Vector3Int(
        Mathf.FloorToInt(playerController.transform.position.x / 16),
        Mathf.FloorToInt(playerController.transform.position.y),
        Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

        // Auxiliar list to remove chunks from chunks dict
        List<Vector2Int> toRemove = new List<Vector2Int>();

        // Check if existing chunks are outside of correct radius and add to "toRemove" list
        foreach (Vector2Int pos in chunks.Keys.ToList())
        {   
            // Four possible scenarios
            bool a = pos.x < (playerChunkPos.x - chunkRadius);
            bool b = pos.x > (playerChunkPos.x + chunkRadius);
            bool c = pos.y < (playerChunkPos.z - chunkRadius);
            bool d = pos.y > (playerChunkPos.z + chunkRadius);

            if(a || b || c || d)
                toRemove.Add(pos);
        }

        // Remove, derender and enqueue chunks
        foreach (Vector2Int pos in toRemove)
        {
            chunks[pos].gameObject.SetActive(false);
            inactiveChunks.Enqueue(chunks[pos]);
            chunks.Remove(pos);
        }

        // Itarate through all chunks in radius
        for(int x = playerChunkPos.x - chunkRadius; x <= playerChunkPos.x + chunkRadius; x++)
        for(int z = playerChunkPos.z - chunkRadius; z <= playerChunkPos.z + chunkRadius; z++)
        {   
            chunkCoord = new Vector2Int(x, z);

            // If currently NOT in chunks dictionary, need to get one from pool (or create one if pool is empty)
            if(!chunks.ContainsKey(chunkCoord))
            {   
                // New empty chunk
                TerrainChunk newChunk;

                // Get from pool and re-activate or instantiate a new one if pool is empty
                if(inactiveChunks.Count > 0)
                {
                    newChunk = inactiveChunks.Dequeue();
                    newChunk.gameObject.SetActive(true);
                }

                else 
                    newChunk = Instantiate(chunkPrefab);

                // Clear previous chunk data
                newChunk.ClearData();

                // Add to chunks dictionary, change name and populate + buildmesh
                chunks.Add(chunkCoord, newChunk);
                newChunk.name = $"Chunk_{x}_{z}";

                newChunk.Init(chunkCoord, this);
                newChunk.populateChunk();
                // newChunk.buildMesh();
            }
        }

        foreach (Vector2Int pos in chunks.Keys)
            chunks[pos].buildMesh();
            
    }
}
