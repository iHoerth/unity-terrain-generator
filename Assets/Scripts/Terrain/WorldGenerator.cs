using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    public TerrainChunk chunkPrefab;
    public PlayerMotor playerController;
    // private int chunkRadius = 3;

    private int innerRadius = 5;
    private int outerRadius = 12;

    public Vector2Int playerCurrentChunkPos;
    public Vector2Int playerLastChunkPos;


    public Vector2Int chunkCoord = new Vector2Int(0,0);

    public Queue<TerrainChunk> inactiveChunks;
    public Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

    void Start()
    {
        Vector2Int playerChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;
        this.playerLastChunkPos = playerChunkPos;
        this.inactiveChunks = new Queue<TerrainChunk>();

        generateWorld();
    }

    void Update()
    {
        this.playerCurrentChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        float distance = Vector2.Distance(playerLastChunkPos, playerCurrentChunkPos);

        if(distance >= (outerRadius - innerRadius))
        {
            playerLastChunkPos = playerCurrentChunkPos;
            RefreshWorld();            
        }
    }

    public void generateWorld()
    {   
        // Get current player position in world and conver to chunk coordinates
        Vector2Int playerChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

        // Itarate through all chunks in radius
        for(int x = playerChunkPos.x - outerRadius; x <= playerChunkPos.x + outerRadius; x++)
        for(int z = playerChunkPos.y - outerRadius; z <= playerChunkPos.y + outerRadius; z++)
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
        Vector2Int playerChunkPos = new Vector2Int(
        Mathf.FloorToInt(playerController.transform.position.x / 16),
        Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

        // Auxiliar list to remove chunks from chunks dict
        List<Vector2Int> toRemove = new List<Vector2Int>();

        // Check if existing chunks are outside of correct radius and add to "toRemove" list
        foreach (Vector2Int pos in chunks.Keys.ToList())
        {   
            // Four possible scenarios
            bool a = pos.x < (playerChunkPos.x - outerRadius);
            bool b = pos.x > (playerChunkPos.x + outerRadius);
            bool c = pos.y < (playerChunkPos.y - outerRadius);
            bool d = pos.y > (playerChunkPos.y + outerRadius);

            // bool a2 = true;
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
        for(int x = playerChunkPos.x - outerRadius; x <= playerChunkPos.x + outerRadius; x++)
        for(int z = playerChunkPos.y - outerRadius; z <= playerChunkPos.y + outerRadius; z++)
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
