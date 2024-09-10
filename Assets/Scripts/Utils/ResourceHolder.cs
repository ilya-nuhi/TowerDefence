using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ResourceHolder : Singleton<ResourceHolder>
{
    public Tile[,] tiles;
    public List<Tile> wallTiles;
    public Material occupiedMaterial;
    public Material emptyMaterial;
    public Material constructMaterial;
    public GameObject archerTowerPrefap;

}
