using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public int damage = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered!");

        // Check if the colliding object has the tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy collider detected!");

            // Get the EnemyHealth component from the colliding object
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                // Inflict damage on the enemy using the TakeDamage method
                enemyHealth.TakeDamage(damage);

                Debug.Log("Enemy Hit!");
            }
        }
    }
}