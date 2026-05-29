using UnityEngine;

[CreateAssetMenu(fileName = "New Item Shape", menuName = "Inventory/Item Shape")]
public class ItemShapeData : ScriptableObject
{
    // The visual inspector hides these fields and handles them cleanly via ItemShapeDataEditor
    [HideInInspector] public int boundingWidth = 1;
    [HideInInspector] public int boundingHeight = 1;
    [HideInInspector] public int[] shapeMatrix;
}