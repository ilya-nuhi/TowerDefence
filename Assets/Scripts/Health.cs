using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider; // Reference to the health slider UI
    [SerializeField] private CanvasGroup canvasGroup;
    private Coroutine _fadeOutBarCoroutine;
    
    private float _currentHealth;

    void Start()
    {
        _currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
        }
        canvasGroup.alpha = 0f;
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        // Update the health slider
        if (healthSlider != null)
        {
            healthSlider.value = _currentHealth;
        }

        // Check if the wall should be destroyed
        if (_currentHealth <= 0)
        {
            DestroyWall();
            EventManager.Instance.UpdateNavMesh();
        }

        ShowHealthBar();
    }

    private void ShowHealthBar()
    {
        // Stop any existing fade-out coroutine
        if (_fadeOutBarCoroutine != null)
        {
            StopCoroutine(_fadeOutBarCoroutine);
        }
        
        // Immediately show the health bar
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        _fadeOutBarCoroutine = StartCoroutine(FadeOutBar());
    }
    
    private IEnumerator FadeOutBar()
    {
        // Wait for 3 seconds before starting the fade-out
        yield return new WaitForSeconds(3f);

        // Fade out the health bar
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < 2)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / 2);
            yield return null;
        }

        // Ensure the alpha is set to 0 at the end
        canvasGroup.alpha = 0f;
    }

    private void DestroyWall()
    {
        // Handle wall destruction logic
        GetComponent<Wall>().DestroyWall();
    }
}