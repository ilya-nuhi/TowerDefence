using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private MapCreator mapCreator;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private GameObject selectionBoxPrefab;
    [SerializeField] private GameObject invisBoxPrefab;
    private GameObject _startObj;
    private Tile[,] _tiles;
    private List<Tile> _walls;
    private List<GameObject> _selectedWalls;
    private List<Tile> _selectedTiles;
    private Tile _lastSelectedTile;
    private List<Tile> _newOuterWalls;
    private bool _isSelecting = false;
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
        _tiles = mapCreator.tiles;
        _walls = mapCreator.walls;
    }

    private void StartSelection(InputAction.CallbackContext context)
    {
        if(!_canExpand) {return;}
        _startObj = GetMouseHitObject();
        // if object has no tile component it should stop checking its type too
        if(_startObj.TryGetComponent(out Tile selectedTile) && selectedTile.Type == TileType.Wall){
            _selectedWalls = new List<GameObject>();
            _selectedTiles = new List<Tile>();
            GameObject selectionWall = Instantiate(selectionBoxPrefab, new Vector3(_startObj.transform.position.x, 1, _startObj.transform.position.z), Quaternion.identity);
            _selectedWalls.Add(selectionWall);
            _lastSelectedTile = selectedTile;
            _isSelecting = true;
            _canExpand = false;
        }
    }

    private void EndSelection(InputAction.CallbackContext context)
    {
        if (!_isSelecting) return;
        _isSelecting = false;
        GameObject nextTile = GetMouseHitObject();

        // If the wall selection is valid expand.
        if(nextTile.TryGetComponent<Tile>(out Tile selectedTile) && selectedTile.Type == TileType.Wall && CheckAdjacencyOfTiles(_lastSelectedTile, selectedTile)){
            foreach(Tile tile in _selectedTiles){
                if(tile.Type == TileType.Empty){
                    tile.SetTileType(TileType.Wall);
                    tile.GetComponent<MeshRenderer>().material = ResourceHolder.Instance.constructMaterial;
                    _walls.Add(tile);
                }
            }
            FindNewOuterWalls();
            BreakInnerWalls();
            BuildNewArcherTowers();
            _walls = new List<Tile>(_newOuterWalls);
            EventManager.Instance.UpdateNavMesh();
        }
        else{
            _canExpand = true;
        }

        // Destroy all GameObjects in _selectedWalls
        foreach (GameObject selectionWall in _selectedWalls)
        {
            Destroy(selectionWall);
        }
        // Clear the references for selected walls
        _selectedWalls.Clear();
        
    }

    private void UpdateSelectedWalls(InputAction.CallbackContext context){
        if (!_isSelecting) return;
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
        Vector2 mousePosition = _inputActions.Player.Select.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            return hit.collider.gameObject;
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
        foreach (Tile wall in _walls)
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
        List<Tile> innerWalls = new List<Tile>();

        // Loop through the existing walls list
        foreach (Tile wall in _walls)
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
        tile.LowerTile();
        Destroy(wall);
        // removing the tile from selected tiles if it exists 
        _selectedTiles.Remove(tile);
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
                queue.Enqueue(_tiles[current.Z + 1, current.X]);
                queue.Enqueue(_tiles[current.Z - 1, current.X]);
                queue.Enqueue(_tiles[current.Z, current.X + 1]);
                queue.Enqueue(_tiles[current.Z, current.X - 1]);
            }
        }
    }

    private void BuildNewArcherTowers()
    {   
        
        if(_selectedTiles.Count > 0)
        {
            List<Tile> archerTiles = new List<Tile>();
            for (int i = 0; i < _selectedTiles.Count; i++)
            {
                Tile currentTile = _selectedTiles[i];
                List<Tile> adjacentWalls = new List<Tile>();

                // Check adjacent tiles (left, right, up, down) using TryGetComponent and archerTower check
                if (_tiles[currentTile.Z, currentTile.X - 1].TryGetComponent(out Wall leftWall) && leftWall.archerTower == null)
                    adjacentWalls.Add(_tiles[currentTile.Z, currentTile.X - 1]);

                if (_tiles[currentTile.Z, currentTile.X + 1].TryGetComponent(out Wall rightWall) && rightWall.archerTower == null)
                    adjacentWalls.Add(_tiles[currentTile.Z, currentTile.X + 1]);
                    
                if (_tiles[currentTile.Z - 1, currentTile.X].TryGetComponent(out Wall downWall) && downWall.archerTower == null)
                    adjacentWalls.Add(_tiles[currentTile.Z - 1, currentTile.X]);
            
                if (_tiles[currentTile.Z + 1, currentTile.X].TryGetComponent(out Wall upWall) && upWall.archerTower == null)
                    adjacentWalls.Add(_tiles[currentTile.Z + 1, currentTile.X]);
                

                // Check if exactly 2 adjacent walls are found
                if (adjacentWalls.Count == 2)
                {
                    Vector3 direction1 = new Vector3(currentTile.transform.position.x - adjacentWalls[0].transform.position.x, 0, currentTile.transform.position.z - adjacentWalls[0].transform.position.z);
                    Vector3 direction2 = new Vector3(adjacentWalls[1].transform.position.x - currentTile.transform.position.x, 0, adjacentWalls[1].transform.position.z - currentTile.transform.position.z);

                    
                    // checks if the side walls have archer tower or not if at least one of them do go to next tile.
                    if(_tiles[adjacentWalls[0].Z - Mathf.RoundToInt(direction1.z), adjacentWalls[0].X - Mathf.RoundToInt(direction1.x)].TryGetComponent<Wall>(out Wall otherWall)){
                        if(otherWall.archerTower!=null) continue;
                    }
                    if(_tiles[adjacentWalls[1].Z + Mathf.RoundToInt(direction1.z), adjacentWalls[1].X + Mathf.RoundToInt(direction1.x)].TryGetComponent<Wall>(out Wall otherWall2)){
                        if(otherWall2.archerTower!=null) continue;
                    }

                    // Check if the directions are aligned in one line
                    if (Vector3.Dot(direction1.normalized, direction2.normalized) > 0.99f)
                    {
                        Wall currentWall = currentTile.GetComponent<Wall>();
                        currentWall.BuildArcherTower();
                        currentWall.archerTower.SetActive(false);
                        archerTiles.Add(currentTile);
                    }
    
                }
        
            }

            // After setting walls spawn tower guards to position on the archertiles
            EventManager.Instance.SpawnTowerGuards(archerTiles);

        }
        
    }

    public void FinishBuildingWalls(){
        foreach(Tile currentTile in _selectedTiles){
            if(currentTile.TryGetComponent(out Wall currentWall)){
                currentTile.RiseTile();
                currentTile.GetComponent<MeshRenderer>().material = ResourceHolder.Instance.wallMaterial;
                if (currentWall.archerTower != null)
                {
                    currentWall.archerTower.SetActive(true); 
                }
                
            }
        }
        _selectedTiles.Clear();
        EventManager.Instance.UpdateNavMesh();
        _canExpand = true;
    }

}
