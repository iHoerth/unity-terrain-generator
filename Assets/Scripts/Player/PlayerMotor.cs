using UnityEngine;
// using System;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    public float speed = 5f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 3f;

    private bool isGrounded;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    private bool sprinting = false;

    [SerializeField] private float crouchRatio = 0.5f; // 50% de la altura original
    [SerializeField] private float crouchDuration = 1f;
    private float standHeight;
    private float crouchHeight;
    private float crouchTimer;
    private bool crouching = false;
    private bool lerpCrouch = false; 
    private bool spawned = false;

    public WorldGenerator world;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        crouchHeight = standHeight * crouchRatio;
    }

    void HandleSpawn()
    {
        Vector2Int chunkPos = new Vector2Int(
            Mathf.FloorToInt(transform.position.x/16),
            Mathf.FloorToInt(transform.position.z/16)
        );
        Debug.Log(chunkPos + " CHUNKPOS");

        Vector3Int playerPosInt = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );
        Debug.Log(playerPosInt + " PlayerPOsInt");

        if((world.activeChunks.TryGetValue(chunkPos, out TerrainChunk currentChunk)))
        {
            Vector3Int chunklocalPos = currentChunk.GlobalToLocal(playerPosInt);

            int spawnHeight = TerrainChunk.chunkHeight;

            
            for(int y = TerrainChunk.chunkHeight; y >= 0; y--)
            {
                if(currentChunk.blocks[chunklocalPos.x, y - 1, chunklocalPos.z] != BlockType.Air)
                {
                    spawnHeight = y - 1;
                    break;
                }
            }
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x, spawnHeight + 0.1f, transform.position.z);
            controller.enabled = true;
        }

        spawned = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!world.loading && !spawned) HandleSpawn();
        isGrounded = controller.isGrounded;

        if(lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / crouchDuration;
            p *= p; // ease-out

            float targetHeight = crouching ? crouchHeight : standHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, p);

            if (p > 1f)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }

    }
    // Receive inputs from input manage and apply to charac contr
    public void ProcessMove(Vector2 input)
    {
        if(!spawned) return;
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;

        if(isGrounded & playerVelocity.y < 0)
            playerVelocity.y = -2f; //hardcode
        
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if(isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void Sprint()
    {
        sprinting = !sprinting;

        if(sprinting)
            speed = sprintSpeed;
        else
            speed = walkSpeed;        
    }

    public void Crouch()
    {
        crouching = !crouching;
        crouchTimer = 0f;
        lerpCrouch = true;

        speed = crouching ? crouchSpeed : walkSpeed;
    }
}
