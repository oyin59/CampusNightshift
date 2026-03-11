using UnityEngine;
using Player; // To use IInteractable

namespace World
{
    /// <summary>
    /// Put this script on a Door's Hinge to make it interactable.
    /// It smoothly rotates the door when you press E. Supports Double Doors.
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [Tooltip("How many degrees the door should swing open (usually 90 or -90)")]
        [SerializeField] private float openAngle = 90f;
        [Tooltip("How fast the door swings")]
        [SerializeField] private float openSpeed = 5f;

        [Header("Double Door Link (Optional)")]
        [Tooltip("Drag the other Door script here if they should open together")]
        [SerializeField] private Door linkedDoor;

        private bool isOpen = false;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool isMoving = false;

        private void Start()
        {
            // Remember our starting rotation as the "Closed" state
            closedRotation = transform.rotation;
            
            // Calculate what the "Open" state should look like
            openRotation = Quaternion.Euler(
                transform.eulerAngles.x, 
                transform.eulerAngles.y + openAngle, 
                transform.eulerAngles.z
            );
        }

        private void Update()
        {
            // Smoothly animate the door opening or closing towards the target rotation
            if (isMoving)
            {
                Quaternion targetRotation = isOpen ? openRotation : closedRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

                // Stop calculating math if we are extremely close to the target angle
                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                {
                    transform.rotation = targetRotation; // Snap it perfectly
                    isMoving = false;
                }
            }
        }

        /// <summary>
        /// Called by the PlayerInteraction raycast when E is pressed.
        /// </summary>
        public void OnInteract()
        {
            ToggleDoor(true);
        }

        /// <summary>
        /// Toggles this door. If triggerLinked is true, it also toggles the linked door.
        /// </summary>
        public void ToggleDoor(bool triggerLinked)
        {
            isOpen = !isOpen;
            isMoving = true;

            // If we have a second door linked, tell IT to toggle too!
            // We pass 'false' to the linked door so it doesn't try to trigger us back in an infinite loop.
            if (triggerLinked && linkedDoor != null)
            {
                linkedDoor.ToggleDoor(false);
            }
        }
    }
}
