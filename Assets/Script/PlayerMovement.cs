using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Required for Volume
using UnityEngine.Rendering.Universal; // Required for URP effects like DepthOfField


public class PlayerMovement : MonoBehaviour
{
    [Header("Post-Processing Blur Settings")]
    [SerializeField] private Volume postProcessingVolume; // Assign your Global Volume object here
    [SerializeField] private float minFocalLength = 1f;   // Focal length for NO blur
    [SerializeField] private float maxFocalLength = 100f; // Focal length for MAXIMUM blur
    [SerializeField] private float minVignetteIntensity = 0.2f; // Intensitas Vignette saat stamina penuh
    [SerializeField] private float maxVignetteIntensity = 0.7f; // Intensitas Vignette saat stamina habis


    [Header("Movement Settings")]
    [SerializeField] float speedX;
    [SerializeField] private float runSpeedMultiplier = 1.8f; // Multiplier for running speed
    public bool isAbleToMove = true;
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 10f; // Stamina drained per second
    [SerializeField] private float staminaRegenRate = 30f; // Stamina regenerated per second
    [SerializeField] private float staminaRegenDelay = 1.5f; // Delay before regen starts
    
    // Public state booleans
    public bool facingRight = true;
    public bool isRunning = false;

    // Private internal variables
    private DepthOfField depthOfField;
    private Vignette vignette;

    private float currentStamina;
    private float timeSinceStoppedRunning = 0f;
    private float leftRightValue;
    private float xVal;
    private Animator animator;
    private Rigidbody2D rbdy;

    void Start()
    {
        animator = GetComponent<Animator>();
        rbdy = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina;

        // Try to get the Depth of Field effect from the assigned Volume Profile
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out depthOfField))
        {
            // Initialize it to the minimum blur
            depthOfField.focalLength.value = minFocalLength;
        }
        else
        {
            Debug.LogError("PlayerMovement: No Post Processing Volume assigned or it's missing a Depth of Field override!");
        }
        
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = minVignetteIntensity;
        }
        else
        {
            Debug.LogError("PlayerMovement: The assigned Post Processing Volume is missing a Vignette override!");
        }
    }

    void Update()
    {
        // Handle input for movement and running
        if (isAbleToMove)
        {
            leftRightValue = Input.GetAxisRaw("Horizontal");

            // Can only START running if you have stamina
            if (Input.GetKeyDown(KeyCode.LeftShift) && currentStamina > 0)
            {
                isRunning = true;
            }
            // Stop running when key is released
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                isRunning = false;
            }
        }
        else
        {
            // If unable to move, ensure we are not running and reset input
            isRunning = false;
            leftRightValue = 0f;
        }
        
        // Always handle stamina and UI updates
        HandleStamina();

        // Update animator if movement is disabled
        if (!isAbleToMove)
        {
            animator.SetFloat("xVelocity", 0);
        }
    }

    private void FixedUpdate()
    {
        if (isAbleToMove)
        {
            PlayerMove();
        }
    }

    private void HandleStamina()
    {
        if (isRunning && leftRightValue != 0)
        {
            timeSinceStoppedRunning = 0f; // Reset regen delay timer
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isRunning = false; // Force stop running if stamina is depleted
            }
        }
        else
        {
            timeSinceStoppedRunning += Time.deltaTime;
            // Regenerate stamina after the delay has passed
            if (timeSinceStoppedRunning >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // Ensure it doesn't exceed max
            }
        }

        float staminaPercentageMissing = 1f - (currentStamina / maxStamina);
        if (depthOfField != null)
        {
            float targetFocalLength = Mathf.Lerp(minFocalLength, maxFocalLength, staminaPercentageMissing);
            depthOfField.focalLength.value = targetFocalLength;
        }
        
        if (vignette != null)
        {
            float targetIntensity = Mathf.Lerp(minVignetteIntensity, maxVignetteIntensity, staminaPercentageMissing);
            vignette.intensity.value = targetIntensity;
        }
    }

    void PlayerMove()
    {
        xVal = leftRightValue * speedX * Time.deltaTime;
        
        // Multiply speed by the multiplier if running
        if (isRunning)
        {
            xVal *= runSpeedMultiplier;
        }

        // Apply final velocity and update animator
        Vector3 targetVelocity = new Vector3(xVal, rbdy.velocity.y);
        rbdy.velocity = targetVelocity;
        animator.SetFloat("xVelocity", Mathf.Abs(rbdy.velocity.x)); // More robust to read from Rigidbody

        // Flip character direction
        if (facingRight && leftRightValue < 0)
        {
            Flip(leftRightValue);
            facingRight = false;
        }
        else if (!facingRight && leftRightValue > 0)
        {
            Flip(leftRightValue);
            facingRight = true;
        }
    }

    private void Flip(float horizontalDirection)
    {
        if (horizontalDirection > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDirection < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void ForceMove(float direction)  
    {
        rbdy.velocity = new Vector2(direction * speedX, rbdy.velocity.y);
        animator.SetFloat("xVelocity", Mathf.Abs(rbdy.velocity.x)); // Using Rigidbody's velocity is safer here
        
        // Handle flipping based on the forced direction
        if (direction > 0) facingRight = true;
        if (direction < 0) facingRight = false;
        Flip(direction);
    }
}