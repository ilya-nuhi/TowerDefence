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
    private bool _isBuilding = false;
    private bool _buildOrder = false;

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

    private void Update() {
        if(_towerGuards.Count>0){
            // if all tower guards are reached to destination position, then building of wall should start
            if(!_isBuilding){
                bool allReached = true;
                foreach (var towerGuard in _towerGuards.Where(towerGuard => !towerGuard.navigation.isReachedDestination))
                {
                    allReached = false;
                }

                if(allReached){
                    _isBuilding = true;
                    // setting build order to false in order to prevent it calling coroutine again after it executed
                    _buildOrder = false;
                    StartCoroutine(FinishBuildingCoroutine());
                }
            }
            // if the towerguards returned to spawner they should be destroyed and removed from the guards list.
            if(_towerGuards[0].navigation.destination == _startTile.transform){
                for (int i = _towerGuards.Count - 1; i >= 0; i--)
                {
                    TowerGuard towerGuard = _towerGuards[i];
                    if (towerGuard.navigation.isReachedDestination)
                    {
                        _towerGuards.RemoveAt(i);
                        Destroy(towerGuard.gameObject);
                    }
                }
            }
        }
        else{
            if(_buildOrder){
                _buildOrder = false;
                _isBuilding = true;
                StartCoroutine(FinishBuildingCoroutine());
        
            }
        }
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
        _buildOrder = true;
        _isBuilding = false;
        foreach(Tile destTile in destTiles){
            TowerGuard guard = Instantiate(guardPrefab, new Vector3(transform.position.x, 1, transform.position.z), Quaternion.identity);
            guard.MovePosition(destTile.transform);
            _towerGuards.Add(guard);
        }
    }

    private void RetreatGuardsToBase(){
        foreach(TowerGuard towerGuard in _towerGuards){
            towerGuard.MovePosition(_startTile.transform);
        }
    }

    private IEnumerator FinishBuildingCoroutine(){
        // Wait 5 seconds for wall to build
        yield return new WaitForSeconds(5);
        EventManager.Instance.FinishBuildingWalls();
        RetreatGuardsToBase();
    }
}
