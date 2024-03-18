using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private GameObject AttackArea = default;
    private bool attacking = false;
    private float timeToAttack = 0.25f;
    private float timer = 0f;

    void Start()
    {
        // Get a reference to the attack area GameObject
        AttackArea = transform.GetChild(0).gameObject;
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z)) 
        {
            Attack(); 
            Debug.Log("Player is attacking!");
        }

        // If currently attacking, update the timer
        if(attacking)
        {
            timer += Time.deltaTime;

            // If the attack animation duration is reached, deactivate the attack area
            if(timer >= timeToAttack)
            {
                timer = 0;
                attacking = false; 
                AttackArea.SetActive(attacking); // Deactivate the attack area
            }
        }
    }

    // Method to initiate the player's attack
    private void Attack()
    {
        attacking = true;
        AttackArea.SetActive(attacking); 
    }
}
