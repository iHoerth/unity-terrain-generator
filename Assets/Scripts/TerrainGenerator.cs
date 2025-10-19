using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public GameObject cube;
    public float scale = 0.05f; //controls noise frequency
    public float freq = 0.05f;
    public int seed = 42;

    public float height = 50f; //controls noise maximum height
    public int xwidth = 50;
    public int zwidth = 50;

    void Start()
    {
        FastNoise noise = new FastNoise();
        
        noise.SetNoiseType(FastNoise.NoiseType.Perlin);
        noise.SetFrequency(freq);
        noise.SetSeed(seed);
        
        for(int x = -xwidth; x <= xwidth; x++)
            for(int z = -zwidth; z <= zwidth; z++)
            {
                float y = noise.GetNoise(x, z, 0) * height;
                y = Mathf.Round(y); // discrete values like mc

                Instantiate(cube , new Vector3(x, y , z), Quaternion.identity);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
