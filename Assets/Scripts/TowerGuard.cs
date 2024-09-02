using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class TowerGuard : MonoBehaviour
{

    [SerializeField] private NavMeshAgent agent;
    public event Action<TowerGuard> OnReachedDestination;
    

    public void SetDestination(Transform destination){
        Vector3 destPos = new Vector3(destination.position.x, transform.position.y, destination.position.z);
        agent.SetDestination(destPos);
        StartCoroutine(CheckDestinationReachedCoroutine());
    }
    
    private IEnumerator CheckDestinationReachedCoroutine()
    {
        float maxTime = 100f; // Maximum time to check
        float elapsedTime = 0f;

        while (elapsedTime < maxTime)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                OnReachedDestination?.Invoke(this);
                yield break;
            }

            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    
        Debug.LogWarning("TowerGuard failed to reach destination in time.");
    }

    
}
