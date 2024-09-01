using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    private void OnEnable()
    {
        EventManager.Instance.OnUpdateNavMesh += UpdateNavMesh;
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnUpdateNavMesh -= UpdateNavMesh;
        }
    }
    private void Start()
    {
        UpdateNavMesh();
    }

    private void UpdateNavMesh(){
        StartCoroutine(UpdateNavMeshCoroutine());
    }

    private IEnumerator UpdateNavMeshCoroutine(){
        // waiting 1 seconds for the walls to rise after starting building walls.
        yield return new WaitForSeconds(1);
        navMeshSurface.BuildNavMesh();
    }

    

    
}
