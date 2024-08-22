using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    public GameObject selectionBoxPrefab;
    private GameObject selectionBox;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool isSelecting = false;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += StartSelection;
        inputActions.Player.Click.canceled += EndSelection;
        inputActions.Player.Select.performed += UpdateSelectionBox; // Update the box during drag
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= StartSelection;
        inputActions.Player.Click.canceled -= EndSelection;
        inputActions.Player.Select.performed -= UpdateSelectionBox;
        inputActions.Disable();
    }

    private void StartSelection(InputAction.CallbackContext context)
    {
        isSelecting = true;
        startPoint = GetMouseWorldPosition();
        selectionBox = Instantiate(selectionBoxPrefab, startPoint, Quaternion.identity);
    }

    private void EndSelection(InputAction.CallbackContext context)
    {
        if (!isSelecting) return;

        isSelecting = false;
        endPoint = GetMouseWorldPosition();

        SelectTilesInBox();
        Destroy(selectionBox);
    }

    private void UpdateSelectionBox(InputAction.CallbackContext context)
{
    if (!isSelecting) return;

    endPoint = GetMouseWorldPosition();

    // Calculate the minimum and maximum points to ensure proper sizing
    Vector3 min = Vector3.Min(startPoint, endPoint);
    Vector3 max = Vector3.Max(startPoint, endPoint);

    // Calculate the center and size of the selection box
    Vector3 center = (min + max) / 2;
    Vector3 size = max - min;

    // Fix y position and scale
    center.y = 0.7f; // Ensure y position is fixed
    size.y = 1; // Ensure y scale is fixed

    selectionBox.transform.position = center;
    selectionBox.transform.localScale = size;
}


    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mousePosition = inputActions.Player.Select.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private void SelectTilesInBox()
    {
        Collider[] colliders = Physics.OverlapBox(selectionBox.transform.position, selectionBox.transform.localScale / 2);

        foreach (var collider in colliders)
        {
            Tile tile = collider.GetComponent<Tile>();
            if (tile != null)
            {
                tile.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
    }
}
