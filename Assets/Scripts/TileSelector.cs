using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] MapCreator mapCreator;
    [SerializeField] LayerMask tileLayerMask;
    [SerializeField] GameObject selectionBoxPrefab;
    [SerializeField] GameObject invisBoxPrefab;
    private GameObject startObj;
    private Vector3 endPoint;
    private Tile[,] tiles;
    private List<Tile> walls;
    private List<GameObject> _selectedWalls;
    private List<Tile> _selectedTiles;
    private Tile _lastSelectedTile;
    List<Tile> _newOuterWalls;
    private bool isSelecting = false;

    // Boundaries of smallest rectangle that can cover our walls
    private int minX, maxX, minZ, maxZ;

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
        inputActions.Player.Select.performed += UpdateSelectedWalls; // Update the box during drag
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= StartSelection;
        inputActions.Player.Click.canceled -= EndSelection;
        inputActions.Player.Select.performed -= UpdateSelectedWalls;
        inputActions.Disable();
    }

    private void Start() {
        tiles = mapCreator.tiles;
        walls = mapCreator.walls;
    }

    private void StartSelection(InputAction.CallbackContext context)
    {
        startObj = GetMouseHitObject();
        if(startObj.TryGetComponent<Tile>(out Tile selectedTile) && selectedTile.Type == TileType.Wall){
            _selectedWalls = new List<GameObject>();
            _selectedTiles = new List<Tile>();
            GameObject selectionWall = Instantiate(selectionBoxPrefab, new Vector3(startObj.transform.position.x, 1, startObj.transform.position.z), Quaternion.identity);
            _selectedWalls.Add(selectionWall);
            _lastSelectedTile = selectedTile;
            isSelecting = true;
        }
    }

    private void EndSelection(InputAction.CallbackContext context)
    {
        if (!isSelecting) return;
        isSelecting = false;
        GameObject nextTile = GetMouseHitObject();
        // If the wall selection is valid expand.
        if(nextTile.TryGetComponent<Tile>(out Tile selectedTile) && selectedTile.Type == TileType.Wall && CheckAdjacencyOfTiles(_lastSelectedTile, selectedTile)){
            foreach(Tile tile in _selectedTiles){
                if(tile.Type == TileType.Empty){
                    tile.SetTileType(TileType.Wall);
                    walls.Add(tile);
                }
            }
            FindNewOuterWalls();
            BreakInnerWalls();
            BuildNewArcherTowers();
            walls = new List<Tile>(_newOuterWalls);
        }

        // Destroy all GameObjects in _selectedWalls
        foreach (GameObject selectionWall in _selectedWalls)
        {
            Destroy(selectionWall);
        }
        // Clear the references for selected walls
        _selectedWalls.Clear();
        // Clear the references for selected tiles
        _selectedTiles.Clear();
        
    }

    private void UpdateSelectedWalls(InputAction.CallbackContext context){
        if (!isSelecting) return;
            GameObject nextTile = GetMouseHitObject();
            if(nextTile.TryGetComponent<Tile>(out Tile selectedTile) && selectedTile.Type == TileType.Empty && CheckAdjacencyOfTiles(_lastSelectedTile, selectedTile)){
                GameObject selectionWall = Instantiate(selectionBoxPrefab, new Vector3(nextTile.transform.position.x, 1, nextTile.transform.position.z), Quaternion.identity);
    
                // Calculate the direction from the last selected tile to the new tile
                Vector3 direction = new Vector3(_lastSelectedTile.X - selectedTile.X , 0, _lastSelectedTile.Z - selectedTile.Z);
                // Calculate the left and right directions (perpendicular to the direction)
                Vector3 leftDirection = new Vector3(-direction.z, 0, direction.x);
                Vector3 rightDirection = new Vector3(direction.z, 0, -direction.x);
                // Invisible walls are to prevent building walls next to each other
                GameObject invisWallLeft = Instantiate(invisBoxPrefab, new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) + leftDirection, Quaternion.identity);
                GameObject invisWallRight = Instantiate(invisBoxPrefab, new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) + rightDirection, Quaternion.identity);
                GameObject invisWallBack = Instantiate(invisBoxPrefab, new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) + direction, Quaternion.identity);
                GameObject invisWallBackRight = Instantiate(invisBoxPrefab, new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) + direction + rightDirection, Quaternion.identity);
                GameObject invisWallBackLeft = Instantiate(invisBoxPrefab, new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) + direction + leftDirection, Quaternion.identity);
                
                _selectedWalls.Add(selectionWall);
                _selectedWalls.Add(invisWallLeft);
                _selectedWalls.Add(invisWallRight);
                _selectedWalls.Add(invisWallBack);
                _selectedWalls.Add(invisWallBackRight);
                _selectedWalls.Add(invisWallBackLeft);

                _selectedTiles.Add(selectedTile);
                _lastSelectedTile = selectedTile;
            }

        return;
    }

    private bool CheckAdjacencyOfTiles(Tile tile1, Tile tile2)
    {
        // Check if both tiles are not null
        if (tile1 == null || tile2 == null){return false;}

        // Check if the tiles are directly next to each other
        int dx = Mathf.Abs(tile1.X - tile2.X);
        int dz = Mathf.Abs(tile1.Z - tile2.Z);

        // Tiles are adjacent if they differ by 1 in either axis and are the same in the other axis
        return (dx == 1 && dz == 0) || (dx == 0 && dz == 1);
    }


    private GameObject GetMouseHitObject()
    {
        Vector2 mousePosition = inputActions.Player.Select.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    private void FindMinMaxPoints()
    {
        // Initialize min and max values
        minX = int.MaxValue;
        maxX = int.MinValue;
        minZ = int.MaxValue;
        maxZ = int.MinValue;

        // Iterate over the walls list
        foreach (Tile wall in walls)
        {
            // Compare and update minX and maxX
            if (wall.X < minX) minX = wall.X;
            if (wall.X > maxX) maxX = wall.X;

            // Compare and update minZ and maxZ
            if (wall.Z < minZ) minZ = wall.Z;
            if (wall.Z > maxZ) maxZ = wall.Z;
        }
    }

    private void FindNewOuterWalls()
    {
        // Find the min/max points
        FindMinMaxPoints();

        // Expand the rectangle by one tile in each direction
        minX -= 1;
        maxX += 1;
        minZ -= 1;
        maxZ += 1;

        // Initialize the list for flood-filled walls
        _newOuterWalls = new List<Tile>();

        // Start the flood fill from the bottom-left corner
        FloodFill(tiles[minZ, minX]);
    }
    // Using floodfill algorithm to search inside the rectnagle untill finding all the outer walls like covering with a rubber band
    private void FloodFill(Tile startTile)
    {
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);

        // Track visited tiles
        HashSet<Tile> visited = new HashSet<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            // Skip if this tile is outside the expanded rectangle bounds
            if (current.X < minX || current.X > maxX || current.Z < minZ || current.Z > maxZ)
                continue;

            // Skip if already visited
            if (visited.Contains(current))
                continue;

            visited.Add(current);

            if (current.Type == TileType.Empty)
            {
                // Add adjacent tiles to the stack for further exploration
                queue.Enqueue(tiles[current.Z + 1, current.X]);
                queue.Enqueue(tiles[current.Z - 1, current.X]);
                queue.Enqueue(tiles[current.Z, current.X + 1]);
                queue.Enqueue(tiles[current.Z, current.X - 1]);
                queue.Enqueue(tiles[current.Z + 1, current.X + 1]);
                queue.Enqueue(tiles[current.Z - 1, current.X - 1]);
                queue.Enqueue(tiles[current.Z - 1, current.X + 1]);
                queue.Enqueue(tiles[current.Z + 1, current.X - 1]);
            }
            else if (current.Type == TileType.Wall)
            {
                // If it's a wall, add it to the flood-filled walls list
                _newOuterWalls.Add(current);
            }
        }
    }

    private void BreakInnerWalls()
    {
        List<Tile> innerWalls = new List<Tile>();

        // Loop through the existing walls list
        foreach (Tile wall in walls)
        {
            // Check if the wall is not in the newOuterWalls list
            if (!_newOuterWalls.Contains(wall))
            {
                // This wall is an inner wall
                innerWalls.Add(wall);
            }
        }

        // Breaking inner walls
        foreach (Tile innerWall in innerWalls)
        {
            BreakWallFromTile(innerWall);
        }
        if(innerWalls.Count>0){
            FloodFillNewOccupied(innerWalls[0]);
        }
    }

    private void BreakWallFromTile(Tile tile)
    {
        Wall wall = tile.GetComponent<Wall>();
        if (wall.archerTower != null)
        {
            Destroy(wall.archerTower);
        }
        StartCoroutine(tile.LowerTile());
        Destroy(wall);
        tile.SetTileType(TileType.Empty);
    }

    private void FloodFillNewOccupied(Tile tile)
    {
        // Create a stack for flood fill (or a queue if you prefer BFS)
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(tile);

        // Track visited tiles
        HashSet<Tile> visited = new HashSet<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            // Skip if already visited
            if (visited.Contains(current))
                continue;

            visited.Add(current);

            if (current.Type == TileType.Empty)
            {
                current.SetTileType(TileType.Occupied);
                // Add adjacent tiles to the stack for further exploration
                queue.Enqueue(tiles[current.Z + 1, current.X]);
                queue.Enqueue(tiles[current.Z - 1, current.X]);
                queue.Enqueue(tiles[current.Z, current.X + 1]);
                queue.Enqueue(tiles[current.Z, current.X - 1]);
            }
        }
    }

    private void BuildNewArcherTowers()
    {   
        
        if(_selectedTiles.Count > 0)
        {
            for (int i = 0; i < _selectedTiles.Count; i++)
            {
                Tile currentTile = _selectedTiles[i];
                List<Tile> adjacentWalls = new List<Tile>();

                // Check adjacent tiles (left, right, up, down) using TryGetComponent and archerTower check
                if (tiles[currentTile.Z, currentTile.X - 1].TryGetComponent(out Wall leftWall) && leftWall.archerTower == null)
                    adjacentWalls.Add(tiles[currentTile.Z, currentTile.X - 1]);

                if (tiles[currentTile.Z, currentTile.X + 1].TryGetComponent(out Wall rightWall) && rightWall.archerTower == null)
                    adjacentWalls.Add(tiles[currentTile.Z, currentTile.X + 1]);
                    
                if (tiles[currentTile.Z - 1, currentTile.X].TryGetComponent(out Wall downWall) && downWall.archerTower == null)
                    adjacentWalls.Add(tiles[currentTile.Z - 1, currentTile.X]);
            
                if (tiles[currentTile.Z + 1, currentTile.X].TryGetComponent(out Wall upWall) && upWall.archerTower == null)
                    adjacentWalls.Add(tiles[currentTile.Z + 1, currentTile.X]);
                

                // Check if exactly 2 adjacent walls are found
                if (adjacentWalls.Count == 2)
                {
                    Vector3 direction1 = new Vector3(currentTile.transform.position.x - adjacentWalls[0].transform.position.x, 0, currentTile.transform.position.z - adjacentWalls[0].transform.position.z);
                    Vector3 direction2 = new Vector3(adjacentWalls[1].transform.position.x - currentTile.transform.position.x, 0, adjacentWalls[1].transform.position.z - currentTile.transform.position.z);

                    
                    // checks if the side walls have archer tower or not if at least one of them do go to next tile.
                    if(tiles[adjacentWalls[0].Z - Mathf.RoundToInt(direction1.z), adjacentWalls[0].X - Mathf.RoundToInt(direction1.x)].TryGetComponent<Wall>(out Wall otherWall)){
                        if(otherWall.archerTower!=null) continue;
                    }
                    if(tiles[adjacentWalls[1].Z + Mathf.RoundToInt(direction1.z), adjacentWalls[1].X + Mathf.RoundToInt(direction1.x)].TryGetComponent<Wall>(out Wall otherWall2)){
                        if(otherWall2.archerTower!=null) continue;
                    }

                    // Check if the directions are aligned in one line
                    if (Vector3.Dot(direction1.normalized, direction2.normalized) > 0.99f)
                    {
                        currentTile.GetComponent<Wall>().BuildArcherTower();
                    }
    
                }
        
            }
        }
        

    }

}
