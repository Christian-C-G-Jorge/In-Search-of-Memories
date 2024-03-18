using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Reduce health when damaged
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy Hit!");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Death function
    void Die()
    {
        Destroy(gameObject);
        Debug.Log("Enemy died!");
    }
}