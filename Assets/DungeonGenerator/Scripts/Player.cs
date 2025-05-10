using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;
    public Animator animator;

    [HideInInspector] public Vector2 movement; // Сделали public

    private Vector2 lastMoveDirection= Vector2.down;

    void Update()
{
    movement.x = Input.GetAxisRaw("Horizontal");
    movement.y = Input.GetAxisRaw("Vertical");

    // Фиксируем последнее направление только при движении
    if (movement.magnitude > 0.1f)
    {
        lastMoveDirection = movement.normalized;
        animator.SetBool("IsMoving", true);
    }
    else
    {
        animator.SetBool("IsMoving", false);
    }

    // Всегда передаём последнее направление
    animator.SetFloat("Horizontal", lastMoveDirection.x);
    animator.SetFloat("Vertical", lastMoveDirection.y);
}

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}