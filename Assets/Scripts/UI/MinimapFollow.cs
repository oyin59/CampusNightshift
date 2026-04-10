using UnityEngine;

namespace UI
{
    /// <summary>
    /// Attaches to the Minimap Orthographic Camera.
    /// Forces the camera to hover exactly above the player at a fixed height,
    /// without rotating when the player turns.
    /// </summary>
    public class MinimapFollow : MonoBehaviour
    {
        [Tooltip("Drag the Player object here")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("How high in the sky should the camera sit?")]
        [SerializeField] private float cameraHeight = 50f;

        private void Start()
        {
            // --- SETTINGS (Day 5) ---
            bool showMinimap = PlayerPrefs.GetInt("ShowMinimap", 1) == 1;
            if (!showMinimap)
            {
                gameObject.SetActive(false); // Completely disables the Minimap rendering loop
            }
        }

        private void LateUpdate()
        {
            if (playerTransform != null)
            {
                // Snap our X and Z to the player, but keep our sky-high Y position!
                Vector3 newPosition = playerTransform.position;
                newPosition.y = cameraHeight;
                transform.position = newPosition;
                
                // We lock our rotation looking straight down (90 on X axis)
                // so the minimap doesn't spin sickeningly when the player turns.
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }
}
