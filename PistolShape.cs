using UnityEngine;

public class PistolShape : MonoBehaviour
{
    void Start()
    {
        DraggableItem item = GetComponent<DraggableItem>();

        // Defining a 3x2 Pistol L-Shape:
        // [1, 1, 1] -> Top barrel
        // [1, 0, 0] -> Grip
        item.shapeMatrix = new int[] {
            1, 1, 1,
            1, 0, 0
        };
    }
}