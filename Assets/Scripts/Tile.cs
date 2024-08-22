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
        // Update material or other properties based on type
        var renderer = GetComponent<MeshRenderer>();
        switch (type)
        {
            case TileType.Wall:
                gameObject.AddComponent<Wall>(); 
                renderer.material = ResourceHolder.Instance.wallMaterial;
                break;
            case TileType.Occupied:
                renderer.material = ResourceHolder.Instance.occupiedMaterial;
                break;
        }
    }

}
