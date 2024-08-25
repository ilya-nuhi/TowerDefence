using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] MapCreator mapCreator;
    [SerializeField] LayerMask tileLayerMask;
    public GameObject selectionBoxPrefab;
    private GameObject selectionBox;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Tile[,] tiles;
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

    private void Start() {
        tiles = mapCreator.tiles;
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

        StartCoroutine(SelectTilesInBox());
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

        return new Vector3(-1, -1, -1);
    }

    private IEnumerator SelectTilesInBox()
    {
        List<Tile> selectedWalls = new List<Tile>();
        List<Tile> selectedTiles = new List<Tile>();
        // Using the layer mask to filter out non-tile objects
        Collider[] colliders = Physics.OverlapBox(selectionBox.transform.position, selectionBox.transform.localScale / 2,
                                                     Quaternion.identity, tileLayerMask);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Tile>(out Tile tile))
            {
                selectedTiles.Add(tile);

                if (tile.Type == TileType.Wall)
                {
                    selectedWalls.Add(tile);
                }
            }
        }

        // Check for 3 or more successive wall tiles
        if (selectedWalls.Count >= 3)
        {
            yield return ConvertWallsToOccupied(selectedWalls);
            ExpandOccupiedArea(selectedTiles);
        }
    }

    private IEnumerator ConvertWallsToOccupied(List<Tile> walls)
    {
        foreach (Tile tile in walls)
        {
            if(tile.Type==TileType.Wall && tile.TryGetComponent<Wall>(out Wall currentWall)){
                // destroying wall component from the tile
                Destroy(currentWall);
                StartCoroutine(tile.LowerTile());
                tile.SetTileType(TileType.Occupied);
            }
        }
        yield return null;
    }

    private void ExpandOccupiedArea(List<Tile> selectedTiles)
    {
        // Find the boundaries of the selected area
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (var tile in selectedTiles)
        {
            if (tile.X < minX) minX = tile.X;
            if (tile.X > maxX) maxX = tile.X;
            if (tile.Z < minZ) minZ = tile.Z;
            if (tile.Z > maxZ) maxZ = tile.Z;
        }
        
        
        // Expand the occupied area to include all selected tiles
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                if (tiles[z, x] != null && tiles[z, x].Type == TileType.Empty)
                {
                    tiles[z, x].SetTileType(TileType.Occupied);
                }
            }
        }

        // Add walls around the expanded occupied area
        AddWallsAroundArea(minX, maxX, minZ, maxZ);
    }

    private void AddWallsAroundArea(int minX, int maxX, int minZ, int maxZ)
    {
        // Add walls along the minimum and maximum x-axis
        for (int x = minX; x <= maxX; x++)
        {
            HandleBordering(minZ, x, minZ-1, x);
            HandleBordering(maxZ, x, maxZ+1, x);
        }

        // Add walls along the minimum and maximum z-axis
        for (int z = minZ; z <= maxZ; z++)
        {
            HandleBordering(z, minX, z, minX-1);
            HandleBordering(z, maxX, z, maxX+1);
        }
    }

    // looking other side of the border to decide what should the tile be
    private void HandleBordering(int originZ, int originX, int lookedZ, int lookedX)
    {
        Tile currentTile = tiles[originZ, originX];
        Tile lookedTile = tiles[lookedZ, lookedX];

        if (currentTile != null && lookedTile != null)
        {
            if (lookedTile.Type == TileType.Empty)
            {
                currentTile.SetTileType(TileType.Wall);
            }
            else if (lookedTile.Type == TileType.Wall)
            {
                // if (currentTile.Type != TileType.Wall && lookedTile.TryGetComponent<Wall>(out Wall currentWall))
                // {
                //     Destroy(currentWall);
                //     StartCoroutine(lookedTile.LowerTile());
                //     lookedTile.SetTileType(TileType.Occupied);
                // }
                //currentTile.SetTileType(TileType.Occupied);
            }
        }
    }


}
