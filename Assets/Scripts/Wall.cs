using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GameObject archerTower;

    public void RiseWall(){
        StartCoroutine(RiseWallCoroutine());
    }

    public void DestroyWall(){
        StartCoroutine(DestroyWallCoroutine());
    }
    

    private IEnumerator RiseWallCoroutine(){

        float duration = 1.0f; // 1 second to rise
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 1, startPos.z); // Raise the wall to y = 1.0f

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Ensure it ends at the exact final position
    }

    private IEnumerator DestroyWallCoroutine()
    {
        float duration = 1.0f; // 1 second to lower
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 0.0f, startPos.z); // Lower the wall to y = 0.0f

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Ensure it ends at the exact final position
        
        Destroy(gameObject);
    }
    
    public void BuildArcherTower(){
        // wait 1 second to wall to build
        archerTower = Instantiate(ResourceHolder.Instance.archerTowerPrefap, 
                                    new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                                    Quaternion.identity); 
        archerTower.transform.parent = transform;
    }

}
