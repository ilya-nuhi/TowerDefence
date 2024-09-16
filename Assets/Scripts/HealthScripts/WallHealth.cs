using System;
using System.Collections;
using UnityEngine;

public class WallHealth : Health
{
    [SerializeField] private float regenRate = 5f;     // Amount of health to regenerate per second

    private Coroutine _regenHealthCoroutine;
    private Wall _wall;

    protected void Start()
    {
        _wall = GetComponent<Wall>();
    }

    protected override void HandleZeroHealth()
    {
        _wall.DestroyWall();
        if (_regenHealthCoroutine != null)
        {
            StopCoroutine(_regenHealthCoroutine); // Stop regeneration when wall is destroyed
        }
    }
    
    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            HandleZeroHealth();
        }
        
        ShowHealthBar();
        
        // Start health regeneration if it's not already active and health is not full
        if (_regenHealthCoroutine == null && currentHealth > 0 && currentHealth < maxHealth)
        {
            _regenHealthCoroutine = StartCoroutine(RegenerateHealth());
        }
    }
    
    private IEnumerator RegenerateHealth()
    {
        while (currentHealth < maxHealth)
        {
            // Wait for 1 second before regenerating health
            yield return new WaitForSeconds(1f);

            // Increase health
            currentHealth += regenRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health does not exceed max

            if (healthSlider != null)
            {
                healthSlider.value = currentHealth; // Update health slider
            }

            // Stop regeneration if health is full
            if (currentHealth >= maxHealth)
            {
                _regenHealthCoroutine = null;
                yield break; // Exit the coroutine
            }
        }
    }
    
}