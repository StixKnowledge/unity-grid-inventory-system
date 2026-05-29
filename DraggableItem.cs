
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform visualSpriteElement; // Drag your child "VisualSprite" here in the inspector
    private float visualRotationAngle = 0f;  // Keeps track of visual rotation

    // The rectangular bounds of the item
    public int boundingWidth = 3;
    public int boundingHeight = 2;

    // Define the custom shape. 1 = filled, 0 = empty hole.
    // For a 3x2 pistol, this array will have 6 elements.
    [HideInInspector]
    public int[] shapeMatrix;

    private GridInventory inventory;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 originalPosition;
    private Transform originalParent;
    private int currentGridX = -1;
    private int currentGridY = -1;
    private bool isPlaced = false;
    private bool isCurrentlyDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventory = FindFirstObjectByType<GridInventory>();
        canvas = FindFirstObjectByType<Canvas>();

        // Default fallback initialization if not set in inspector: Solid rect
        if (shapeMatrix == null || shapeMatrix.Length != boundingWidth * boundingHeight)
        {
            shapeMatrix = new int[boundingWidth * boundingHeight];
            for (int i = 0; i < shapeMatrix.Length; i++) shapeMatrix[i] = 1;
        }
    }
    void Update()
    {
        if (isCurrentlyDragging)
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                RotateShape90Degrees();
            }
        }
    }

    // Helper to get shape data at a specific local coordinate
    public bool IsCellFilled(int localX, int localY)
    {
        int index = localY * boundingWidth + localX;
        if (index >= 0 && index < shapeMatrix.Length)
        {
            return shapeMatrix[index] == 1;
        }
        return false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isCurrentlyDragging = true; 

        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        if (isPlaced)
        {
            // Pass 'this' item so the grid knows exactly which shape slots to clear
            inventory.ClearItemData(currentGridX, currentGridY, this);
            isPlaced = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }


    private void RotateShape90Degrees()
    {
        int[] rotatedMatrix = new int[boundingWidth * boundingHeight];

        for (int y = 0; y < boundingHeight; y++)
        {
            for (int x = 0; x < boundingWidth; x++)
            {
                int newX = boundingHeight - 1 - y;
                int newY = x;
                rotatedMatrix[newY * boundingHeight + newX] = shapeMatrix[y * boundingWidth + x];
            }
        }

        // 1. Swap bounding box structural sizes
        int temp = boundingWidth;
        boundingWidth = boundingHeight;
        boundingHeight = temp;

        shapeMatrix = rotatedMatrix;

        // 2. Adjust the parent container size to fit the new flipped dimensions
        rectTransform.sizeDelta = new Vector2(boundingWidth * inventory.cellSize, boundingHeight * inventory.cellSize);

        // 3. Rotate the visual sprite component only
        visualRotationAngle -= 90f;
        if (visualSpriteElement != null)
        {
            visualSpriteElement.localRotation = Quaternion.Euler(0, 0, visualRotationAngle);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isCurrentlyDragging = false;
        canvasGroup.blocksRaycasts = true;

        // CRITICAL: We still look at the absolute Top-Left of the bounding box!
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);
        Vector2 topLeftCornerOfItem = objectCorners[1];

        float halfCell = inventory.cellSize / 2f;
        Vector2 evaluationPoint = new Vector2(topLeftCornerOfItem.x + halfCell, topLeftCornerOfItem.y - halfCell);

        if (inventory.ScreenPointToGridPos(evaluationPoint, out int gridX, out int gridY))
        {
            // Pass 'this' item to check the custom shape matrix instead of a rectangle
            if (inventory.CheckCustomShapePlacement(gridX, gridY, this))
            {
                inventory.PlaceCustomShapeData(gridX, gridY, this);
                currentGridX = gridX;
                currentGridY = gridY;
                isPlaced = true;

                SnapToGrid(gridX, gridY);
                return;
            }
        }

        rectTransform.position = originalPosition;
        transform.SetParent(originalParent, true);
    }

    private void SnapToGrid(int gridX, int gridY)
    {
        transform.SetParent(inventory.transform, true);
        RectTransform gridRect = inventory.GetComponent<RectTransform>();

        float localX = (gridX * inventory.cellSize) + ((boundingWidth * inventory.cellSize) / 2f) - (gridRect.rect.width / 2f);
        float localY = (gridRect.rect.height / 2f) - (gridY * inventory.cellSize) - ((boundingHeight * inventory.cellSize) / 2f);

        rectTransform.localPosition = new Vector3(localX, localY, 0);
    }
}