using UnityEngine;

namespace AI
{
    /// <summary>
    /// Attaches a Spotlight to a specific bone on the Guard's animated rig (like his hand)
    /// so the light physically swings as he patrols.
    /// </summary>
    public class GuardFlashlight : MonoBehaviour
    {
        [Header("Flashlight Settings")]
        [Tooltip("The Light component representing the flashlight")]
        [SerializeField] private Light spotlight;
        
        [Tooltip("The specific transform (e.g., RightHand) to attach the light to")]
        [SerializeField] private Transform handBone;
        
        [Tooltip("Optional offset to adjust how the light points out of the hand")]
        [SerializeField] private Vector3 positionOffset;
        [SerializeField] private Vector3 rotationOffset;

        private void Start()
        {
            if (spotlight == null)
            {
                // Try to find it on this object if not assigned
                spotlight = GetComponentInChildren<Light>();
            }

            if (spotlight != null && spotlight.type != LightType.Spot)
            {
                Debug.LogWarning("GuardFlashlight: The attached light is not a Spotlight!");
            }
        }

        private void LateUpdate()
        {
            // LateUpdate runs after the Animator has moved the bones for this frame.
            // We snap the flashlight to the hand bone so it perfectly follows the animation.
            if (handBone != null && spotlight != null)
            {
                spotlight.transform.position = handBone.position + handBone.TransformDirection(positionOffset);
                
                // Keep the light pointing in the same direction the hand is pointing, plus our custom offset
                spotlight.transform.rotation = handBone.rotation * Quaternion.Euler(rotationOffset);
            }
        }
    }
}
