using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerFOV playerFOV;
    
    // --- Sound System Integration ---
    [Header("Sound Settings")]
    [Tooltip("Time between footstep sounds while moving")]
    public float footstepInterval = 0.4f;
    
    private SoundEmitter soundEmitter;
    private float lastFootstepTime;
    private bool wasMovingLastFrame = false;
    
    private PlayerInput playerInput;
    private Vector2 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadow;
    

    private void Awake()
    {
        playerInput = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        
        // Configure Rigidbody2D for proper physics
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
        shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();
        
        // --- Sound System Setup ---
        soundEmitter = GetComponent<SoundEmitter>();
        if (soundEmitter == null)
        {
            Debug.LogWarning("SoundEmitter component not found on the player GameObject. Sound system will not work.");
        }
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void Update()
    {
        PlayerInput();
        FlipSprite(); // Flip sprite in Update for responsiveness
        
        // --- Sound System: Footstep Detection ---
        bool isMoving = movement.magnitude > 0.1f;
        
        if (soundEmitter != null)
        {
            if (isMoving && !wasMovingLastFrame)
            {
                // Just started moving - start continuous footstep audio
                soundEmitter.StartFootstepAudio();
            }
            else if (!isMoving && wasMovingLastFrame)
            {
                // Just stopped moving - pause footstep audio
                soundEmitter.PauseFootstepAudio();
            }
            
            // Continue emitting footstep events for AI detection at intervals while moving
            if (isMoving && Time.time - lastFootstepTime >= footstepInterval)
            {
                soundEmitter.EmitFootstep(); // For AI detection only, no audio playback
                lastFootstepTime = Time.time;
            }
        }
        
        wasMovingLastFrame = isMoving;
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
            Vector3 rightVector = new Vector3(0.34f, -0.12f, -1f);
            spriteRenderer.flipX = true;
            shadow.flipX = true;
            playerFOV.SetAimDirection(up);
            playerFOV.SetOrigin(transform.position);
            if (playerFOV.transform.localPosition != rightVector)
                playerFOV.transform.localPosition = rightVector;
            
        }
        else if (movement.x < -0.01f)
        {
            Vector3 leftVector = new Vector3(-0.34f, -0.17f, -1f);
            spriteRenderer.flipX = false;
            shadow.flipX = false;
            playerFOV.SetAimDirection(down);
            playerFOV.SetOrigin(transform.position);
            if (playerFOV.transform.localPosition != leftVector)
                playerFOV.transform.localPosition = leftVector;
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
    
    // --- Sound System Methods ---
    
    /// <summary>
    /// Public method to emit loud sounds (for interactions, breaking objects, etc.)
    /// </summary>
    public void EmitInteractionSound(float intensity, float range)
    {
        if (soundEmitter != null)
        {
            soundEmitter.EmitLoudNoise(intensity, range);
        }
    }
}