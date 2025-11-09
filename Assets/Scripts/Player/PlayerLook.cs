using UnityEngine;

public class PlayerLook : MonoBehaviour
{   
    public Camera cam;
    public RaycastHit hit;  

    // Input variables
    private float xRotation = 0f;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    // hit Getter
    public RaycastHit CurrentHit => hit;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ProcessLook(Vector2 input) // we get the input from the InputManager
    {
        float mouseX = input.x;
        float mouseY = input.y;
        float maxRotationX = 90;

        // calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -maxRotationX, maxRotationX);

        //apply to our cam transform
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //rotate our player
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }

    public void Aim()
    {
        Vector3 origin = cam.transform.position + cam.transform.forward * 0.1f;
        Vector3 direction = cam.transform.forward;
        float maxDistance = 3f;

        Physics.Raycast(origin, direction, out hit, maxDistance);
        // if(Physics.Raycast(origin, direction, out hit, maxDistance))
        // {
        //     Debug.DrawRay(origin, direction, Color.red);
        // }
        // else {
        //     Debug.DrawRay(origin, direction, Color.green);
        // }
    }

    public void Attack()
    {
        if (hit.collider == null) return;
        // Position of the Raycast hit (minus normal) to be "inside" the cube and ensure being in the correct cube 
        Vector3Int hitGlobalPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);

        BlockType newType = BlockType.Air;

        GameObject target = hit.collider.gameObject;
        
        if (target.TryGetComponent<TerrainChunk>(out TerrainChunk chunk))
        {
            Vector3Int hitLocalPos = chunk.GlobalToLocal(hitGlobalPos);
            chunk.ConvertBlock(hitGlobalPos, newType);
            chunk.buildMesh();

            if (chunk.isChunkLimit(hitGlobalPos))
            {
                foreach (Vector2Int chord in chunk.neighbours.Values)
                {
                    if (chunk.world.chunks.ContainsKey(chord))
                        chunk.world.chunks[chord].buildMesh();
                }
            }
        }
    }

    public void Build(BlockType newBlock = BlockType.Dirt)
    {  
        // Return when Raycast pointing null to avoid error 
        if (hit.collider == null) return;
        // Position of the Raycast hit (plus normal) to be "outside" the cube in the direction of the face i am currently looking 
        Vector3Int hitGlobalPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);
        GameObject target = hit.collider.gameObject;

        // Get hit chunk reference if possible
        if (target.TryGetComponent<TerrainChunk>(out TerrainChunk chunk))
        {   
            if(chunk.InsideChunk(hitGlobalPos))
            {
                // Convert block and build entire mesh
                chunk.ConvertBlock(hitGlobalPos, newBlock);
                chunk.buildMesh();
            }
            
            else 
            {
                // Versor normal and reduce it to R2 (x,z)
                Vector2Int fixedNormal = new Vector2Int(Mathf.RoundToInt(hit.normal.x), Mathf.RoundToInt(hit.normal.z));
                // Obtain neighbour coords based on the normal and current chunk coords.
                Vector2Int neighbourCoords = chunk.chunkCoord + fixedNormal;
                // Neighbour chunk should exist because the player executes the build function and chunks render around player
                TerrainChunk neighbourChunk = chunk.world.chunks[neighbourCoords];

                // Convert block and build entire mesh
                neighbourChunk.ConvertBlock(hitGlobalPos, newBlock);
                neighbourChunk.buildMesh();

                // Rebuild neighbour to avoid duplicate in-faces.
                chunk.buildMesh(); 
            }
        }
    }
}
