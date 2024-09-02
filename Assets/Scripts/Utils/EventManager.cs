using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public event Action<List<Tile>> OnSpawnTowerGuards;
    public event Action OnFinishBuildingWalls;
    public event Action OnUpdateNavMesh;
    
    public void SpawnTowerGuards(List<Tile> destinationTiles)
    {
        OnSpawnTowerGuards?.Invoke(destinationTiles);
    }

    public void FinishBuildingWalls(){
        OnFinishBuildingWalls?.Invoke();
    }
    public void UpdateNavMesh()
    {
        OnUpdateNavMesh?.Invoke();
    }
    
}
