using UnityEngine;

namespace Player
{
    /// <summary>
    /// A clean, simple follow camera. Maintains a set distance and height behind the target.
    /// Uses LateUpdate so the target has completely finished moving before the camera updates.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;

        [Header("Offset Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
        [SerializeField] private float followSpeed = 10f;

        private void LateUpdate()
        {
            // We use Debug.Assert to catch setup errors immediately without spamming the console
            Debug.Assert(target != null, "FollowCamera: Target is not assigned!");
            if (target == null) return;

            // Calculate the desired position based on the target's rotation and our offset
            // This ensures if the player rotates, the camera rotates around them
            Vector3 desiredPosition = target.position + (target.rotation * offset);

            // Smoothly move the camera towards that position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            // Force the camera to always look slightly above the target's origin (e.g. at their back/head)
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
}
