using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    [SerializeField] private int buildingTime = 5;
    
    private Tile _startTile;

    private void OnEnable()
    {
        EventManager.Instance.OnConstructWall += ConstructWall;
    }

    private void OnDisable()
    {
        if(EventManager.Instance!=null){
            EventManager.Instance.OnConstructWall -= ConstructWall;
        }
        
    }

    void Start()
    {
        StartCoroutine(DetectTileAfterFrame());
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

    private void ConstructWall(List<Tile> archerTiles, List<Tile> wallTiles)
    {
        // Spawning tower guards to protect the construction area
        List<Tile> wallTilesCopy = new List<Tile>(wallTiles);
        List<Tile> archerTilesCopy = new List<Tile>(archerTiles);
        StartCoroutine(BuildWallCoroutine(wallTilesCopy));
        StartCoroutine(SpawnGuardsRoutine(archerTilesCopy));
    }

    private IEnumerator SpawnGuardsRoutine(List<Tile> archerTiles)
    {
        List<TowerGuard> guards = new List<TowerGuard>();
        foreach (Tile destTile in archerTiles)
        {
            TowerGuard guard = ObjectPool.Instance.GetTowerGuard(_startTile.transform).GetComponent<TowerGuard>();
            guard.agent.enabled = false;
            yield return new WaitForSeconds(0.1f);
            guard.agent.enabled = true;
            guard.SetDutyDestination(destTile.transform);
            guards.Add(guard);
        }
        float retreatTime = Mathf.Clamp(buildingTime - 1, 1f, buildingTime);
        yield return new WaitForSeconds(retreatTime);
        RetreatGuardsToBase(guards);
    }
    

    private void RetreatGuardsToBase(List<TowerGuard> guards = null)
    {
        if (guards == null )return;
        foreach (var guard in guards)
        {
            if (guard.isActiveAndEnabled)
            {
                guard.OnReachedDestination += OnGuardReturnedToBase;
                guard.SetDutyDestination(_startTile.transform);
            }
        }
        
    }
    private void OnGuardReturnedToBase(TowerGuard guard)
    {
        guard.OnReachedDestination -= OnGuardReturnedToBase;
        // if guard has stopped an enemy, release the enemy before returning pool
        if (guard.targetEnemy != null) guard.targetEnemy.navAgent.isStopped = false;
        
        ObjectPool.Instance.ReturnTowerGuard(guard);
    }

    private IEnumerator BuildWallCoroutine(List<Tile> wallTiles){
        // Wait for wall to build
        yield return new WaitForSeconds(buildingTime);
        EventManager.Instance.FinishBuildingWalls(wallTiles);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>().HandleDestroy();
        }
    }
}
