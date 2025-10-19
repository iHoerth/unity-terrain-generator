using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public GameObject cube;

    void Start()
    {
        for(int x = -10; x <= 10; x++)
            for(int z = -10; z <= 10; z++)
            {
                Instantiate(cube , new Vector3(x, 0 , z), Quaternion.identity);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
