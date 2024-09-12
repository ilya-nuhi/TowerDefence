using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

            // Get the enemy from the object pool
            Enemy enemy = ObjectPool.Instance.GetEnemy(transform.position);

            // Disable the NavMeshAgent to prevent it from calculating paths prematurely
            enemy.navAgent.enabled = false;
            
            // Wait for one or more frames to ensure the NavMesh system updates properly
            yield return new WaitForSeconds(0.1f);

            // Re-enable the NavMeshAgent
            enemy.navAgent.enabled = true;
            
        }
    }
    
}