using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Diagnostics;

public class WorldGenerator : MonoBehaviour
{
    // Thread classes
    class ChunkJob
    {
        public Vector2Int coord;
        public BlockType[,,] blocks;

        public ChunkJob(Vector2Int coord, BlockType[,,] blocks)
        {
            this.coord = coord;
            this.blocks = blocks;
        }
    }

    class ChunkResult
    {
        public Vector2Int coord;
        public BlockType[,,] blocks;
        public MeshData mesh;

        public ChunkResult(Vector2Int coord, BlockType[,,] blocks, MeshData mesh)
        {
            this.coord = coord;
            this.blocks = blocks;
            this.mesh = mesh;
        }
    }

    // Player variables
    public PlayerMotor playerController;
    public Vector2Int playerCurrentChunkPos;
    public Vector2Int playerLastChunkPos;

    // Noise variables
    public int seed = 42;
    public float persistance = 0.5f;
    public float lacunarity = 1.7f;
    public float amplitude = 2f;
    public float frequency = 1.35f;
    public float scale = 3.10f;
    public int octaves = 4;
    public float heightMultiplier = 30f;
    public AnimationCurve meshHeightCurve;
    public float globalMinNoise;
    public float globalMaxNoise;
    public float caveScaleW = 10f;
    public float caveScaleH = 5f;
    
    // ChunkData & Render variables
    public int innerRadius = 7;
    public int outerRadius = 8;
    public TerrainChunk chunkPrefab;
    public Vector2Int chunkCoord = new Vector2Int(0,0);
    public Dictionary<Vector2Int, BlockType[,,]> WorldData = new Dictionary<Vector2Int, BlockType[,,]>();
    public Dictionary<Vector2Int, TerrainChunk> activeChunks = new Dictionary<Vector2Int, TerrainChunk>();
    public Queue<TerrainChunk> inactiveChunks = new Queue<TerrainChunk>();
    public Queue<(TerrainChunk chunk, MeshData mesh)> ReadyToRender = new Queue<(TerrainChunk chunk, MeshData mesh)>();

    // Thread Variables
    Queue<ChunkJob> jobQueue = new Queue<ChunkJob>();
    Queue<ChunkResult> resultQueue = new Queue<ChunkResult>();
    readonly object jobLock = new object();
    readonly object resultLock = new object();
    bool workersRunning = true;

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
        int logicalCores = Environment.ProcessorCount;
        int workerCount = Mathf.Max(1, logicalCores - 2);

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

        CreateWorkers(workerCount);
        GenerateWorld(true);
        StartCoroutine(DrawChunkDelay());
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
        while(true)
        {   
            ChunkResult jobResult = null;
            
            lock(resultLock)
            {   
                if(resultQueue.Count > 0)
                    jobResult = resultQueue.Dequeue(); 
            }

            if(jobResult != null)
            {
                if(activeChunks.TryGetValue(jobResult.coord, out TerrainChunk chunk))
                {
                    chunk.blocks = jobResult.blocks;
                    chunk.DrawMesh(jobResult.mesh);
                }
            }

            yield return null;
        }
    }

    public void GenerateWorld(bool instant = false)
    {   
        // var swTotal = Stopwatch.StartNew();
        // Get current player position in world and convert to chunk coordinates
        Vector2Int playerChunkPos = new Vector2Int(
        Mathf.FloorToInt(playerController.transform.position.x / 16),
        Mathf.FloorToInt(playerController.transform.position.z / 16)
        );

        this.playerCurrentChunkPos = playerChunkPos;

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
            {
                activeChunks[pos].gameObject.SetActive(false);
                inactiveChunks.Enqueue(activeChunks[pos]);
                activeChunks.Remove(pos);
            }
        }
        StartCoroutine(GenerateChunksDelay());

        // var swCreateJobs = Stopwatch.StartNew();
        // Itarate through all chunks in radius

            // StartCoroutine(DrawChunkDelay());
        // swCreateJobs.Stop();
        // swTotal.Stop();
        // UnityEngine.Debug.Log(
        //     $"GenerateWorld: total={swTotal.ElapsedMilliseconds} ms | " +
        //     $"scan/remove={swScanRemove.ElapsedMilliseconds} ms | " +
        //     $"deactivate={swDeactivate.ElapsedMilliseconds} ms | " +
        //     $"create+jobs={swCreateJobs.ElapsedMilliseconds} ms"
        // );
    }

    IEnumerator GenerateChunksDelay()
    {
        int createdThisFrame = 0;
        int budget = 1; 
        
        for(int x = playerCurrentChunkPos.x - outerRadius; x <= playerCurrentChunkPos.x + outerRadius; x++)
        for(int z = playerCurrentChunkPos.y - outerRadius; z <= playerCurrentChunkPos.y + outerRadius; z++)
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

                lock(jobLock)
                {
                    // BlockType[,,] jobBlocks = new BlockType[
                    // TerrainChunk.chunkWidth,
                    // TerrainChunk.chunkHeight,
                    // TerrainChunk.chunkWidth
                    // ];

                    ChunkJob job = new ChunkJob(chunkCoord, newChunk.blocks);

                    jobQueue.Enqueue(job);
                    Monitor.Pulse(jobLock);
                }

                createdThisFrame++;

                // si ya hicimos varios en este frame, pausamos hasta el siguiente
                if (createdThisFrame >= budget)
                {
                    createdThisFrame = 0;
                    yield return null; // seguir en el próximo frame
                }
            }
        }
    }

    void CreateWorkers(int n)
    {   
        for(int i = 0; i < n; i++)
        {
            var thread = new Thread(WorkerLoop);
            thread.IsBackground = true;
            thread.Start();
        }
    }

    void WorkerLoop()
    {
        ChunkJob job = null;

        while(workersRunning)
        {

            lock(jobLock)
            {
                while(jobQueue.Count == 0)
                    Monitor.Wait(jobLock);

                job = jobQueue.Dequeue();
            }

            ChunkData chunkData = new ChunkData(
                job.coord,
                job.blocks,
                this
            );

            chunkData.PopulateChunkData();
            MeshData meshData = chunkData.GenerateMeshData();

            ChunkResult result = new ChunkResult(
                job.coord,
                job.blocks,
                meshData            
            );

            // calcular datos
            lock(resultLock)
            {
                resultQueue.Enqueue(result);
            }
        }
    }
}
