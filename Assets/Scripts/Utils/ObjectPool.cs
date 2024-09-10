using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject towerGuardPrefab;
    [SerializeField] private GameObject enemyPrefab;
    
    private Queue<GameObject> _wallPool = new Queue<GameObject>();
    private Queue<GameObject> _towerGuardPool = new Queue<GameObject>();
    private Queue<GameObject> _enemyPool = new Queue<GameObject>();

    // Method to get a wall object from the pool
    public GameObject GetWall(Tile wallTile)
    {
        GameObject wall;
        if (_wallPool.Count > 0)
        {
            wall = _wallPool.Dequeue();
            wall.transform.position = wallTile.transform.position;
            wall.SetActive(true);
            
        }
        else
        {
            wall = Instantiate(wallPrefab, wallTile.transform.position, Quaternion.identity);
        }
        wall.transform.parent = wallTile.transform;
        return wall;
    }

    // Method to return a wall object to the pool
    public void ReturnWall(GameObject wall)
    {
        wall.transform.parent = transform;
        wall.GetComponent<WallHealth>().ResetHealth();
        wall.SetActive(false);
        _wallPool.Enqueue(wall);
    }

    // Method to get a tower guard object from the pool
    public GameObject GetTowerGuard(Transform guardSpawner)
    {
        GameObject towerGuard;
        if (_towerGuardPool.Count > 0)
        {
            towerGuard = _towerGuardPool.Dequeue();
            towerGuard.transform.position = new Vector3(guardSpawner.position.x, 1, guardSpawner.position.z);
            towerGuard.SetActive(true);
        }
        else
        {
            towerGuard = Instantiate(towerGuardPrefab, guardSpawner.position, Quaternion.identity);
            towerGuard.transform.parent = transform;
        }
        
        return towerGuard;
    }

    // Method to return a tower guard object to the pool
    public void ReturnTowerGuard(GameObject towerGuard)
    {
        towerGuard.SetActive(false);
        _towerGuardPool.Enqueue(towerGuard);
    }

    // Method to get an enemy object from the pool
    public GameObject GetEnemy(Vector3 position)
    {
        GameObject enemy;
        if (_enemyPool.Count > 0)
        {
            enemy = _enemyPool.Dequeue();
            enemy.transform.position = position;
            enemy.SetActive(true);
            
        }
        else
        {
            enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        }
        
        return enemy;
    }

    // Method to return an enemy object to the pool
    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        _enemyPool.Enqueue(enemy);
    }
    
    
    
    
    
}
