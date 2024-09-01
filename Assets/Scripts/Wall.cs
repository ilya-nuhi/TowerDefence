using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GameObject archerTower;

    public void BuildArcherTower(){
        // wait 1 second to wall to build
        archerTower = Instantiate(ResourceHolder.Instance.archerTowerPrefap, 
                                    new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                                    Quaternion.identity); 
        archerTower.transform.parent = transform;
    }

}
