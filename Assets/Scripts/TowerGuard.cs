using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGuard : MonoBehaviour
{

    public NavigationScript navigation;

    public void MovePosition(Transform destination){
        navigation.SetDestination(destination);
    }
}
