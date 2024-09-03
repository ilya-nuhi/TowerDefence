using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty,
    Occupied,
    Wall,
}
public class Tile : MonoBehaviour
{
    public TileType Type { get; private set; }
    public int X { get; private set; }
    public int Z { get; private set; }

    public void Initialize(int x, int z)
    {
        X = x;
        Z = z;
        Type = TileType.Empty;
    }

    public void SetTileType(TileType type)
    {
        Type = type;
        
        switch (type)
        {
            case TileType.Occupied:
                GetComponent<MeshRenderer>().material = ResourceHolder.Instance.occupiedMaterial;
                break;
            case TileType.Wall:
                GameObject wall = Instantiate(ResourceHolder.Instance.wallPrefab, transform.position, Quaternion.identity);
                wall.transform.parent = transform;
                break;
        }
    }
    
}
