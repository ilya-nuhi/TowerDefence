using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public ArcherTower archerTower;
    private List<Tile> _wallTiles;

    private void Start()
    {
        _wallTiles = ResourceHolder.Instance.wallTiles;
    }

    public void RiseWall(){
        StartCoroutine(RiseWallCoroutine());
    }

    public void DestroyWall(){
        Tile tile = GetComponentInParent<Tile>();
        if (_wallTiles!=null)
        {
            _wallTiles.Remove(tile);
        }
        if (tile != null) tile.SetTileType(TileType.Empty);
        StartCoroutine(DestroyWallCoroutine());
    }
    

    private IEnumerator RiseWallCoroutine(){

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

    private IEnumerator DestroyWallCoroutine()
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
        
        // return the wall and archer tower instances to pool
        ObjectPool.Instance.ReturnWall(this);
        if (archerTower != null)
        {
            ObjectPool.Instance.ReturnArcher(archerTower);
            archerTower = null;
        }
    }
    
    public void BuildArcherTower(){
        // wait 1 second to wall to build
        archerTower = ObjectPool.Instance.GetArcher(new Vector3(transform.position.x, transform.position.y + 0.5f,
            transform.position.z));
        archerTower.transform.parent = transform;
    }

}
