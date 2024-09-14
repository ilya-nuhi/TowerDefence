using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentTest : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;
    
    void Start()
    {
        agent.Warp(transform.position);
        agent.SetDestination(target.position);
    }

   
}
