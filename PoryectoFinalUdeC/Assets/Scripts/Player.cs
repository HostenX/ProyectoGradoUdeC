using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;

public class Player : MonoBehaviour
{
    public float speed;
    public Animator animator;

    private bool isFlipped = false; // Track the flip state

    private void Update()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Create direction vector
        Vector2 direction = new Vector2(horizontal, vertical).normalized;

        // Animate player
        AnimateMovement(direction);

        // Apply movement
        transform.position += new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }

    void Interact()
    {
        Vector3 facingDir = new Vector3(animator.GetFloat("horizontal"), animator.GetFloat("vertical"), 0f);
        Debug.DrawRay(transform.position, facingDir, Color.red, 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDir, 1f);

        if (hit.collider != null && hit.collider.CompareTag("Interactable"))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }



    void AnimateMovement(Vector2 direction)
    {
        if (animator != null)
        {
            if (direction.magnitude > 0)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("horizontal", direction.x);
                animator.SetFloat("vertical", direction.y);

                // Control animation speed based on the player's speed
                animator.speed = speed;

                // Rotate Animation - only when moving horizontally
                if (direction.x != 0)
                {
                    bool shouldFlip = direction.x < 0;

                    // Flip only when the direction has changed
                    if (shouldFlip != isFlipped)
                    {
                        transform.rotation = Quaternion.Euler(new Vector3(0f, shouldFlip ? 180f : 0f, 0f));
                        isFlipped = shouldFlip;
                    }
                }
            }
            else
            {
                animator.SetBool("isMoving", false);
                // Set animation speed back to 1 (default) when not moving
                animator.speed = 1;
            }
        }
    }
}


