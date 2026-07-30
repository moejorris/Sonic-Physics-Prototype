using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set;}

    [HideInInspector] public Vector2 moveInputDir {get; private set;}
    [HideInInspector] public bool jumpButtonDown {get; private set;}
    [HideInInspector] public bool jumpButton {get; private set;}
    [HideInInspector] public bool debugButtonDown {get; private set;}
    [HideInInspector] public bool sloModeButtonDown {get; private set;}
    [HideInInspector] public bool slowModeButtonUp {get; private set;}

    [SerializeField] InputActionReference moveInput;
    [SerializeField] InputActionReference jumpInput;
    [SerializeField] InputActionReference debugInput;
    [SerializeField] InputActionReference sloModeInput;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        moveInputDir = moveInput.action.ReadValue<Vector2>();

        
        jumpButton = jumpInput.action.IsPressed();
        jumpButtonDown = jumpInput.action.WasPressedThisFrame();
        debugButtonDown = debugInput.action.WasPressedThisFrame();
        sloModeButtonDown = sloModeInput.action.WasPressedThisFrame();
        slowModeButtonUp = sloModeInput.action.WasReleasedThisFrame();
    }

    public void OverrideMoveInput(Vector2 overriddenInput)
    {
        moveInputDir = overriddenInput;
    }
}
