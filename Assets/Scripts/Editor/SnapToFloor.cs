using UnityEngine;
using UnityEditor;

namespace EditorUtilities
{
    /// <summary>
    /// A simple Editor script to force objects to snap to the floor perfectly.
    /// Good for waypoints.
    /// </summary>
    public class SnapToFloor
    {
        [MenuItem("Tools/Snap Selected To Floor")]
        public static void SnapSelected()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("SnapToFloor: Please select at least one object (like your Waypoints) in the Hierarchy first!");
                return;
            }

            int snappedCount = 0;

            foreach (GameObject obj in selectedObjects)
            {
                // To avoid snapping inside the floor, we start the raycast slightly above the object's current position
                Vector3 rayStart = obj.transform.position + Vector3.up * 5f; 
                
                // Shoot a laser straight down
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
                {
                    // If it hits anything (like your floor), move the object to that exact hit point
                    Undo.RecordObject(obj.transform, "Snap to Floor"); // Allows Ctrl+Z
                    obj.transform.position = hit.point;
                    snappedCount++;
                }
            }

            Debug.Log($"Successfully snapped {snappedCount} objects perfectly flush to the floor!");
        }
    }
}
