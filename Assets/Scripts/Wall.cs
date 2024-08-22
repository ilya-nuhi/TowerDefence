using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{

    private void Start() {
        StartCoroutine(RiseWall());    
    }

    public IEnumerator RiseWall(){
        GetComponent<MeshRenderer>().material = ResourceHolder.Instance.wallMaterial;

        float duration = 1.0f; // 1 second to rise
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, 1.0f, startPos.z); // Raise the wall to y = 1.0f

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // Ensure it ends at the exact final position
    }


}
