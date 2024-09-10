using System;
using UnityEngine;

public class EnemyHealth : Health
{
    protected override void HandleZeroHealth()
    {
        base.HandleZeroHealth();
        // Implement enemy-specific death behavior
        Destroy(gameObject);
    }
    
}