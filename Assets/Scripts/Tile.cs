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
                if (GetComponent<Wall>() == null)
                {
                    gameObject.AddComponent<Wall>();
                }
                renderer.material = ResourceHolder.Instance.wallMaterial;
                break;
            case TileType.Occupied:
                renderer.material = ResourceHolder.Instance.occupiedMaterial;
                break;
        }
    }
    
    public void RiseTile(){
        StartCoroutine(RiseTileCoroutine());
    }

    public void LowerTile(){
        StartCoroutine(LowerTileCoroutine());
    }
    

    private IEnumerator RiseTileCoroutine(){

        float duration = 1.0f; // 1 second to rise
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 1, startPos.z); // Raise the wall to y = 1.0f

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Ensure it ends at the exact final position
    }

    private IEnumerator LowerTileCoroutine()
    {
        float duration = 1.0f; // 1 second to lower
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 0.0f, startPos.z); // Lower the wall to y = 0.0f

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Ensure it ends at the exact final position
    }
}
