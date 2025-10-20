using UnityEngine;
// using System;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;

    public float speed = 5f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

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
    // lerp = linear interpolation. Unity tiene una built-in function Mathf.Lerp(A,B,t).
    // en general Lerp(A,B,t)= A + (B−A)∗t de manera que si t es 0, lerp devuelve A, si t 1, lerp devuelve B y si t 0,5 devuelve el punto medio.
    // En el caso anterior el comportamiento seria completamente lineal. Otra cosa q se hace es un ease-out, t *= t de esta manera el comportamiento es cuadratico.
    // En MC en realidad el crouch es sin lerp, pero voy a implementarlo para aprender.

    void Start()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        crouchHeight = standHeight * crouchRatio;
    }

    // Update is called once per frame
    void Update()
    {
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
    }
}
