using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemShapeData))]
public class ItemShapeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get a reference to our ScriptableObject target data
        ItemShapeData data = (ItemShapeData)target;

        // 1. Draw standard Width and Height integer input fields
        int newWidth = Mathf.Max(1, EditorGUILayout.IntField("Bounding Width", data.boundingWidth));
        int newHeight = Mathf.Max(1, EditorGUILayout.IntField("Bounding Height", data.boundingHeight));

        // If the developer changes dimensions, resize the flat array instantly
        if (newWidth != data.boundingWidth || newHeight != data.boundingHeight || data.shapeMatrix == null || data.shapeMatrix.Length != newWidth * newHeight)
        {
            Undo.RecordObject(data, "Resize Shape Matrix");
            data.boundingWidth = newWidth;
            data.boundingHeight = newHeight;

            int[] oldMatrix = data.shapeMatrix;
            data.shapeMatrix = new int[data.boundingWidth * data.boundingHeight];

            // Retain old values where applicable during resize adjustments
            if (oldMatrix != null)
            {
                for (int i = 0; i < oldMatrix.Length && i < data.shapeMatrix.Length; i++)
                {
                    data.shapeMatrix[i] = oldMatrix[i];
                }
            }
            EditorUtility.SetDirty(data);
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Visual Shape Grid Designer", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Check a box to mark a cell as occupied (1). Clear it for an empty space (0).");
        EditorGUILayout.Space(5);

        // 2. Draw the 2D Visual Checkbox Layout Matrix Grid
        // Loop through the height (Rows)
        for (int y = 0; y < data.boundingHeight; y++)
        {
            // Start a horizontal row block layout
            EditorGUILayout.BeginHorizontal();

            // Loop through the width (Columns)
            for (int x = 0; x < data.boundingWidth; x++)
            {
                int index = y * data.boundingWidth + x;

                // Read current state: true if 1, false if 0
                bool isOccupied = data.shapeMatrix[index] == 1;

                // Draw a sleek visual toggle checkbox button
                // Width constraint ensures it stays square and doesn't stretch across the inspector window
                bool newState = EditorGUILayout.Toggle(isOccupied, GUILayout.Width(30), GUILayout.Height(30));

                // If clicked, register changes
                if (newState != isOccupied)
                {
                    Undo.RecordObject(data, "Toggle Shape Grid Cell");
                    data.shapeMatrix[index] = newState ? 1 : 0;
                    EditorUtility.SetDirty(data);
                }
            }

            // End the current horizontal row block layout
            EditorGUILayout.EndHorizontal();
        }
    }
}