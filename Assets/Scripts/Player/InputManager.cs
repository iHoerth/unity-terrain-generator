using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions playerInput;
    private InputSystem_Actions.PlayerActions playerActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerMotor motor;
    private PlayerLook look;

    void Awake()
    {
        playerInput = new InputSystem_Actions();
        playerActions = playerInput.Player;
        
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        playerActions.Jump.performed += ctx => motor.Jump();
        playerActions.Crouch.performed += ctx => {
            Debug.Log("Crouch pressed!");
            motor.Crouch();
        };
        playerActions.Sprint.performed += ctx => motor.Sprint();
        playerActions.Attack.performed += ctx => look.Attack();
        playerActions.Build.performed += ctx => look.Build();
    }

    // Update is called once per frame/
    void FixedUpdate()
    {
        motor.ProcessMove(playerActions.Move.ReadValue<Vector2>());
    }

    void Update()
    {
        look.ProcessLook(playerActions.Look.ReadValue<Vector2>());
        look.Aim();
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }


}
