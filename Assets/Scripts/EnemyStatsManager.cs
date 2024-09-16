using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsManager : MonoBehaviour
{
    public static float Damage = 10f;
    public static float Health = 100f;
    public static float Speed = 3.5f;

    private void Update()
    {
        // Detect spacebar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IncreaseEnemyStats();
        }
    }
    
    public static void IncreaseEnemyStats()
    {
        Damage += 2f; // Increase damage by 50%
        Health += 20f;  // Increase health by 50%
        Speed += 0.5f;   // Increase speed by 20%
    }
}

