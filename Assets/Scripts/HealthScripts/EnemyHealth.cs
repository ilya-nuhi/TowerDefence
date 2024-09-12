using System;
using UnityEngine;

public class EnemyHealth : Health
{
    public Action<EnemyHealth> OnDeath;
    protected override void HandleZeroHealth()
    {
        base.HandleZeroHealth();
        HandleDestroy();
    }

    public void HandleDestroy()
    {
        OnDeath?.Invoke(this);
        // Implement enemy-specific death behavior
        ObjectPool.Instance.ReturnEnemy(GetComponent<Enemy>());
    }
}