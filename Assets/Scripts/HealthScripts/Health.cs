using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected Slider healthSlider;  // Reference to the health slider UI
    [SerializeField] protected CanvasGroup canvasGroup;
    protected float currentHealth;
    private Coroutine _fadeOutBarCoroutine;


    private void OnEnable()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void OnDisable()
    {
        if (_fadeOutBarCoroutine != null)
        {
            StopCoroutine(_fadeOutBarCoroutine);
        }
    }

    public virtual void TakeDamage(float amount)
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
        else
        {
            ShowHealthBar();
        }

    }

    protected virtual void HandleZeroHealth(){}

    protected void ShowHealthBar()
    {
        if (_fadeOutBarCoroutine != null)
        {
            StopCoroutine(_fadeOutBarCoroutine);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        _fadeOutBarCoroutine = StartCoroutine(FadeOutBar());
    }

    private IEnumerator FadeOutBar()
    {
        yield return new WaitForSeconds(3f);

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / 2f);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth += health-maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    
}
