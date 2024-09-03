using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceHolder : Singleton<ResourceHolder>
{
    public Tile[,] tiles;
    public List<Tile> walls;
    public Material occupiedMaterial;
    public Material wallMaterial;
    public Material constructMaterial;
    public GameObject archerTowerPrefap;
    public GameObject wallPrefab;

}
