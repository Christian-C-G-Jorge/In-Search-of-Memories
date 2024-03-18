using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int attackDamage = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object has the tag "Player"
        if (other.CompareTag("Player"))
        {
            HealthManager playerHealth = other.GetComponent<HealthManager>();

            // Check if the HealthManager component is not null
            if (playerHealth != null)
            {
                // Inflict damage on the player using the takeDamage method
                playerHealth.takeDamage(attackDamage);
                Debug.Log("Player Hit by Enemy! Damage: " + attackDamage);
            }
        }
    }
}
