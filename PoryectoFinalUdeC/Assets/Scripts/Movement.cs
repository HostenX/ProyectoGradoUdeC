using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement : MonoBehaviour
{
	public float speed;

    public Animator animator;
    //Get input

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

       Vector3 direction = new Vector3(horizontal, vertical).normalized;

        AnimateMovement(direction);

        transform.position += direction * speed *Time.deltaTime;
    }
    //apply movement

    void AnimateMovement(Vector3 direction) {
        if (animator != null) {

            if (direction.magnitude > 0)
            {
                animator.SetBool("isMoving", true);

                animator.SetFloat("horizontal", direction.x);
				animator.SetFloat("vertical", direction.y);
			}
            else
            {
				animator.SetBool("isMoving", false);
			}
        }
    }
}
