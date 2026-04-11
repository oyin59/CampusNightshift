using UnityEngine;

namespace Player
{
    /// <summary>
    /// Handles player movement using Unity's CharacterController.
    /// Also passes movement speed to the Animator (Lab 5 specs).
    /// Modular design: only handles inputs to movement, nothing else.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        [Header("Noise Settings (Day 3)")]
        [Tooltip("How much noise the player makes when sprinting")]
        [SerializeField] private float sprintNoise = 95f;
        [Tooltip("How much noise the player makes when walking")]
        [SerializeField] private float walkNoise = 45f;
        [Tooltip("How much noise the player makes when crouching")]
        [SerializeField] private float crouchNoise = 10f;
        [Tooltip("How much noise the player makes when standing still")]
        [SerializeField] private float idleNoise = 0f;
        [Tooltip("How quickly the noise meter smooths out")]
        [SerializeField] private float noiseSmoothSpeed = 5f;

        public float CurrentNoiseLevel { get; private set; }

        [Header("Dependencies")]
        [Tooltip("The main camera so movement is relative to where we look.")]
        [SerializeField] private Transform mainCamera;
        [Tooltip("Animator to trigger walking animations.")]
        [SerializeField] private Animator animator;

        // Private caching
        private CharacterController controller;
        private float turnSmoothVelocity;
        private Vector3 velocity; // For gravity
        private float originalHeight; // To remember how tall we are before crouching

        private void Awake()
        {
            // Cache the reference to avoid expensive GetComponent calls in Update
            controller = GetComponent<CharacterController>();
            if (controller != null) originalHeight = controller.height;

            if (mainCamera == null)
            {
                mainCamera = Camera.main.transform;
                Debug.Assert(mainCamera != null, "PlayerController: Main Camera is missing!");
            }
        }

        private void Update()
        {
            HandleRotation();
            HandleMovement();
            ApplyGravity();
        }

        /// <summary>
        /// Reads input to actively turn the character. Supports Mouse and Keyboard (Q/E).
        /// </summary>
        private void HandleRotation()
        {
            // --- DYNAMIC MOUSE LOOK (Day 5) ---
            // Grabs the custom sensitivity from the Settings Menu, mapping 45 slider value to the old 200f baseline (4.44x scale)
            float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 45f) * 4.44f;
            
            float mouseX = Input.GetAxis("Mouse X") * turnSmoothTime;
            transform.Rotate(Vector3.up * mouseX * mouseSensitivity * Time.deltaTime);

            // --- ACCESSIBILITY KEYBOARD TURNING (Q / E) ---
            // If the user holds Q, spin left. If E, spin right.
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up * -150f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up * 150f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Reads input, moves the CharacterController relative to where the player is facing, 
        /// and updates the Animator.
        /// </summary>
        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal"); // A/D (Left/Right)
            float vertical = Input.GetAxisRaw("Vertical");     // W/S (Forward/Back)
            
            // Create a movement vector based on input
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // Update Animator (Lab 5 requirement: 'walkingSpeed')
            if (animator != null)
            {
                animator.SetFloat("walkingSpeed", inputDirection.magnitude);
            }

            // ONLY move if there is actual input
            float targetNoise = idleNoise;

            if (inputDirection.magnitude >= 0.1f)
            {
                // Dynamic Speed & Noise depending on inputs!
                float currentSpeed = walkSpeed;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentSpeed = sprintSpeed;
                    targetNoise = sprintNoise;
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                {
                    currentSpeed = crouchSpeed;
                    targetNoise = crouchNoise;
                }
                else
                {
                    currentSpeed = walkSpeed;
                    targetNoise = walkNoise;
                }

                Vector3 moveDir = transform.TransformDirection(inputDirection);
                controller.Move(moveDir * currentSpeed * Time.deltaTime);
            }

            // Dynamic Crouching Physically squashes the CharacterController collision box down!
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
            {
                controller.height = Mathf.Lerp(controller.height, originalHeight / 2f, Time.deltaTime * 6f);
            }
            else
            {
                controller.height = Mathf.Lerp(controller.height, originalHeight, Time.deltaTime * 6f);
            }

            // Smoothly calculate current noise level
            CurrentNoiseLevel = Mathf.Lerp(CurrentNoiseLevel, targetNoise, Time.deltaTime * noiseSmoothSpeed);
        }

        /// <summary>
        /// Keeps the player grounded using standard gravity simulation.
        /// </summary>
        private void ApplyGravity()
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small constant downward force when grounded to stick to floor
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
