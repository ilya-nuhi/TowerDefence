using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform baseTower;  // Reference to the base tower
    public float detectionRadius = 10f;  // Detection radius for walls
    [SerializeField] private LayerMask wallLayer; // Layer mask to filter walls
    [SerializeField] private NavMeshAgent navAgent;  // Reference to the enemy's NavMeshAgent
    private Tile[,] _tiles;
    [SerializeField] private float damagePerSecond = 10f; // Damage dealt per second to walls
    private Coroutine _checkingCoroutine; // Store reference to the coroutine
    private bool _startCalled = false;
    private void OnEnable()
    {
        
        if (_startCalled)
        {
            // Wait until Start is called before doing anything
            // Start the CheckingCoroutine only after Start has been called
            _checkingCoroutine = StartCoroutine(CheckingCoroutine());
        }
    }

    // Called when the GameObject is disabled
    private void OnDisable()
    {
        // Stop the CheckingCoroutine when the enemy is disabled
        if (_checkingCoroutine != null)
        {
            StopCoroutine(_checkingCoroutine);
        }
        
    }
    
    private void Start()
    {
        _tiles = ResourceHolder.Instance.tiles;
        baseTower = GameObject.FindGameObjectWithTag("Base").transform;
        StartCoroutine(CheckingCoroutine());
        _startCalled = true;
    }

    private void UpdateDestination()
{
    // wait until the navmesh is set
    if (!navAgent.isOnNavMesh) 
    {
        Debug.LogWarning($"{gameObject.name} is not on the NavMesh.");
        return;
    }

    // Send a ray downward to check the tile below
    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
    {
        Tile tileBelow = hit.collider.GetComponent<Tile>();
        // Enemy is in the occupied area set its destination to base tower
        if (tileBelow != null && tileBelow.Type == TileType.Occupied)
        {
            _checkingCoroutine = StartCoroutine(SetDestinationAfterDelay(baseTower.position));
            return;
        }
    }

    // Find the closest wall
    Transform closestWall = FindClosestWall();
    if (closestWall != null)
    {
        //Check for a gap between the closest wall and its neighbors
        if (IsGapBetweenWalls(closestWall))
        {
            // If there's a gap, move to the base tower
            StartCoroutine(SetDestinationAfterDelay(baseTower.position));
        }
        else
        {
            // Otherwise, move to the closest wall
            navAgent.SetDestination(closestWall.position);
        }
    }
    else
    {
        // If no walls are found, move to the closest edge
        if (navAgent.FindClosestEdge(out var closestEdge))
        {
            navAgent.SetDestination(closestEdge.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} couldn't find any edges to move to.");
        }
    }
}

    
    IEnumerator SetDestinationAfterDelay(Vector3 destination)
    {
        // wait 1 second to navmesh to update for walls
        yield return new WaitForSeconds(1);
        navAgent.SetDestination(destination);
    }
    
    Transform FindClosestWall()
    {
        Collider[] hitColliders = new Collider[100];
        var size = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders, wallLayer);
        Transform closestWall = null;
        float minDistance = Mathf.Infinity;
        
        for (int i = 0; i < size; i++)
        {
            Collider currentWall = hitColliders[i];
            float distanceToWall = Vector3.Distance(transform.position, currentWall.transform.position);
            if (distanceToWall < minDistance)
            {
                minDistance = distanceToWall;
                closestWall = currentWall.transform;
            }
        }
        return closestWall;
    }

    bool IsGapBetweenWalls(Transform wall)
    {
        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        Tile tile = wall.gameObject.GetComponentInParent<Tile>();
        queue.Enqueue(tile);

        int[] directionsZ = { 0, 1, 0, -1 };
        int[] directionsX = { 1, 0, -1, 0 };
        bool isFirstTile = true;

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();

            // Skip if already visited
            if (!visited.Add(currentTile)) { continue; }

            int unvisitedWallsCount = 0;
            bool hasGap = true;

            for (int i = 0; i < directionsZ.Length; i++)
            {
                int nextZ = currentTile.Z + directionsZ[i];
                int nextX = currentTile.X + directionsX[i];
                
                // Ensure the next tile is within bounds before accessing it
                if (nextZ < 0 || nextZ >= _tiles.GetLength(0) || nextX < 0 || nextX >= _tiles.GetLength(1)) 
                { continue; }

                Tile nextTile = _tiles[nextZ, nextX];

                if (visited.Contains(nextTile)) { continue; }

                if (nextTile.Type == TileType.Wall)
                {
                    Wall nextWall = nextTile.GetComponentInChildren<Wall>();
                    if (!nextWall) { return true; }

                    hasGap = false;
                    unvisitedWallsCount++;

                    // Check distance from the original wall
                    if (Vector3.Distance(wall.position, nextTile.transform.position) > detectionRadius / 2) { continue; }
                    
                    queue.Enqueue(nextTile);
                }
            }

            if (isFirstTile)
            {
                isFirstTile = false;
                // First tile has less than 2 unvisited adjacent walls. Considering it as a gap
                if (unvisitedWallsCount < 2)
                {
                    return true;
                }
            }

            if (hasGap)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator CheckingCoroutine()
    {
        while (true)
        {
            UpdateDestination();
            DetectAndDamageWalls();
            yield return new WaitForSeconds(1);
        }
    }
    
    void DetectAndDamageWalls()
    {
        Collider[] results = new Collider[8];
        var size = Physics.OverlapSphereNonAlloc(transform.position, 0.75f, results, wallLayer);

        for (int i = 0; i < size; i++)
        {
            Collider currentCollider = results[i];
            
            Health wallHealth = currentCollider.GetComponent<Health>();
            if (wallHealth != null)
            {
                wallHealth.TakeDamage(damagePerSecond);
            }
            
        }
    }

    void OnDrawGizmos()
    {
        // Set the color of the gizmo
        Gizmos.color = Color.red;

        // Draw a wireframe sphere at the enemy's position with the detection radius
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
