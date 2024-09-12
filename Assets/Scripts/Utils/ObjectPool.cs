using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject towerGuardPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject selectionBoxPrefab;
    [SerializeField] private GameObject invisBoxPrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject arrowPrefab;
    
    
    private ObjectPooler<Wall> _wallPool;
    private ObjectPooler<TowerGuard> _towerGuardPool;
    private ObjectPooler<Enemy> _enemyPool;
    private ObjectPooler<GameObject> _selectionBoxPool;
    private ObjectPooler<GameObject> _invisBoxPool;
    private ObjectPooler<ArcherTower> _archerPool;
    private ObjectPooler<Arrow> _arrowPool;

    protected override void Awake()
    {
        base.Awake();
        _wallPool = new ObjectPooler<Wall>(wallPrefab);
        _towerGuardPool = new ObjectPooler<TowerGuard>(towerGuardPrefab);
        _enemyPool = new ObjectPooler<Enemy>(enemyPrefab);
        _selectionBoxPool = new ObjectPooler<GameObject>(selectionBoxPrefab);
        _invisBoxPool = new ObjectPooler<GameObject>(invisBoxPrefab);
        _archerPool = new ObjectPooler<ArcherTower>(archerPrefab);
        _arrowPool = new ObjectPooler<Arrow>(arrowPrefab);
    }

    public Wall GetWall(Tile wallTile)
    {
        Wall wall = _wallPool.GetObject();
        wall.transform.position = wallTile.transform.position;
        wall.transform.parent = wallTile.transform;
        return wall;
    }

    public void ReturnWall(Wall wall)
    {
        wall.transform.parent = transform;
        _wallPool.ReturnObject(wall);
    }

    public TowerGuard GetTowerGuard(Transform guardSpawner)
    {
        TowerGuard towerGuard = _towerGuardPool.GetObject();
        towerGuard.transform.position = new Vector3(guardSpawner.position.x, 1, guardSpawner.position.z);
        return towerGuard;
    }

    public void ReturnTowerGuard(TowerGuard towerGuard)
    {
        _towerGuardPool.ReturnObject(towerGuard);
    }

    public Enemy GetEnemy(Vector3 position)
    {
        Enemy enemy = _enemyPool.GetObject();
        enemy.transform.position = position;
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        _enemyPool.ReturnObject(enemy);
        enemy.navAgent.enabled = false;
    }

    public GameObject GetSelectionBox(Vector3 position)
    {
        GameObject selectionBox = _selectionBoxPool.GetObject();
        selectionBox.transform.position = new Vector3(position.x, 1, position.z);
        return selectionBox;
    }

    public void ReturnSelectionBox(GameObject selectionBox)
    {
        _selectionBoxPool.ReturnObject(selectionBox);
    }
    
    public GameObject GetInvisBox(Vector3 position)
    {
        GameObject invisBox = _invisBoxPool.GetObject();
        invisBox.transform.position = position;
        return invisBox;
    }

    public void ReturnInvisBox(GameObject invisBox)
    {
        _invisBoxPool.ReturnObject(invisBox);
    }
    
    public ArcherTower GetArcher(Vector3 position)
    {
        ArcherTower archer = _archerPool.GetObject();
        archer.transform.position = position;
        return archer;
    }

    public void ReturnArcher(ArcherTower archer)
    {
        _archerPool.ReturnObject(archer);
    }
    
    public Arrow GetArrow(Vector3 position, Quaternion rotation)
    {
        Arrow arrow = _arrowPool.GetObject();
        arrow.transform.position = position;
        arrow.transform.rotation = rotation;
        return arrow;
    }

    public void ReturnArrow(Arrow arrow)
    {
        _arrowPool.ReturnObject(arrow);
        arrow.transform.parent = transform;
    }
}

public class ObjectPooler<T> where T : class
{
    private readonly GameObject _prefab;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    public ObjectPooler(GameObject prefab)
    {
        this._prefab = prefab;
    }

    public T GetObject()
    {
        GameObject obj;
        if (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity);
        }

        // If T is a GameObject, return the GameObject itself, else return the component
        if (typeof(T) == typeof(GameObject))
        {
            return obj as T;
        }

        return obj.GetComponent<T>();
    }

    public void ReturnObject(T obj)
    {
        GameObject go = (obj as GameObject) ?? (obj as Component)?.gameObject;
        if (go != null)
        {
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }
}
