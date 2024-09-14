using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class TowerGuard : MonoBehaviour
{
    [SerializeField] public NavMeshAgent agent;
    [SerializeField] private float damagePerSecond = 10f; // Damage dealt per second
    [SerializeField] private SphereCollider guardDetectionCollider;
    [SerializeField] private LayerMask enemyLayerMask;

    private Coroutine _detectAndDamageCoroutine;
    private HashSet<EnemyHealth> _enemyHealthsInRange;
    public Enemy targetEnemy;
    private Transform _wallTransform;
    private Coroutine _destinationReachedRoutine;
    public event Action<TowerGuard> OnReachedDestination;

    private void OnEnable()
    {
        Debug.Log("TowerGuard enabled: Starting enemy detection and movement logic.");
        RebuildEnemiesInRange();
        _detectAndDamageCoroutine = StartCoroutine(CheckingCoroutine());
    }

    private void OnDisable()
    {
        Debug.Log("TowerGuard disabled: Stopping enemy detection and movement logic.");
        if (_detectAndDamageCoroutine != null)
        {
            StopCoroutine(_detectAndDamageCoroutine);
            _detectAndDamageCoroutine = null;
        }
        OnReachedDestination = null;
    }

    private void RebuildEnemiesInRange()
    {
        Debug.Log("Rebuilding enemies in range...");
        targetEnemy = null;
        _enemyHealthsInRange = new HashSet<EnemyHealth>();
        Collider[] results = new Collider[100];
        var size = Physics.OverlapSphereNonAlloc(transform.position, guardDetectionCollider.radius, results, enemyLayerMask);

        for (int i = 0; i < size; i++)
        {
            EnemyHealth enemyHealth = results[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                _enemyHealthsInRange.Add(enemyHealth);
                enemyHealth.OnDeath += RemoveEnemyFromRange;
                Debug.Log($"Detected enemy in range: {enemyHealth.gameObject.name}");
            }
        }

        SetClosestEnemyAsTarget();
    }

    private void RemoveEnemyFromRange(EnemyHealth enemyHealth)
    {
        if (_enemyHealthsInRange.Remove(enemyHealth))
        {
            Debug.Log($"Enemy removed from range: {enemyHealth.gameObject.name}");
            enemyHealth.OnDeath -= RemoveEnemyFromRange;

            if (targetEnemy == enemyHealth.GetComponent<Enemy>())
            {
                Debug.Log("Target enemy died, finding closest enemy...");
                SetClosestEnemyAsTarget();
            }
        }
    }

    private void SetClosestEnemyAsTarget()
    {
        if (_enemyHealthsInRange.Count > 0)
        {
            EnemyHealth closestEnemy = null;
            float shortestDistance = Mathf.Infinity;
            List<EnemyHealth> enemiesToRemove = new List<EnemyHealth>();

            foreach (var enemyHealth in _enemyHealthsInRange)
            {
                if (enemyHealth != null && enemyHealth.isActiveAndEnabled && IsEnemyInTriggerArea(enemyHealth))
                {
                    float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestEnemy = enemyHealth;
                    }
                }
                else
                {
                    enemiesToRemove.Add(enemyHealth);
                }
            }

            foreach (var enemyToRemove in enemiesToRemove)
            {
                _enemyHealthsInRange.Remove(enemyToRemove);
                enemyToRemove.OnDeath -= RemoveEnemyFromRange;
            }

            if (closestEnemy != null)
            {
                targetEnemy = closestEnemy.GetComponent<Enemy>();
                Debug.Log($"Closest enemy set as target: {targetEnemy.gameObject.name}");
            }
            else
            {
                Debug.Log("No valid enemies found, setting target to null.");
                targetEnemy = null;
            }
        }

        if (_enemyHealthsInRange.Count == 0)
        {
            Debug.Log("No enemies in range.");
            targetEnemy = null;
        }

        SetGuardDestination();
    }

    private void SetGuardDestination()
    {
        if (targetEnemy != null)
        {
            Debug.Log($"Guard moving to enemy: {targetEnemy.gameObject.name}");
            agent.SetDestination(targetEnemy.transform.position);
            if (_destinationReachedRoutine != null)
            {
                StopCoroutine(_destinationReachedRoutine);
                _destinationReachedRoutine = null;
            }
            targetEnemy.navAgent.isStopped = true;
        }
        else
        {
            if (_wallTransform != null)
            {
                Debug.Log("No enemies detected, moving to wall.");
                SetDutyDestination(_wallTransform);
            }
            else
            {
                Debug.Log("No target set for the guard.");
            }
        }
    }

    private bool IsEnemyInTriggerArea(EnemyHealth enemyHealth)
    {
        float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);
        return distance <= guardDetectionCollider.radius * transform.localScale.x;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Enemy entered guard's detection range: {other.gameObject.name}");
            EnemyHealth newEnemyHealth = other.GetComponent<EnemyHealth>();
            if (!_enemyHealthsInRange.Contains(newEnemyHealth))
            {
                _enemyHealthsInRange.Add(newEnemyHealth);
                newEnemyHealth.OnDeath += RemoveEnemyFromRange;
            }

            if (targetEnemy == null)
            {
                targetEnemy = newEnemyHealth.GetComponent<Enemy>();
                Debug.Log($"New enemy target: {targetEnemy.gameObject.name}");
                agent.SetDestination(targetEnemy.transform.position);
                targetEnemy.navAgent.isStopped = true;
                if (_destinationReachedRoutine != null)
                {
                    StopCoroutine(_destinationReachedRoutine);
                    _destinationReachedRoutine = null;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Enemy exited guard's detection range: {other.gameObject.name}");
            EnemyHealth quittingEnemyHealth = other.GetComponent<EnemyHealth>();
            _enemyHealthsInRange.Remove(quittingEnemyHealth);
            quittingEnemyHealth.OnDeath -= RemoveEnemyFromRange;
            if (targetEnemy == quittingEnemyHealth.GetComponent<Enemy>())
            {
                Debug.Log("Target enemy exited range, finding new target...");
                SetClosestEnemyAsTarget();
            }
        }
    }

    public void SetDutyDestination(Transform destination)
    {
        Debug.Log($"Setting destination to wall at position: {destination.position}");
        _wallTransform = destination;
        if (targetEnemy!=null) return; // if there is an enemy in range, don't leave the enemy.
        Vector3 destPos = new Vector3(destination.position.x, transform.position.y, destination.position.z);
        if (agent.isActiveAndEnabled)
        {
            agent.SetDestination(destPos);
            _destinationReachedRoutine = StartCoroutine(CheckDestinationReachedCoroutine());
        }
    }

    private IEnumerator CheckDestinationReachedCoroutine()
    {
        float maxTime = 100f;
        float elapsedTime = 0f;

        while (elapsedTime < maxTime)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                Debug.Log("Guard reached its destination.");
                OnReachedDestination?.Invoke(this);
                yield break;
            }

            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogWarning("Guard failed to reach the destination in time.");
    }

    private IEnumerator CheckingCoroutine()
    {
        while (true)
        {
            DetectAndDamage();
            yield return new WaitForSeconds(1);
        }
    }

    void DetectAndDamage()
    {
        Collider[] results = new Collider[15];
        var size = Physics.OverlapSphereNonAlloc(transform.position, 0.75f, results, enemyLayerMask);

        for (int i = 0; i < size; i++)
        {
            Collider currentCollider = results[i];
            Health health = currentCollider.GetComponent<Health>();
            if (health != null && health.isActiveAndEnabled)
            {
                Debug.Log($"Damaging enemy: {currentCollider.gameObject.name}");
                health.TakeDamage(damagePerSecond);
            }
        }
    }
}
