using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
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
