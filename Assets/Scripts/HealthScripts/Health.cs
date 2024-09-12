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
    protected float _currentHealth;
    private Coroutine _fadeOutBarCoroutine;


    private void OnEnable()
    {
        _currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
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
    
}
