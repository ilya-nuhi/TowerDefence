using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    [SerializeField] private TowerGuard guardPrefab;
    private Tile _startTile;
    private List<TowerGuard> _towerGuards;
    private int _guardsAtDestination = 0;

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
        _towerGuards = new List<TowerGuard>();
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

    private void SpawnTowerGuards(List<Tile> destTiles)
    {   
        _guardsAtDestination = 0;
        
        foreach(Tile destTile in destTiles){
            TowerGuard guard = Instantiate(guardPrefab, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);
            guard.OnReachedDestination += OnGuardReachedDestination;
            guard.SetDestination(destTile.transform);
            _towerGuards.Add(guard);
        }

        if (destTiles.Count==0)
        {
            StartCoroutine(FinishBuildingCoroutine());
        }
    }
    
    private void OnGuardReachedDestination(TowerGuard guard)
    {
        _guardsAtDestination++;
        guard.OnReachedDestination -= OnGuardReachedDestination;

        if (_guardsAtDestination == _towerGuards.Count)
        {
            StartCoroutine(FinishBuildingCoroutine());
        }
    }

    private void RetreatGuardsToBase(){
        for (int i = _towerGuards.Count-1; i >= 0; i--)
        {
            _towerGuards[i].OnReachedDestination += OnGuardReturnedToBase;
            _towerGuards[i].SetDestination(_startTile.transform);
            _towerGuards.Remove(_towerGuards[i]);
        }
    }
    private void OnGuardReturnedToBase(TowerGuard guard)
    {
        guard.OnReachedDestination -= OnGuardReturnedToBase;
        Destroy(guard.gameObject);
    }

    private IEnumerator FinishBuildingCoroutine(){
        // Wait 5 seconds for wall to build
        yield return new WaitForSeconds(5);
        EventManager.Instance.FinishBuildingWalls();
        RetreatGuardsToBase();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
        }
    }
}
