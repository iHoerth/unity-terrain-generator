using UnityEngine;

public class PlayerLook : MonoBehaviour
{   
    public Camera cam;
    public RaycastHit hit;  
    
    private float xRotation = 0f;

    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ProcessLook(Vector2 input) // we get the input from the InputManager
    {
        float mouseX = input.x;
        float mouseY = input.y;

        // calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80, 80);

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

    public void Build(BlockType newBlock)
    {   
        // obtener refe del chunk q estoy mirando
        // ConvertType (newBlock type) del bloque qe estoy mirando pero con y+1 
        // buildMesh 
        // UpdateInventory en un futuro o algo asi
    }
}
