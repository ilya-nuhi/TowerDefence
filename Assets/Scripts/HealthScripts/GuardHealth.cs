using System;
using UnityEngine;

public class GuardHealth : Health
{
    protected override void HandleZeroHealth()
    {
        TowerGuard guard = GetComponent<TowerGuard>();
        if (guard.targetEnemy!=null) guard.targetEnemy.navAgent.isStopped = false;
        ObjectPool.Instance.ReturnTowerGuard(guard);
    }
    
}