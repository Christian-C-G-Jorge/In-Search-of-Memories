using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float dashDistance;
    public float dashCooldown;
    public float dashSpeedMultiplier;
    private bool canDash = true;
    private Vector2 input; 

    void Update()
    {
        // Get player input for movement
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        // Check if there's any input from the player
        if (input != Vector2.zero)
        {
            // Check if the dash button (X key) is pressed
            if (Input.GetKeyDown(KeyCode.X) && canDash)
            {
                Debug.Log("Dash!");
                // Exectue dash function 
                StartCoroutine(Dash(input.normalized));
            }
            else
            {
                // Calculate the target position for regular movement
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                // Move towards the target position with regular movement speed
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
        }
    }

    // Coroutine for handling the dash movement (Function is used executing over mutiple frames)
    IEnumerator Dash(Vector2 direction)
    {
        Debug.Log("Dash coroutine started");

        // Calculate the target position for the dash
        Vector3 dashTarget = transform.position + new Vector3(direction.x * dashDistance, direction.y * dashDistance, 0f);
        // Calculate the speed of the dash
        float dashSpeed = moveSpeed * dashSpeedMultiplier;
        // Get the start time of the dash
        float startTime = Time.time;

        // Perform the dash movement until reaching the target position
        while (Time.time < startTime + dashDistance / dashSpeed)
        {
            // Move towards the dash target position at the dash speed
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure reaching the exact dash target position
        transform.position = dashTarget;

        // End of dash, apply cooldown
        Debug.Log("Dash coroutine ended");
        canDash = false;
        yield return new WaitForSeconds(dashCooldown); // Wait for the dash cooldown period
        canDash = true;
    }
}