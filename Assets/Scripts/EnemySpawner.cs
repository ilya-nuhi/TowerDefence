using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemy;  // The enemy prefab to spawn
    [SerializeField] private float spawnInterval = 3f;  // Time interval between spawns

    private bool _isSpawned ;
    // Start is called before the first frame update
    void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnEnemies());
    }

    // Coroutine to spawn enemies at intervals
    private IEnumerator SpawnEnemies()
    {
        while (true && !_isSpawned)
        {

            // Wait for the specified interval before spawning the next enemy
            yield return new WaitForSeconds(spawnInterval);
            // Spawn the enemy at the specified spawn point
            Instantiate(enemy, transform.position, Quaternion.identity);
            _isSpawned = true;
        }
    }
}