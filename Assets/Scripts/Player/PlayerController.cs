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
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        [Header("Dependencies")]
        [Tooltip("The main camera so movement is relative to where we look.")]
        [SerializeField] private Transform mainCamera;
        [Tooltip("Animator to trigger walking animations.")]
        [SerializeField] private Animator animator;

        // Private caching
        private CharacterController controller;
        private float turnSmoothVelocity;
        private Vector3 velocity; // For gravity

        private void Awake()
        {
            // Cache the reference to avoid expensive GetComponent calls in Update
            controller = GetComponent<CharacterController>();

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
            // --- STANDARD MOUSE LOOK ---
            float mouseX = Input.GetAxis("Mouse X") * turnSmoothTime;
            transform.Rotate(Vector3.up * mouseX * 200f * Time.deltaTime);

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

            // Only move if there is actual input
            if (inputDirection.magnitude >= 0.1f)
            {
                // We convert our local WASD input (inputDirection) into World Space based on our current rotation.
                // This means 'W' (vertical=1) will always push us in the direction we are currently facing, 
                // and 'S' (vertical=-1) will push us backwards.
                Vector3 moveDir = transform.TransformDirection(inputDirection);

                // Move the character
                controller.Move(moveDir * walkSpeed * Time.deltaTime);
            }
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
