using UnityEngine;
using Player; // To use IInteractable
using Systems; // To talk to GameManager

namespace World
{
    /// <summary>
    /// Put this on the 3 items the player needs to find.
    /// Implements IInteractable so the PlayerInteraction raycast can see it.
    /// </summary>
    public class Collectible : MonoBehaviour, IInteractable
    {
        [Header("Item Details")]
        [Tooltip("The name of the item (e.g., 'Fuse', 'Keycard')")]
        public string itemName = "Objective Item";

        [Header("Feedback")]
        [Tooltip("Sound to play when picked up (Optional)")]
        public AudioClip pickupSound;

        // This method is required because we signed the 'IInteractable' contract
        public void OnInteract()
        {
            Debug.Log($"Picked up: {itemName}");

            // 1. Tell the GameManager we got one!
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                manager.ObjectiveCollected();
            }
            else
            {
                Debug.LogWarning("Collectible: Could not find GameManager to update score!");
            }

            // 2. Play Audio (Day 8: Using the AudioTriggers manager)
            AudioTriggers audio = FindObjectOfType<AudioTriggers>();
            if (audio != null) audio.PlayCollectSound(transform.position);

            // 3. Destroy this object so you can't pick it up twice
            Destroy(gameObject);
        }
    }
}
