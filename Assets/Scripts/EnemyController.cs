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

    private void Start()
    {
        _tiles = ResourceHolder.Instance.tiles;
        baseTower = GameObject.FindGameObjectWithTag("Base").transform;
    }

    void Update()
    {
        // wait until the navmesh is set
        if (!navAgent.isOnNavMesh) return;
        
        // Send a ray downward to check the tile below
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            Tile tileBelow = hit.collider.GetComponent<Tile>();
            // Enemy is in the occupied area set its destination to base tower
            if (tileBelow != null && tileBelow.Type == TileType.Occupied)
            {
                StartCoroutine(SetDestinationAfterDelay(baseTower.position));
                return;
            }
        }
        // Find the closest wall
        Transform closestWall = FindClosestWall();
        if (closestWall != null)
        {

            // Check for a gap between the closest wall and its neighbors
            if (IsGapBetweenWalls(closestWall))
            {
                Debug.Log("found gap!");
                // If there's a gap, move to the base tower
                StartCoroutine(SetDestinationAfterDelay(baseTower.position));
            }
            else
            {
                Debug.Log("moving closest wall!");
                // Otherwise, move to the closest wall
                navAgent.SetDestination(closestWall.position);
            }
        }
        else
        {
            Debug.Log("no walls are found");
            // If no walls are found, move to the base tower
            navAgent.SetDestination(baseTower.position);
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

    Debug.Log($"Starting gap check for wall at position: {wall.position}");

    while (queue.Count > 0)
    {
        Tile currentTile = queue.Dequeue();
        Debug.Log($"Checking tile at position: {currentTile.transform.position}");

        // Skip if already visited
        if (!visited.Add(currentTile))
        {
            Debug.Log($"Tile at position {currentTile.transform.position} already visited.");
            continue;
        }

        int unvisitedWallsCount = 0;
        bool hasGap = true;

        for (int i = 0; i < directionsZ.Length; i++)
        {
            int nextZ = currentTile.Z + directionsZ[i];
            int nextX = currentTile.X + directionsX[i];
            
            // Ensure the next tile is within bounds before accessing it
            if (nextZ < 0 || nextZ >= _tiles.GetLength(0) || nextX < 0 || nextX >= _tiles.GetLength(1))
            {
                Debug.Log($"Next tile at ({nextZ}, {nextX}) is out of bounds.");
                continue;
            }

            Tile nextTile = _tiles[nextZ, nextX];
            Debug.Log($"Evaluating adjacent tile at position: {nextTile.transform.position}");

            if (visited.Contains(nextTile))
            {
                Debug.Log($"Adjacent tile at position {nextTile.transform.position} already visited.");
                continue;
            }

            if (nextTile.Type == TileType.Wall)
            {
                Wall nextWall = nextTile.GetComponentInChildren<Wall>();
                if (!nextWall)
                {
                    Debug.Log($"Gap detected: No wall component found at adjacent tile {nextTile.transform.position}.");
                    return true;
                }

                hasGap = false;
                unvisitedWallsCount++;

                // Check distance from the original wall
                if (Vector3.Distance(wall.position, nextTile.transform.position) > detectionRadius / 2)
                {
                    Debug.Log($"Skipping tile at {nextTile.transform.position} as it is beyond half the detection radius.");
                    continue;
                }

                Debug.Log($"Enqueuing adjacent tile at {nextTile.transform.position} for further checks.");
                queue.Enqueue(nextTile);
            }
        }

        if (isFirstTile)
        {
            isFirstTile = false;
            // First tile has less than 2 unvisited adjacent walls. Considering it as a gap
            if (unvisitedWallsCount < 2)
            {
                Debug.Log($"Gap detected: First tile at {currentTile.transform.position} has less than 2 unvisited adjacent walls.");
                return true;
            }
        }

        if (hasGap)
        {
            Debug.Log($"Gap detected near tile at position {currentTile.transform.position}.");
            return true;
        }
    }

    Debug.Log("No gaps detected.");
    return false;
}


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Base"))
        {
            Destroy(gameObject);
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
