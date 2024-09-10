using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 5f;  // Time interval between spawns
    // Start is called before the first frame update
    void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnEnemies());
    }

    // Coroutine to spawn enemies at intervals
    private IEnumerator SpawnEnemies()
    {
        while (true)
        {

            // Wait for the specified interval before spawning the next enemy
            yield return new WaitForSeconds(spawnInterval);
            // Spawn the enemy at the specified spawn point
            ObjectPool.Instance.GetEnemy(transform.position);
        }
    }
    
}