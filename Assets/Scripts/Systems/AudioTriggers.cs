using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Rubric: Simple audio triggers
    /// Handles playing global sounds like the scary "caught" noise or background ambiance.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioTriggers : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("The scary sound that plays the moment the guard catches you")]
        [SerializeField] private AudioClip caughtSound;
        
        [Tooltip("The sound that plays when you collect a battery")]
        [SerializeField] private AudioClip collectSound;
        
        [Tooltip("Creepy background ambiance")]
        [SerializeField] private AudioClip ambientMusic;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            
            // Setup Background Music
            if (ambientMusic != null)
            {
                audioSource.clip = ambientMusic;
                audioSource.loop = true;
                audioSource.volume = 0.3f; // Keep it quiet
                audioSource.Play();
            }
        }

        /// <summary>
        /// Call this when the Game Over state triggers.
        /// </summary>
        public void PlayCaughtSound()
        {
            if (caughtSound != null)
            {
                // PlayClipAtPoint creates a temporary audio source that plays the sound and cleans itself up.
                // It ensures the sound plays completely even if the main object is destroyed or disabled.
                AudioSource.PlayClipAtPoint(caughtSound, Camera.main.transform.position, 1.0f);
            }
        }
        
        /// <summary>
        /// Call this when a collectible is grabbed.
        /// </summary>
        public void PlayCollectSound(Vector3 position)
        {
            if (collectSound != null)
            {
                 AudioSource.PlayClipAtPoint(collectSound, position, 0.7f);
            }
        }
    }
}
