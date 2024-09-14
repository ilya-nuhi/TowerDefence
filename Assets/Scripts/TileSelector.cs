using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private GameObject selectionBoxPrefab;
    [SerializeField] private GameObject invisBoxPrefab;
    private GameObject _startObj;
    private Tile[,] _tiles;
    private List<Tile> _wallTiles;
    private List<GameObject> _selectedWalls;
    private List<GameObject> _invisWalls;
    private List<Tile> _selectedTiles;
    private Tile _lastSelectedTile;
    private Tile _firstSelectedTile;
    private List<Tile> _newOuterWalls;
    private bool _isSelecting;
    private bool _canExpand = true;

    // Boundaries of smallest rectangle that can cover our walls
    private int _minX, _maxX, _minZ, _maxZ;

    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Click.performed += StartSelection;
        _inputActions.Player.Click.canceled += EndSelection;
        _inputActions.Player.Select.performed += UpdateSelectedWalls; // Update the box during drag
        EventManager.Instance.OnFinishBuildingWalls += FinishBuildingWalls;
    }

    private void OnDisable()
    {
        _inputActions.Player.Click.performed -= StartSelection;
        _inputActions.Player.Click.canceled -= EndSelection;
        _inputActions.Player.Select.performed -= UpdateSelectedWalls;
        _inputActions.Disable();
        if(EventManager.Instance!=null){
            EventManager.Instance.OnFinishBuildingWalls -= FinishBuildingWalls;
        }
    }

    private void Start() {
        _tiles = ResourceHolder.Instance.tiles;
        _wallTiles = ResourceHolder.Instance.wallTiles;
    }

    private void StartSelection(InputAction.CallbackContext context)
    {
        if(!_canExpand) {return;}
        _startObj = GetMouseHitObject();
        // if object has no tile component it should stop checking its type too
        if(_startObj.TryGetComponent(out Tile selectedTile) && selectedTile.Type == TileType.Wall){
            _selectedWalls = new List<GameObject>();
            _invisWalls = new List<GameObject>();
            _selectedTiles = new List<Tile>();
            GameObject selectionWall = ObjectPool.Instance.GetSelectionBox(selectedTile.transform.position);
            _selectedWalls.Add(selectionWall);
            _lastSelectedTile = selectedTile;
            _firstSelectedTile = selectedTile;
            _isSelecting = true;
            _canExpand = false;
        }
    }

    private void EndSelection(InputAction.CallbackContext context)
    {
        if (!_isSelecting) return;
        _isSelecting = false;
        _canExpand = true;

        if (_firstSelectedTile.Type == TileType.Wall)
        {   
            GameObject nextTile = GetMouseHitObject();

            // If the wall selection is valid expand.
            if(nextTile.TryGetComponent<Tile>(out Tile selectedTile) && selectedTile.Type == TileType.Wall && CheckAdjacencyOfTiles(_lastSelectedTile, selectedTile)){
                foreach(Tile tile in _selectedTiles){
                    if(tile.Type == TileType.Empty){
                        tile.SetTileType(TileType.Wall);
                        tile.GetComponentInChildren<Wall>().gameObject.SetActive(false);
                        tile.GetComponent<MeshRenderer>().material = ResourceHolder.Instance.constructMaterial;
                        _wallTiles.Add(tile);
                    }
                }
                FindNewOuterWalls();
                BreakInnerWalls();
                BuildNewArcherTowers();
                _wallTiles.Clear();
                _wallTiles.AddRange(_newOuterWalls);
            }
        }
        
        // Destroy all GameObjects in _selectedWalls
        foreach (GameObject selectionWall in _selectedWalls)
        {
            ObjectPool.Instance.ReturnSelectionBox(selectionWall);
        }

        foreach (var invisWall in _invisWalls)
        {
            ObjectPool.Instance.ReturnInvisBox(invisWall);
        }
        
        // Clear the references for selected walls
        _selectedWalls.Clear();
        _invisWalls.Clear();
        
    }

    private void UpdateSelectedWalls(InputAction.CallbackContext context){
        if (!_isSelecting) return;
        GameObject nextTile = GetMouseHitObject();
        if(nextTile.TryGetComponent(out Tile selectedTile) && selectedTile.Type == TileType.Empty && CheckAdjacencyOfTiles(_lastSelectedTile, selectedTile)){
            GameObject selectionWall = ObjectPool.Instance.GetSelectionBox(selectedTile.transform.position);

            // Calculate the direction from the last selected tile to the new tile
            Vector3 direction = new Vector3(_lastSelectedTile.X - selectedTile.X , 0, _lastSelectedTile.Z - selectedTile.Z);
            // Calculate the left and right directions (perpendicular to the direction)
            Vector3 leftDirection = new Vector3(-direction.z, 0, direction.x);
            Vector3 rightDirection = new Vector3(direction.z, 0, -direction.x);
            // Invisible walls are to prevent building walls next to each other
            GameObject invisWallLeft = ObjectPool.Instance.GetInvisBox(
                new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) +
                leftDirection);
            GameObject invisWallRight = ObjectPool.Instance.GetInvisBox(
                new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) +
                rightDirection);
            GameObject invisWallBack = ObjectPool.Instance.GetInvisBox(
                new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) +
                direction);
            GameObject invisWallBackRight = ObjectPool.Instance.GetInvisBox(
                new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) +
                direction);
            GameObject invisWallBackLeft = ObjectPool.Instance.GetInvisBox(
                new Vector3(_lastSelectedTile.transform.position.x, 1, _lastSelectedTile.transform.position.z) +
                direction);
            
            _selectedWalls.Add(selectionWall);
            _invisWalls.Add(invisWallLeft);
            _invisWalls.Add(invisWallRight);
            _invisWalls.Add(invisWallBack);
            _invisWalls.Add(invisWallBackRight);
            _invisWalls.Add(invisWallBackLeft);

            _selectedTiles.Add(selectedTile);
            _lastSelectedTile = selectedTile;
        }
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
        Vector2 mousePosition = _inputActions.Player.Select.ReadValue<Vector2>();
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        
            int tileLayerMask = LayerMask.GetMask("Tile", "MouseBlocker");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayerMask))
            {
                return hit.collider.gameObject;
            }
        }

        return null;
    }

    private void FindMinMaxPoints()
    {
        // Initialize min and max values
        _minX = int.MaxValue;
        _maxX = int.MinValue;
        _minZ = int.MaxValue;
        _maxZ = int.MinValue;

        // Iterate over the walls list
        foreach (Tile wall in _wallTiles)
        {
            // Compare and update minX and maxX
            if (wall.X < _minX) _minX = wall.X;
            if (wall.X > _maxX) _maxX = wall.X;

            // Compare and update minZ and maxZ
            if (wall.Z < _minZ) _minZ = wall.Z;
            if (wall.Z > _maxZ) _maxZ = wall.Z;
        }
    }

    private void FindNewOuterWalls()
    {
        // Find the min/max points
        FindMinMaxPoints();

        // Expand the rectangle by one tile in each direction
        _minX -= 1;
        _maxX += 1;
        _minZ -= 1;
        _maxZ += 1;

        // Initialize the list for flood-filled walls
        _newOuterWalls = new List<Tile>();

        // Start the flood fill from the bottom-left corner
        FloodFill(_tiles[_minZ, _minX]);
    }
    
    // Using floodfill algorithm to search inside the rectangle until finding all the outer walls like covering with a rubber band
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
            if (current.X < _minX || current.X > _maxX || current.Z < _minZ || current.Z > _maxZ)
                continue;

            // Skip if already visited
            if (!visited.Add(current))
                continue;

            if (current.Type == TileType.Empty)
            {
                // Add adjacent tiles to the stack for further exploration
                queue.Enqueue(_tiles[current.Z + 1, current.X]);
                queue.Enqueue(_tiles[current.Z - 1, current.X]);
                queue.Enqueue(_tiles[current.Z, current.X + 1]);
                queue.Enqueue(_tiles[current.Z, current.X - 1]);
                queue.Enqueue(_tiles[current.Z + 1, current.X + 1]);
                queue.Enqueue(_tiles[current.Z - 1, current.X - 1]);
                queue.Enqueue(_tiles[current.Z - 1, current.X + 1]);
                queue.Enqueue(_tiles[current.Z + 1, current.X - 1]);
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
        List<Tile> innerWallTiles = new List<Tile>();

        // Loop through the existing walls list
        foreach (Tile wallTile in _wallTiles)
        {
            // Check if the wall is not in the newOuterWalls list
            if (!_newOuterWalls.Contains(wallTile))
            {
                // This wall is an inner wall
                innerWallTiles.Add(wallTile);
            }
        }

        // Breaking inner walls
        foreach (Tile innerWallTile in innerWallTiles)
        {
            BreakWallFromTile(innerWallTile);
        }
        if(innerWallTiles.Count>0){
            FloodFillNewOccupied(innerWallTiles);
        }
        else
        {
            FindInnerEmptyTilesAndOccupy();
        }
    }

    private void BreakWallFromTile(Tile tile)
    {
        Wall wall = tile.GetComponentInChildren<Wall>(true);
        wall.gameObject.SetActive(true);
        if (!wall)
        {
            return;
        }
        wall.DestroyWall();
        // removing the tile from selected tiles if it exists 
        _selectedTiles.Remove(tile);
    }

    private void FloodFillNewOccupied(List<Tile> tiles)
    {
        // Create a stack for flood fill (or a queue if you prefer BFS)
        Queue<Tile> queue = new Queue<Tile>();
        foreach (Tile tile in tiles) queue.Enqueue(tile);

        // Track visited tiles
        HashSet<Tile> visited = new HashSet<Tile>();

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            // Skip if already visited
            if (!visited.Add(current))
                continue;

            if (current.Type == TileType.Empty)
            {
                current.SetTileType(TileType.Occupied);
                // Add adjacent tiles to the stack for further exploration
                queue.Enqueue(_tiles[current.Z + 1, current.X]);
                queue.Enqueue(_tiles[current.Z - 1, current.X]);
                queue.Enqueue(_tiles[current.Z, current.X + 1]);
                queue.Enqueue(_tiles[current.Z, current.X - 1]);
            }
        }
    }

    // if there is no inner wall breaked while building new walls, then find the new occupied tiles with using selectedtiles
    private void FindInnerEmptyTilesAndOccupy()
    {
        int[] directionsZ = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] directionsX = { 1, 0, -1, 0, 1, -1, 1, -1 };
        HashSet<Tile> innerEmptyTiles = new HashSet<Tile>();
        HashSet<Tile> visitedTiles = new HashSet<Tile>();

        // Loop through each selected tile
        foreach (var selectedTile in _selectedTiles)
        {
            List<Tile> possibleTiles = new List<Tile>();

            // Check the surrounding tiles (8 directions)
            for (int i = 0; i < 8; i++)
            {
                var searchedTile = _tiles[selectedTile.Z + directionsZ[i], selectedTile.X + directionsX[i]];
                if (!visitedTiles.Add(searchedTile)) { continue; }
                if (searchedTile.Type != TileType.Empty) { continue; }

                // Check surrounding tiles of the neighbor to see if it is adjacent to an occupied tile
                for (int j = 0; j < 8; j++)
                {
                    Tile adjacentTile = _tiles[searchedTile.Z + directionsZ[j], searchedTile.X + directionsX[j]];
                    if (adjacentTile.Type == TileType.Occupied)
                    {
                        possibleTiles.Add(searchedTile);
                        break;
                    }
                }
            }

            // If more than one possible tile is found, find the closest one to the last selected tile
            if (possibleTiles.Count > 1)
            {
                float minDistance = int.MaxValue;
                Tile minTile = null;

                foreach (var tile in possibleTiles)
                {
                    float distance = Vector3.Distance(tile.transform.position, _selectedTiles[^1].transform.position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minTile = tile;
                    }
                }
                
                innerEmptyTiles.Add(minTile);
            }
            else if (possibleTiles.Count == 1)
            {
                innerEmptyTiles.Add(possibleTiles[0]);
            }
        }

        // Call flood fill function with inner empty tiles
        FloodFillNewOccupied(innerEmptyTiles.ToList());
    }


    private void BuildNewArcherTowers()
    {   
        int[] directionsZ = { 0, 1, 0, -1 };
        int[] directionsX = { 1, 0, -1, 0 };
        
        if(_selectedTiles.Count > 0)
        {
            List<Tile> archerTiles = new List<Tile>();
            for (int i = 0; i < _selectedTiles.Count; i++)
            {
                Tile currentTile = _selectedTiles[i];
                List<Tile> adjacentWallTiles = new List<Tile>();

                // Check adjacent tiles (left, right, up, down) using TryGetComponent and archerTower check
                for (int j = 0; j < directionsZ.Length; j++)
                {
                    Wall currentWall = _tiles[currentTile.Z + directionsZ[j], currentTile.X + directionsX[j]].GetComponentInChildren<Wall>(true);
                    if (currentWall!=null && currentWall.archerTower == null)
                        adjacentWallTiles.Add(_tiles[currentTile.Z + directionsZ[j], currentTile.X + directionsX[j]]);
                }

                // Check if exactly 2 adjacent walls are found
                if (adjacentWallTiles.Count == 2)
                {
                    Vector3 direction1 = new Vector3(currentTile.transform.position.x - adjacentWallTiles[0].transform.position.x,
                        0, currentTile.transform.position.z - adjacentWallTiles[0].transform.position.z);
                    Vector3 direction2 = new Vector3(adjacentWallTiles[1].transform.position.x - currentTile.transform.position.x,
                        0, adjacentWallTiles[1].transform.position.z - currentTile.transform.position.z);
                    
                    // checks if the side walls have archer tower or not if at least one of them do go to next tile.
                    Wall otherWall = _tiles[adjacentWallTiles[0].Z - Mathf.RoundToInt(direction1.z),
                            adjacentWallTiles[0].X - Mathf.RoundToInt(direction1.x)].GetComponentInChildren<Wall>(true);
                    if(otherWall!=null && otherWall.archerTower != null){ continue;}

                    otherWall = _tiles[adjacentWallTiles[1].Z + Mathf.RoundToInt(direction1.z),
                        adjacentWallTiles[1].X + Mathf.RoundToInt(direction1.x)].GetComponentInChildren<Wall>(true);
                    if(otherWall!=null && otherWall.archerTower != null){ continue;}

                    // Check if the directions are aligned in one line
                    if (Vector3.Dot(direction1.normalized, direction2.normalized) > 0.99f)
                    {
                        Wall archerWall = currentTile.GetComponentInChildren<Wall>(true);
                        archerWall.BuildArcherTower();
                        archerWall.archerTower.gameObject.SetActive(false);
                        archerTiles.Add(currentTile);
                    }
    
                }
        
            }

            // After setting walls spawn tower guards to position on the archertiles
            EventManager.Instance.ConstructWall(archerTiles, _selectedTiles);

        }
        
    }

    private void FinishBuildingWalls(List<Tile> wallTiles){
        foreach(Tile currentTile in wallTiles){
            Wall currentWall = currentTile.GetComponentInChildren<Wall>(true);
            if(currentWall!=null){
                currentWall.gameObject.SetActive(true);
                currentWall.RiseWall();
                currentTile.GetComponent<MeshRenderer>().material = ResourceHolder.Instance.occupiedMaterial;
                if (currentWall.archerTower != null)
                {
                    currentWall.archerTower.gameObject.SetActive(true); 
                }
                
            }
        }
        _canExpand = true;
    }

}
