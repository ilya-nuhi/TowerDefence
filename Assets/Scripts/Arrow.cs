using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 10f;
    public float destroyTime = 5f;
    [SerializeField] public Rigidbody rigidBody;
    void Start()
    {
        Destroy(gameObject, destroyTime); // Destroy the arrow after some time if it doesn't hit anything
    }

    void OnCollisionEnter(Collision collision)
    {
        EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
        
        rigidBody.isKinematic = true;
        // Make the arrow stick to the object it hit
        transform.parent = collision.transform;
    }
}