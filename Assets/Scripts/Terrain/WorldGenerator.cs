using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    public TerrainChunk chunkPrefab;
    public PlayerMotor playerController;
    // private int chunkRadius = 3;

    private int innerRadius = 1;
    private int outerRadius = 3;

    public Vector2Int playerCurrentChunkPos;
    public Vector2Int playerLastChunkPos;

    // Noise Variables
    public int seed = 42;
    public const float frequency = 1.5f;
    public const float amplitude = 2f;
    public const float scale = 1.7f;
    public const float lacunarity = 1.8f;
    public const float persistance = 0.5f;
    public const int octaves = 4;
    public float globalMinNoise;
    public float globalMaxNoise;

    public Vector2Int chunkCoord = new Vector2Int(0,0);

    public Queue<TerrainChunk> inactiveChunks = new Queue<TerrainChunk>();
    public Queue<TerrainChunk> ReadyToRender = new Queue<TerrainChunk>();

    public Dictionary<Vector2Int, TerrainChunk> activeChunks = new Dictionary<Vector2Int, TerrainChunk>();
    public Dictionary<Vector2Int, BlockType[,,]> WorldData = new Dictionary<Vector2Int, BlockType[,,]>();

    void Awake()
    {
        Vector2Int playerChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerController.transform.position.x / 16),
            Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;
        this.playerLastChunkPos = playerChunkPos;
    }

    void Start()
    {
        FastNoise noise = new FastNoise();
        (float minNoise, float maxNoise) = NoiseSampler.SampleNoiseRange(
            noise,
            1024,
            frequency,
            amplitude,
            scale,
            lacunarity,
            persistance,
            octaves
        );

        globalMinNoise = minNoise;
        globalMaxNoise = maxNoise;

        Debug.Log(globalMinNoise + " " + globalMaxNoise);

        GenerateWorld(true);
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
            GenerateWorld(false);   
        }
    }

    public IEnumerator DrawChunkDelay()
    {
        while(ReadyToRender.Count > 0)
        {
            TerrainChunk newChunk = ReadyToRender.Dequeue();
            Vector2Int pos = newChunk.chunkCoord;

            // si ya existe en world data, no lo quiero popular, quiero copiarle la data
            if(WorldData.ContainsKey(pos))
            {
                activeChunks[pos].chunkBlocks = WorldData[pos];
            }

            // si no existe en world data, lo quiero popular y guardar esa info en world data
            else
            {
                activeChunks[pos].populateChunk();
                WorldData[pos] = activeChunks[pos].chunkBlocks;
            }

            newChunk.buildMesh();
            yield return null;
        }
    }

    public void GenerateWorld(bool instant = false)
    {   
        // Get current player position in world and conver to chunk coordinates
        Vector2Int playerChunkPos = new Vector2Int(
        Mathf.FloorToInt(playerController.transform.position.x / 16),
        Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

        // Auxiliar list to remove activeChunks from activeChunks dict
        List<Vector2Int> toRemove = new List<Vector2Int>();

        // Check if existing activeChunks are outside of correct radius and add to "toRemove" list
        foreach (Vector2Int pos in activeChunks.Keys.ToList())
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

        // Remove, derender and enqueue activeChunks
        foreach (Vector2Int pos in toRemove)
        {
            activeChunks[pos].gameObject.SetActive(false);
            inactiveChunks.Enqueue(activeChunks[pos]);
            activeChunks.Remove(pos);
        }

        // Itarate through all activeChunks in radius
        for(int x = playerChunkPos.x - outerRadius; x <= playerChunkPos.x + outerRadius; x++)
        for(int z = playerChunkPos.y - outerRadius; z <= playerChunkPos.y + outerRadius; z++)
        {   
            chunkCoord = new Vector2Int(x, z);

            // If currently NOT in activeChunks dictionary, need to get one from pool (or create one if pool is empty)
            if(!activeChunks.ContainsKey(chunkCoord))
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

                // Add to activeChunks dictionary, change name and populate + buildmesh
                newChunk.name = $"Chunk_{x}_{z}";

                activeChunks.Add(chunkCoord, newChunk);
                newChunk.Init(chunkCoord, this, globalMinNoise, globalMaxNoise);
            }
        }

        foreach (Vector2Int pos in activeChunks.Keys)
        {
            if(instant)
            {
                // si ya existe en world data, no lo quiero popular, quiero copiarle la data
                if(WorldData.ContainsKey(pos))
                {
                    activeChunks[pos].chunkBlocks = WorldData[pos];
                }
                // si no existe en world data, lo quiero popular y guardar esa info en world data
                else
                {
                    activeChunks[pos].populateChunk();
                    WorldData[pos] = activeChunks[pos].chunkBlocks;
                }

                activeChunks[pos].buildMesh();
            }
            else
                ReadyToRender.Enqueue(activeChunks[pos]);
        }
        StartCoroutine(DrawChunkDelay());
    }
}
