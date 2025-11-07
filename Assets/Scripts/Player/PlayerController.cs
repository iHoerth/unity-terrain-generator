// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerController : MonoBehaviour
// {
//     [Header("References")]
//     public Transform playerCamera;

//     [Header("Movement Settings")]
//     public float moveSpeed = 5f;
//     public float jumpHeight = 2f;
//     public float gravity = -9.81f;

//     [Header("Mouse Settings")]
//     public float mouseSensitivity = 100f;

//     private CharacterController controller;
//     private InputSystem_Actions input;
//     private float xRotation = 0f;
//     private Vector3 velocity;
//     private bool jumpRequested = false;

//     // -----------------------------------------------------

//     void Awake()
//     {
//         input = new InputSystem_Actions();
//         controller = GetComponent<CharacterController>();
//     }

//     void OnEnable()
//     {
//         input.Enable();
//         input.Player.Jump.performed += OnJump; // escucha evento de salto
//     }

//     void OnDisable()
//     {
//         input.Player.Jump.performed -= OnJump;
//         input.Disable();
//     }

//     void Start()
//     {
//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }

//     void Update()
//     {
//         // isGrounded = controller.isGrounded;
//         HandleLook();
//         HandleMovement();

//         Aim();
//     }

//     // -----------------------------------------------------

//     private void HandleLook()
//     {
//         Vector2 look = input.Player.Look.ReadValue<Vector2>();

//         float mouseX = look.x * mouseSensitivity * Time.deltaTime;
//         float mouseY = look.y * mouseSensitivity * Time.deltaTime;

//         xRotation -= mouseY;
//         xRotation = Mathf.Clamp(xRotation, -90f, 90f);

//         playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//         transform.Rotate(Vector3.up * mouseX);
//     }

//     private void HandleMovement()
//     {
//         // Lectura WASD
//         Vector2 move = input.Player.Move.ReadValue<Vector2>();
//         Vector3 moveDir = transform.right * move.x + transform.forward * move.y;

//         // Movimiento horizontal
//         controller.Move(moveDir * moveSpeed * Time.deltaTime);

//         // Gravedad
//         if (controller.isGrounded && velocity.y < 0)
//             velocity.y = -2f;

//         // Salto
//         if (controller.isGrounded && jumpRequested)
//         {
//             velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
//             jumpRequested = false;
//         }

//         // Aplicar gravedad
//         velocity.y += gravity * Time.deltaTime;
//         controller.Move(velocity * Time.deltaTime);
//     }

//     private void OnJump(InputAction.CallbackContext context)
//     {
//         jumpRequested = true;
//     }

 
// }
