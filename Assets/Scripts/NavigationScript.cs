using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;
    public Transform destination;
    public bool isReachedDestination = false;

    public void SetDestination(Transform dest){
        navMeshAgent.destination = dest.position;
        destination = dest;
        isReachedDestination = false;
    }

    private void Update() {
        if(isReachedDestination) return;
        Vector2 distance = new Vector2(transform.position.x - destination.position.x, transform.position.z - destination.position.z);
        if(distance.magnitude < 0.5f){
            isReachedDestination = true;
        }
    }
}
