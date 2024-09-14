using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    public event Action<List<Tile>, List<Tile>> OnConstructWall;
    public event Action<List<Tile>> OnFinishBuildingWalls;
    
    public void ConstructWall(List<Tile> destinationTiles, List<Tile> wallTiles)
    {
        OnConstructWall?.Invoke(destinationTiles, wallTiles);
    }

    public void FinishBuildingWalls(List<Tile> archerTiles){
        OnFinishBuildingWalls?.Invoke(archerTiles);
    }
    
}
