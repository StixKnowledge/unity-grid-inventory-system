using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Shape Configuration")]
    [Tooltip("The ScriptableObject asset holding this item's grid dimensions and layout.")]
    public ItemShapeData shapeData;

    [Header("Visual References")]
    [Tooltip("The nested child UI Image object that holds your actual artwork sprite.")]
    public RectTransform visualSpriteElement;

    // Runtime variables cloned from the ScriptableObject
    [HideInInspector] public int boundingWidth;
    [HideInInspector] public int boundingHeight;
    [HideInInspector] public int[] shapeMatrix;

    private GridInventory inventory;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // Position tracking flags
    private Vector3 originalPosition;
    private Transform originalParent;
    private int currentGridX = -1;
    private int currentGridY = -1;
    private bool isPlaced = false;
    private bool isCurrentlyDragging = false;
    private float visualRotationAngle = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Find main engine dependencies in the active scene
        inventory = FindFirstObjectByType<GridInventory>();
        canvas = FindFirstObjectByType<Canvas>();

        // Initialize using the custom ScriptableObject data asset
        if (shapeData != null)
        {
            boundingWidth = shapeData.boundingWidth;
            boundingHeight = shapeData.boundingHeight;

            // CRITICAL: Clone the array so runtime rotation changes 
            // don't permanently write back over your original asset file!
            shapeMatrix = (int[])shapeData.shapeMatrix.Clone();
        }
        else
        {
            // Fallback default setup if no asset is attached (Solid 1x1 block)
            boundingWidth = 1;
            boundingHeight = 1;
            shapeMatrix = new int[] { 1 };
        }
    }

    // Helper method used by the master grid to determine if a relative coordinate is occupied
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

        // Lift the item to the top layer of the canvas hierarchy to prevent layout clipping
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false; // Disable so mouse pointer can see grid slots underneath

        // If it was already slotted on the board, wipe its data footprint to clear room
        if (isPlaced)
        {
            inventory.ClearItemData(currentGridX, currentGridY, this);
            isPlaced = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Smoothly translate position across your custom canvas scale factor
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    void Update()
    {
        // Using New Input System in a fixed frame loop ensures 100% responsive spam clicks
        if (isCurrentlyDragging)
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                RotateShape90Degrees();
            }
        }
    }

    private void RotateShape90Degrees()
    {
        int[] rotatedMatrix = new int[boundingWidth * boundingHeight];

        // Perform 2D matrix transformation on flat 1D array indices
        for (int y = 0; y < boundingHeight; y++)
        {
            for (int x = 0; x < boundingWidth; x++)
            {
                int newX = boundingHeight - 1 - y;
                int newY = x;
                rotatedMatrix[newY * boundingHeight + newX] = shapeMatrix[y * boundingWidth + x];
            }
        }

        // 1. Swap mathematical bounding dimensions for the grid logic
        int temp = boundingWidth;
        boundingWidth = boundingHeight;
        boundingHeight = temp;

        shapeMatrix = rotatedMatrix;

        // 2. Alter structural size of the parent calculation container to match the grid math
        rectTransform.sizeDelta = new Vector2(boundingWidth * inventory.cellSize, boundingHeight * inventory.cellSize);

        // 3. Rotate visual tracking data
        visualRotationAngle -= 90f;

        if (visualSpriteElement != null)
        {
            // Simply spin the child around its center point. 
            // Since it's anchored to the center and not stretching, it will never distort!
            visualSpriteElement.localRotation = Quaternion.Euler(0, 0, visualRotationAngle);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isCurrentlyDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Evaluate location using the true structural Top-Left corner of your shape container
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);
        Vector2 topLeftCornerOfItem = objectCorners[1];

        // Apply a safe internal offset buffer point so sample rates don't read directly on grid border lines
        float halfCell = inventory.cellSize / 2f;
        Vector2 evaluationPoint = new Vector2(topLeftCornerOfItem.x + halfCell, topLeftCornerOfItem.y - halfCell);

        if (inventory.ScreenPointToGridPos(evaluationPoint, out int gridX, out int gridY))
        {
            if (inventory.CheckCustomShapePlacement(gridX, gridY, this))
            {
                // Lock item structural state down into background index array positions
                inventory.PlaceCustomShapeData(gridX, gridY, this);
                currentGridX = gridX;
                currentGridY = gridY;
                isPlaced = true;

                SnapToGrid(gridX, gridY);
                return;
            }
        }

        // Return to original structural location if drop matrix requirements fail
        rectTransform.position = originalPosition;
        transform.SetParent(originalParent, true);
    }

    private void SnapToGrid(int gridX, int gridY)
    {
        transform.SetParent(inventory.transform, true);
        RectTransform gridRect = inventory.GetComponent<RectTransform>();

        // Precise anchor position mapping relative to inventory center coordinate rules
        float localX = (gridX * inventory.cellSize) + ((boundingWidth * inventory.cellSize) / 2f) - (gridRect.rect.width / 2f);
        float localY = (gridRect.rect.height / 2f) - (gridY * inventory.cellSize) - ((boundingHeight * inventory.cellSize) / 2f);

        rectTransform.localPosition = new Vector3(localX, localY, 0);
    }
}