using UnityEngine;

public class WallHealth : Health
{
    private Wall _wall;

    protected void Start()
    {
        _wall = GetComponent<Wall>();
    }

    protected override void HandleZeroHealth()
    {
        _wall.DestroyWall();
        EventManager.Instance.UpdateNavMesh();
    }
    
    public override void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        if (healthSlider != null)
        {
            healthSlider.value = _currentHealth;
        }

        if (_currentHealth <= 0)
        {
            HandleZeroHealth();
        }
        
        ShowHealthBar();
    }
}