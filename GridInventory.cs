using UnityEngine;

public class GridInventory : MonoBehaviour
{
    public int gridWidth = 4;
    public int gridHeight = 4;
    public float cellSize = 50f; // Matching your UI cell size in pixels

    private bool[,] grid;
    private RectTransform rectTransform;

    void Awake()
    {
        grid = new bool[gridWidth, gridHeight];
        rectTransform = GetComponent<RectTransform>();
    }

    // Converts a screen position (mouse) to local grid coordinates (X, Y)
    public bool ScreenPointToGridPos(Vector2 screenPos, out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out Vector2 localPos))
            return false;

        // Calculate the top-left corner as (0,0) origin
        float originX = localPos.x + (rectTransform.rect.width / 2);
        float originY = (rectTransform.rect.height / 2) - localPos.y;

        gridX = Mathf.FloorToInt(originX / cellSize);
        gridY = Mathf.FloorToInt(originY / cellSize);

        // Check if inside bounds
        return (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight);
    }

    public bool CheckPlacement(int startX, int startY, int width, int height)
    {
        if (startX + width > gridWidth || startY + height > gridHeight) return false;
        if (startX < 0 || startY < 0) return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[startX + x, startY + y]) return false;
            }
        }
        return true;
    }

    public void PlaceItemData(int startX, int startY, int width, int height, bool state)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[startX + x, startY + y] = state;
            }
        }
    }

    public bool IsInventoryPerfectlyFull()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y]) return false;
            }
        }
        return true;
    }

    // Checks if the custom shape slots are clear on the board
    public bool CheckCustomShapePlacement(int startX, int startY, DraggableItem item)
    {
        // Ensure the outer bounding box doesn't clip outside the inventory edges
        if (startX + item.boundingWidth > gridWidth || startY + item.boundingHeight > gridHeight) return false;
        if (startX < 0 || startY < 0) return false;

        // Loop through the item's custom matrix space
        for (int x = 0; x < item.boundingWidth; x++)
        {
            for (int y = 0; y < item.boundingHeight; y++)
            {
                // If the item has a physical piece here, see if the grid cell is occupied
                if (item.IsCellFilled(x, y))
                {
                    if (grid[startX + x, startY + y])
                    {
                        return false; // Collision detected!
                    }
                }
            }
        }
        return true;
    }

    // Writes the custom shape data into the grid master array
    public void PlaceCustomShapeData(int startX, int startY, DraggableItem item)
    {
        for (int x = 0; x < item.boundingWidth; x++)
        {
            for (int y = 0; y < item.boundingHeight; y++)
            {
                if (item.IsCellFilled(x, y))
                {
                    grid[startX + x, startY + y] = true;
                }
            }
        }
    }

    // Clears only the custom shape data cells when picked back up
    public void ClearItemData(int startX, int startY, DraggableItem item)
    {
        for (int x = 0; x < item.boundingWidth; x++)
        {
            for (int y = 0; y < item.boundingHeight; y++)
            {
                if (item.IsCellFilled(x, y))
                {
                    grid[startX + x, startY + y] = false;
                }
            }
        }
    }

}