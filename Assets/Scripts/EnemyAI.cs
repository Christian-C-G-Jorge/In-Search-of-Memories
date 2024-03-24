using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float chaseRadius = 4f;
    public float roamRadius = 2f;
    public float minSpeed = 1f;
    public float smoothTime = 0.5f;
    private Vector3 lastKnownPlayerPosition;
    private bool playerLastSeen;
    private Vector3 currentVelocity = Vector3.zero;

    // Layer mask for solid objects
    public LayerMask solidObjectsLayer;

    void Start()
    {
        // Initialize the last known player position to the initial position of the enemy
        lastKnownPlayerPosition = transform.position;

        // Set playerLastSeen to false initially since the player hasn't been seen yet
        playerLastSeen = false;
    }

    void Update()
    {
        // Get Distance to follow player
        float distance = Vector2.Distance(transform.position, player.transform.position);

        // If the player is within chase radius, move towards player
        if (distance < chaseRadius)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // If player was last seen, move towards last known player position
            if (playerLastSeen)
            {
                MoveTowardsLastKnownPosition();
            }
            // If player was not last seen, perform random roaming behavior
            else
            {
                RandomRoam();
            }
        }

        // Reduce enemy speed when moving to last player position
        if (playerLastSeen && speed > minSpeed)
        {
            Decelerate();
        }
    }

    // Move the enemy towards the player
    void MoveTowardsPlayer()
    {
        Vector2 direction = player.transform.position - transform.position;
        direction.Normalize();

        // Check if the next position is walkable before moving towards the player
        Vector3 nextPosition = transform.position + (Vector3)direction * speed * Time.deltaTime;
        if (IsWalkable(nextPosition))
        {
            // Sets AI angle, allows for directional tracking
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(Vector3.forward * angle);
            lastKnownPlayerPosition = player.transform.position;
            playerLastSeen = true;
        }
        else
        {
            // Player cannot move towards the player, so stop chasing
            playerLastSeen = false;
        }
    }

    // Move the enemy towards the last known player position
    void MoveTowardsLastKnownPosition()
    {
        float roamingDistance = Vector2.Distance(transform.position, lastKnownPlayerPosition);
        if (roamingDistance > roamRadius)
        {
            // Check if the target position is walkable
            if (IsWalkable(lastKnownPlayerPosition))
            {
                transform.position = Vector3.SmoothDamp(transform.position, lastKnownPlayerPosition, ref currentVelocity, smoothTime, speed);
            }
            else
            {
                playerLastSeen = false; // Player is behind a solid object, stop chasing
            }
        }
        else
        {
            playerLastSeen = false;
        }
    }

    // Perform random roaming behavior
    void RandomRoam()
    {
        float randomX = UnityEngine.Random.Range(-5f, 5f);
        float randomY = UnityEngine.Random.Range(-5f, 5f);
        Vector3 randomDirection = new Vector3(randomX, randomY, 0);
        
        // Check if the target position is walkable
        if (IsWalkable(randomDirection))
        {
            transform.position = Vector3.SmoothDamp(transform.position, randomDirection, ref currentVelocity, smoothTime, speed);
        }
    }

    // Gradually reduce speed as it approaches last known player position
    void Decelerate()
    {
        speed -= 0.1f * Time.deltaTime;
        speed = Mathf.Max(speed, minSpeed);
    }

    // Function to check if a position is walkable
    bool IsWalkable(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, direction.magnitude, solidObjectsLayer);
        
        return hit.collider == null;
    }
}