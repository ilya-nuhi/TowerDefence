using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform baseTower;  // Reference to the base tower
    public float detectionRadius = 10f;  // Detection radius for walls
    [SerializeField] private LayerMask wallLayer; // Layer mask to filter walls
    [SerializeField] public NavMeshAgent navAgent;  // Reference to the enemy's NavMeshAgent
    private Tile[,] _tiles;
    [SerializeField] private float damagePerSecond = 10f; // Damage dealt per second
    private Coroutine _checkingCoroutine; // Store reference to the coroutine
    private bool _startCalled = false;
    private Vector3 _currentDestination;
    private void OnEnable()
    {
        // resetting current destination in order setdestinationifchanged to work
        _currentDestination = Vector3.zero;
        if (_startCalled)
        {
            // Wait until Start is called before doing anything
            // Start the CheckingCoroutine only after Start has been called
            _checkingCoroutine = StartCoroutine(CheckingCoroutine());
        }

        SetStats();
    }

    private void SetStats()
    {
        damagePerSecond = EnemyStatsManager.Damage;
        GetComponent<Health>().SetMaxHealth(EnemyStatsManager.Health);
        navAgent.speed = EnemyStatsManager.Speed;
    }
    
    private void Update()
    {
        // Detect spacebar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetStats();
        }
    }

    // Called when the GameObject is disabled
    private void OnDisable()
    {
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
        // Wait until the navmesh is set
        if (!navAgent.isOnNavMesh) 
        {
            //Debug.LogWarning($"{gameObject.name} is not on the NavMesh.");
            return;
        }
        

        // Send a ray downward to check the tile below
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            Tile tileBelow = hit.collider.GetComponent<Tile>();

            // Enemy is in the occupied area set its destination to base tower
            if (tileBelow != null && tileBelow.Type == TileType.Occupied)
            {
                SetDestinationIfChanged(baseTower.position);  // Compare and set destination
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
                SetDestinationIfChanged(baseTower.position);
            }
            else
            {
                SetDestinationIfChanged(closestWall.position);
            }
        }
        else
        {
            SetDestinationIfChanged(baseTower.position);
        }
    }


    // Helper method to check and update destination only if it changed to avoid unnecessary path calculation
    private void SetDestinationIfChanged(Vector3 newDestination)
    {
        if (_currentDestination != newDestination)
        {
            _currentDestination = newDestination;  // Update stored destination
            navAgent.SetDestination(newDestination);  // Set the new destination
        }
        
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

        int[] directionsZ = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] directionsX = { 1, 0, -1, 0, -1, 1, -1, 1 };
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
            DetectAndDamage();
            yield return new WaitForSeconds(1);
        }
    }
    
    void DetectAndDamage()
    {
        Collider[] results = new Collider[15];
        LayerMask layerMask = LayerMask.GetMask("Wall","Guard");
        var size = Physics.OverlapSphereNonAlloc(transform.position, 0.75f, results, layerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < size; i++)
        {
            Collider currentCollider = results[i];
            
            Health health = currentCollider.GetComponent<Health>();
            if (health != null && health.isActiveAndEnabled)
            {
                health.TakeDamage(damagePerSecond);
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
