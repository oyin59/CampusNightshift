using UnityEngine;
using UnityEditor;

namespace UI.Editor
{
    /// <summary>
    /// A simple utility to help you find the WorldMin/WorldMax coordinates for your GameHUD map.
    /// Instructions: 
    /// 1. Open the Window -> Map Calibrator.
    /// 2. Pick any two objects in your scene that represent the Bottom-Left and Top-Right of your school building.
    /// 3. The script will give you the numbers to type into your GameHUD script!
    /// </summary>
    public class MapCalibrator : EditorWindow
    {
        private GameObject corner1;
        private GameObject corner2;

        [MenuItem("Window/Map Calibrator")]
        public static void ShowWindow()
        {
            GetWindow<MapCalibrator>("Map Calibrator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Map Bound Calculator", EditorStyles.boldLabel);
            
            corner1 = (GameObject)EditorGUILayout.ObjectField("Bottom-Left Corner Obj", corner1, typeof(GameObject), true);
            corner2 = (GameObject)EditorGUILayout.ObjectField("Top-Right Corner Obj", corner2, typeof(GameObject), true);

            if (GUILayout.Button("Calculate Bounds"))
            {
                if (corner1 != null && corner2 != null)
                {
                    Vector3 p1 = corner1.transform.position;
                    Vector3 p2 = corner2.transform.position;

                    // Calculate Min/Max based on the two points
                    float minX = Mathf.Min(p1.x, p2.x);
                    float maxX = Mathf.Max(p1.x, p2.x);
                    float minZ = Mathf.Min(p1.z, p2.z);
                    float maxZ = Mathf.Max(p1.z, p2.z);

                    Debug.Log("<color=cyan><b>CALIBRATION RESULT:</b></color>");
                    Debug.Log($"<b>World Min:</b> X: {minX:F2}, Y: {minZ:F2}");
                    Debug.Log($"<b>World Max:</b> X: {maxX:F2}, Y: {maxZ:F2}");
                    
                    EditorUtility.DisplayDialog("Calibration Complete", 
                        $"Copy these into your GameHUD Inspector:\n\nWorld Min: ({minX:F2}, {minZ:F2})\nWorld Max: ({maxX:F2}, {maxZ:F2})", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please drag two corner objects from your scene into the slots!", "OK");
                }
            }
        }
    }
}
