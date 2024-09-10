using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    [SerializeField] private int buildingTime = 5;
    
    private Tile _startTile;
    private Dictionary<List<TowerGuard>, List<Tile>> _guardsAndWallsList;
    private Dictionary<List<TowerGuard>, int> _guardsReachedDict; // Track reached guards per batch

    private void OnEnable()
    {
        EventManager.Instance.OnSpawnTowerGuards += SpawnTowerGuards;
    }

    private void OnDisable()
    {
        if(EventManager.Instance!=null){
            EventManager.Instance.OnSpawnTowerGuards -= SpawnTowerGuards;
        }
        
    }

    void Start()
    {
        StartCoroutine(DetectTileAfterFrame());
        _guardsAndWallsList = new Dictionary<List<TowerGuard>, List<Tile>>();
        _guardsReachedDict = new Dictionary<List<TowerGuard>, int>();
    }

    private IEnumerator DetectTileAfterFrame()
    {
        // Wait for one frame for map to construct
        yield return null;

        DetectTileBelowSpawner();
    }

    void DetectTileBelowSpawner()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Assuming the tile has a component called Tile
            _startTile = hit.collider.GetComponent<Tile>();
            if (_startTile == null)
            {
                Debug.Log("No tile detected at the spawner's position.");
            }
        }
    }

    private void SpawnTowerGuards(List<Tile> destTiles, List<Tile> wallTiles)
    {
        if (destTiles.Count == 0)
        {
            StartCoroutine(FinishBuildingCoroutine(new KeyValuePair<List<TowerGuard>, List<Tile>>(null, wallTiles)));
            return;
        }

        List<TowerGuard> guards = new List<TowerGuard>();
        foreach (Tile destTile in destTiles)
        {
            TowerGuard guard = ObjectPool.Instance.GetTowerGuard(_startTile.transform).GetComponent<TowerGuard>();
            guard.OnReachedDestination += OnGuardReachedDestination;
            guard.SetDestination(destTile.transform);
            guards.Add(guard);
        }
        // Create a copy of the wallTiles list
        List<Tile> wallTilesCopy = new List<Tile>(wallTiles);
        // Track this batch of guards
        _guardsAndWallsList[guards] = wallTilesCopy;
        _guardsReachedDict[guards] = 0; // Initialize the count of reached guards for this batch
    }
    
    private void OnGuardReachedDestination(TowerGuard guard)
    {
        // Find which batch the guard belongs to
        foreach (var guardsBatch in _guardsAndWallsList)
        {
            if (guardsBatch.Key.Contains(guard))
            {
                _guardsReachedDict[guardsBatch.Key]++; // Increment the reached count for this batch

                // Check if all guards in this batch have reached their destination
                if (_guardsReachedDict[guardsBatch.Key] == guardsBatch.Key.Count)
                {
                    StartCoroutine(FinishBuildingCoroutine(guardsBatch));
                    _guardsAndWallsList.Remove(guardsBatch.Key); // Remove this batch from the dict
                    _guardsReachedDict.Remove(guardsBatch.Key); // Remove tracking for this batch
                }

                break;
            }
        }

        guard.OnReachedDestination -= OnGuardReachedDestination; // Unsubscribe from the event
    }

    private void RetreatGuardsToBase(List<TowerGuard> guards = null)
    {
        if (guards == null)return;
        foreach (var guard in guards)
        {
            guard.OnReachedDestination += OnGuardReturnedToBase;
            guard.SetDestination(_startTile.transform);
        }
        
    }
    private void OnGuardReturnedToBase(TowerGuard guard)
    {
        guard.OnReachedDestination -= OnGuardReturnedToBase;
        ObjectPool.Instance.ReturnTowerGuard(guard.gameObject);
    }

    private IEnumerator FinishBuildingCoroutine(KeyValuePair<List<TowerGuard>, List<Tile>>? guardsBatch = null){
        // Wait 5 seconds for wall to build
        yield return new WaitForSeconds(buildingTime);
        EventManager.Instance.FinishBuildingWalls(guardsBatch?.Value);
        RetreatGuardsToBase(guardsBatch?.Key);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ObjectPool.Instance.ReturnEnemy(other.gameObject);
        }
    }
}
