using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerFOV playerFOV;
    private PlayerInput playerInput;
    private Vector2 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadow;
    

    private void Awake()
    {
        playerInput = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
        shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void Update()
    {
        PlayerInput();
        FlipSprite(); // Flip sprite in Update for responsiveness\
        
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void PlayerInput()
    {
        movement = playerInput.Movement.Move.ReadValue<Vector2>();
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void FlipSprite()
    {
        Vector3 up = new Vector3(0, 1, 0); 
        Vector3 down = new Vector3(0, -1, 0);
        Vector3 left = new Vector3(-1, 0, 0);
        Vector3 right = new Vector3(1, 0, 0);
        // If moving right, face right; if moving left, face left
        if (movement.x > 0.01f)
        {
            spriteRenderer.flipX = true;
            shadow.flipX = true;
            playerFOV.SetAimDirection(up);
            playerFOV.SetOrigin(transform.position);
        }
        else if (movement.x < -0.01f)
        {
            spriteRenderer.flipX = false;
            shadow.flipX = false;
            playerFOV.SetAimDirection(down);
            playerFOV.SetOrigin(transform.position);
        }
        else if (movement.y > 0.01f)
        {
            playerFOV.SetAimDirection(left);
            playerFOV.SetOrigin(transform.position);
        }
        else if (movement.y < -0.01f)
        {
            playerFOV.SetAimDirection(right);
            playerFOV.SetOrigin(transform.position);
        }
    }
}