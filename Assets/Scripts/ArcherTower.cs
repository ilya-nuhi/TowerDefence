using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ArcherTower : MonoBehaviour
{
    public float fireRate = 1f; // Fire rate in seconds
    public GameObject arrowPrefab; // Prefab of the arrow
    public Transform arrowSpawnPoint; // Spawn point of the arrow
    public float arrowSpeed = 20f; // Speed of the arrow
    [SerializeField] private SphereCollider towerCollider;
    [SerializeField] private LayerMask enemyLayerMask;

    private float _nextFireTime;
    private List<EnemyHealth> _enemyHealthsInRange;
    private Enemy _targetEnemy;

    void OnEnable()
    {
        RebuildEnemiesInRange();
    }
    
    void Update()
    {
        // Fire at the closest enemy if one is in range
        if (_targetEnemy!=null && Time.time >= _nextFireTime)
        {
            ShootAtEnemy();
            _nextFireTime = Time.time + fireRate;
        }
    }
    
    // Rebuild the list of enemies currently in the collider area
    private void RebuildEnemiesInRange()
    {
        // Clear the current list
        _enemyHealthsInRange = new List<EnemyHealth>();
        Collider[] results = new Collider[100];
        // Detect all colliders within the detection area
        var size = Physics.OverlapSphereNonAlloc(transform.position, towerCollider.radius, results, enemyLayerMask);

        for (int i = 0; i < size; i++)
        {
            // Check if the collider belongs to an enemy
            EnemyHealth enemyHealth = results[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                _enemyHealthsInRange.Add(enemyHealth);
                enemyHealth.OnDeath += RemoveEnemyFromRange;
            }
        }

        // Optionally, set the target enemy if there are any in range
        if (_enemyHealthsInRange.Count > 0)
        {
            _targetEnemy = _enemyHealthsInRange[0].GetComponent<Enemy>();  // Set the first enemy as the target
        }
        else
        {
            _targetEnemy = null;  // No enemies in range
        }
    }
    
    
    private void ShootAtEnemy()
    {
        if (_targetEnemy == null) return;
        if (!_targetEnemy.isActiveAndEnabled)
        {
            SetNewTargetAndRemoveInactive();
        }
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
        Arrow arrow = ObjectPool.Instance.GetArrow(arrowSpawnPoint.position, arrowSpawnPoint.rotation);
        arrow.rigidBody.isKinematic = false;
        
        arrow.rigidBody.velocity = direction * arrowSpeed;
    }

    // Called when an enemy enters the tower's range
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            
            EnemyHealth newEnemyHealth = other.GetComponent<EnemyHealth>();
            if (!_enemyHealthsInRange.Contains(newEnemyHealth))
            {
                _enemyHealthsInRange.Add(newEnemyHealth);
                newEnemyHealth.OnDeath += RemoveEnemyFromRange;
            }
            
            if (_targetEnemy==null)
            {
                _targetEnemy  = newEnemyHealth.GetComponent<Enemy>();
            }
        }
    }

    // Called when an enemy exits the tower's range
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth quittingEnemyHealth = other.GetComponent<EnemyHealth>();
            _enemyHealthsInRange.Remove(quittingEnemyHealth);
            quittingEnemyHealth.OnDeath -= RemoveEnemyFromRange;
            if (_targetEnemy == quittingEnemyHealth.GetComponent<Enemy>())
            {
                SetNewTargetAndRemoveInactive();
            }
        }
    }

    private void SetNewTargetAndRemoveInactive()
    {
        if (_enemyHealthsInRange.Count > 0)
        {
            for (int i = 0; i < _enemyHealthsInRange.Count; i++)
            {
                var enemyHealth = _enemyHealthsInRange[i];

                if (enemyHealth!=null && enemyHealth.isActiveAndEnabled && IsEnemyInTriggerArea(enemyHealth))
                {
                    _targetEnemy = enemyHealth.GetComponent<Enemy>(); // Set the first valid enemy as the target
                    break; // Stop after finding the first valid enemy
                }
                else
                {
                    _enemyHealthsInRange.RemoveAt(i); // Remove invalid enemy
                    enemyHealth.OnDeath -= RemoveEnemyFromRange;
                    i--; // Adjust index after removal to avoid skipping elements
                }
            }
        }

        // If no valid enemies are found in range, set target to null
        if (_enemyHealthsInRange.Count == 0)
        {
            _targetEnemy = null;
        }
    }

    // Helper method to check if an enemy is still in the trigger area
    private bool IsEnemyInTriggerArea(EnemyHealth enemyHealth)
    {
        float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);
        return distance <= towerCollider.radius * transform.localScale.x; // Adjust for scaling
    }
    
    private void RemoveEnemyFromRange(EnemyHealth enemyHealth)
    {
        if (_enemyHealthsInRange.Contains(enemyHealth))
        {
            _enemyHealthsInRange.Remove(enemyHealth);
            enemyHealth.OnDeath -= RemoveEnemyFromRange;

            // If the target enemy was the one that died, set a new target
            if (_targetEnemy == enemyHealth.GetComponent<Enemy>())
            {
                SetNewTargetAndRemoveInactive();
            }
        }
    }
    
}

