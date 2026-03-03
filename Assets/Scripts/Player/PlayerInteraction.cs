using UnityEngine;
using UI; // To access GameHUD

namespace Player
{
    /// <summary>
    /// Handles looking at and interacting with objects in the world.
    /// Day 6 Goal: Crosshair Raycasting and 'E' to interact.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("How far away the player can reach out and interact with something.")]
        [SerializeField] private float interactRange = 3f;
        [Tooltip("The LayerMask for interactable objects to optimize the raycast.")]
        [SerializeField] private LayerMask interactableLayer;
        
        [Header("Dependencies")]
        [Tooltip("Leave blank to auto-find Main Camera")]
        public Camera mainCamera; // Public so we don't need a custom editor to see it

        private GameHUD gameHUD;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            
            // Find the HUD to turn the crosshair red when hovering
            gameHUD = FindObjectOfType<GameHUD>();
        }

        private void Update()
        {
            CheckForInteractable();
        }

        /// <summary>
        /// Shoots an invisible laser out of the exact center of the screen every frame.
        /// </summary>
        private void CheckForInteractable()
        {
            if (mainCamera == null) return;

            // Calculate the exact center of the screen
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);

            // Did the laser hit anything within reach on the 'Interactable' layer?
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
            {
                // Is this object programmed to be interactable?
                IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
                if (interactableObject != null)
                {
                    // 1. Tell the UI to make the crosshair red (Visual Feedback)
                    if (gameHUD != null) gameHUD.SetCrosshairColor(Color.red);

                    // 2. Listen for the E key
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactableObject.OnInteract();
                    }
                    
                    return; // Exit early so we don't reset the crosshair color below
                }
            }

            // If we hit nothing, or the thing we hit wasn't interactable, keep crosshair white
            if (gameHUD != null) gameHUD.SetCrosshairColor(Color.white);
        }
    }

    /// <summary>
    /// The contract every interactable object must sign.
    /// If a script implements this, the PlayerInteraction system guarantees 
    /// it can call OnInteract() when the player presses E.
    /// </summary>
    public interface IInteractable
    {
        void OnInteract();
    }
}
