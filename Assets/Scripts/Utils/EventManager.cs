using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public event Action<List<Tile>, List<Tile>> OnSpawnTowerGuards;
    public event Action<List<Tile>> OnFinishBuildingWalls;
    public event Action OnUpdateNavMesh;
    
    public void SpawnTowerGuards(List<Tile> destinationTiles, List<Tile> wallTiles)
    {
        OnSpawnTowerGuards?.Invoke(destinationTiles, wallTiles);
    }

    public void FinishBuildingWalls(List<Tile> archerTiles){
        OnFinishBuildingWalls?.Invoke(archerTiles);
    }
    public void UpdateNavMesh()
    {
        OnUpdateNavMesh?.Invoke();
    }
    
}
