using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ArcherTower : MonoBehaviour
{
    public float fireRate = 1f; // Fire rate in seconds
    public GameObject arrowPrefab; // Prefab of the arrow
    public Transform arrowSpawnPoint; // Spawn point of the arrow
    public float arrowSpeed = 20f; // Speed of the arrow

    private float _nextFireTime;
    private List<Enemy> _enemiesInRange = new List<Enemy>();
    private Enemy _targetEnemy;

    void Update()
    {
        // Fire at the closest enemy if one is in range
        if (_targetEnemy!=null && Time.time >= _nextFireTime)
        {
            ShootAtEnemy();
            _nextFireTime = Time.time + fireRate;
        }
    }
    
    private void ShootAtEnemy()
    {
        if (_targetEnemy == null) return;
        
        // Get the enemy's velocity (if they have a NavMeshAgent)
        NavMeshAgent enemyNavAgent = _targetEnemy.navAgent;
        if (enemyNavAgent == null) return;
        Vector3 enemyVelocity = enemyNavAgent.velocity; // Get the enemy's current velocity
        
        // Calculate the distance between the tower and the enemy
        float distanceToEnemy = Vector3.Distance(transform.position, _targetEnemy.transform.position);

        // Calculate the time it will take for the arrow to reach the enemy
        float timeToReachTarget = distanceToEnemy / arrowSpeed;
        
        // Predict where the enemy will be based on their current velocity and the time to reach
        Vector3 predictedPosition = _targetEnemy.transform.position + (enemyVelocity * timeToReachTarget);
        
        // Aim at the enemy
        Vector3 direction = (predictedPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1);

        // Instantiate and shoot the arrow
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.velocity = direction * arrowSpeed;
    }

    // Called when an enemy enters the tower's range
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {

            Enemy newEnemy = other.GetComponent<Enemy>();
            _enemiesInRange.Add(newEnemy);
            if (_targetEnemy==null)
            {
                _targetEnemy  = newEnemy;
            }
        }
    }

    // Called when an enemy exits the tower's range
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy quittingEnemy = other.GetComponent<Enemy>();
            _enemiesInRange.Remove(quittingEnemy);
            if (_targetEnemy == quittingEnemy)
            {
                if (_enemiesInRange.Count == 0)
                {
                    _targetEnemy  = null;
                }
                else
                {
                    _targetEnemy  = _enemiesInRange[0];
                }
            }
        }
    }
}
