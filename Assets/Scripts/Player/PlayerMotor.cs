using UnityEngine;
// using System;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    public float speed = 5f;

    public float gravity = -9.8f;
    public float jumpHeight = 3f;
    private bool isGrounded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
    }
    // Receive inputs from input manage and apply to charac contr
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;

        if(isGrounded & playerVelocity.y < 0)
            playerVelocity.y = -2f; //hardcode
        
        controller.Move(playerVelocity * Time.deltaTime);

        // Debug.Log(playerVelocity.y);
        // Debug.Log($"Input: {input} | MoveDir: {moveDirection}");
        Debug.Log($"Player rotation: {transform.eulerAngles}");


    }

    public void Jump()
    {
        if(isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }
}
