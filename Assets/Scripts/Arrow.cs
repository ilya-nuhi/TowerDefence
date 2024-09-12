using System;
using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 10f;
    public float destroyTime = 5f;
    [SerializeField] public Rigidbody rigidBody;
    private Coroutine _destroyCoroutine;

    private void OnEnable()
    {
        _destroyCoroutine = StartCoroutine(DestroyArrowAfterTime());
    }
    
    
    private IEnumerator DestroyArrowAfterTime()
    {
        yield return new WaitForSeconds(destroyTime);
        DestroyArrow();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Cancel the destruction if the arrow hits something
        
        
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        
        if (enemyHealth != null && enemyHealth.isActiveAndEnabled)
        {
            enemyHealth.OnDeath += DestroyArrow;
            enemyHealth.TakeDamage(damage);
            // if arrow hits an enemy stop destroying coroutine
            if (_destroyCoroutine != null)
            {
                StopCoroutine(_destroyCoroutine);
                _destroyCoroutine = null;
            }
        }
        
        rigidBody.isKinematic = true;
        // Make the arrow stick to the object it hit
        transform.parent = collision.transform;
    }

    void DestroyArrow(EnemyHealth enemyHealth = null)
    {
        if (enemyHealth!=null)
        {
            enemyHealth.OnDeath -= DestroyArrow;
        }
        ObjectPool.Instance.ReturnArrow(this);
    }
}