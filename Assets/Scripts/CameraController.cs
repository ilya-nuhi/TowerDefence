using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f; // Speed for XZ movement
    [SerializeField] private float scrollSpeed = 2f; // Speed for Y movement (scroll)
    [SerializeField] private float minY = 10f; // Minimum height for Y axis
    [SerializeField] private float maxY = 50f; // Maximum height for Y axis

    private PlayerInputActions playerInputActions;
    private Vector2 moveInput;
    private float scrollInput;

    private void Awake()
    {
        // Initialize the input system
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Enable the input actions
        playerInputActions.Enable();

        // Subscribe to the movement action callback
        playerInputActions.Player.MoveCameraXZ.performed += OnMovePerformed;
        playerInputActions.Player.MoveCameraXZ.canceled += OnMoveCanceled;
        // Subscribe to the scroll action callback for Y-axis movement
        playerInputActions.Player.MoveCameraScroll.performed += OnScrollPerformed;
        playerInputActions.Player.MoveCameraScroll.canceled += OnScrollCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from the callbacks
        playerInputActions.Player.MoveCameraXZ.performed -= OnMovePerformed;
        playerInputActions.Player.MoveCameraXZ.canceled -= OnMoveCanceled;
        
        playerInputActions.Player.MoveCameraScroll.performed -= OnScrollPerformed;
        playerInputActions.Player.MoveCameraScroll.canceled -= OnScrollCanceled;

        // Disable the input actions
        playerInputActions.Disable();
    }

    // This function is called when the movement keys are pressed or held
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // This function is called when the movement keys are released
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero; // Reset movement when no keys are pressed
    }
    
    // This function is called when the scroll input is performed
    private void OnScrollPerformed(InputAction.CallbackContext context)
    {
        scrollInput = context.ReadValue<float>();
    }

    // This function is called when the scroll input is canceled
    private void OnScrollCanceled(InputAction.CallbackContext context)
    {
        scrollInput = 0f; // Reset scroll input when not scrolling
    }

    private void Update()
    {
        // Continuously move the camera every frame based on the moveInput
        MoveCamera(moveInput, scrollInput);
    }

    // This is where you move the camera based on input
    private void MoveCamera(Vector2 moveDirection, float scroll)
    {
        // Move in X and Z directions based on input
        Vector3 move = new Vector3(moveDirection.x, 0f, moveDirection.y) * (moveSpeed * Time.deltaTime);
        transform.Translate(move, Space.World);

        // Adjust the Y-axis based on scroll input
        float newY = transform.position.y - scroll * scrollSpeed * Time.deltaTime;
        newY = Mathf.Clamp(newY, minY, maxY); // Clamp the Y position

        // Apply the new Y position
        Vector3 newPosition = new Vector3(transform.position.x, newY, transform.position.z);
        transform.position = newPosition;
    }
}
