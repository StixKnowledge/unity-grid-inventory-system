# Unity Perfect-Fit Grid Inventory System

A puzzle-focused, Tetris-style grid inventory system built for Unity using the **New Input System** and **Unity UI (uGUI)**. Items must be organized to fit perfectly inside the inventory dimensions, with **zero empty cells allowed** before players can advance to the next level. It fully supports custom irregular shapes (like L-shaped pistols) and responsive right-click item rotation.

---

## 🚀 Key Features
* **Irregular Shape Support:** Define item matrices (e.g., L-shapes, T-shapes) beyond standard bounding box rectangles.
* **Responsive Mouse Input:** Rotates instantly via Right-Click during dragging, utilizing the Unity New Input System package.
* **Calculated Snap Alignment:** Snaps accurately to the grid based on the top-left structural orientation of the item rather than the offset mouse cursor position.
* **Strict Validation:** Features a complete grid analysis method (`IsInventoryPerfectlyFull()`) to verify a 100% full board for level progression.

---
<img width="1919" height="1024" alt="Screenshot 2026-05-29 135110" src="https://github.com/user-attachments/assets/048aca91-db32-4221-9211-0d760c0b76b4" />


## 🛠️ Unity Engine UI Setup Guide

Follow these sequential setup steps to integrate the scripts into your Unity scene layout:

### 1. Setting up the Inventory Grid Board
1. In your Hierarchy window, right-click and create a **UI > Panel**. Rename this object to `InventoryBoard`.
2. Attach the `GridInventory` component to `InventoryBoard`.
3. Configure the `GridInventory` fields in the Inspector:
   * **Grid Width**: `4`
   * **Grid Height**: `4`
   * **Cell Size**: `50`
4. Set the `InventoryBoard` RectTransform dimensions to match your mathematical boundaries perfectly: **Width: 200, Height: 200** ($4\text{ cells} \times 50\text{px}$).
5. *Optional Visual Gaps:* Temporarily add a **Grid Layout Group** component to `InventoryBoard` with a cell size of `50x50` and fill it with 16 square images to represent background tiles. **Crucial:** Disable or remove the Grid Layout Group component once the layout looks right so it doesn't hijack the manual script anchoring.

### 2. Creating a Draggable Item (Standard or Irregular)
To keep the mathematical calculation grid upright while letting your visual graphics rotate cleanly, use a decoupled parent-child object structure:

1. Right-click outside the inventoryBoard and select **UI > Image**. Rename it to your item name (e.g., `Item_Pistol`). 
2. Remove the `Image` component from this root parent object.
3. Attach the `DraggableItem` component and a `Canvas Group` component to this object.
4. Right-click your new item object and select **UI > Image** to create a nested child object. Name it `VisualSprite`.
   * Apply your actual texture/sprite artwork here.
   * Set its anchor configurations to **Stretch / Stretch** (Left: 0, Right: 0, Top: 0, Bottom: 0) to match its parent.
5. Head back to the parent item inspector and drag the child `VisualSprite` object into the **Visual Sprite Element** slot.

### 3. Applying Custom Shapes (e.g., Pistol Shape)
1. Select your item object (`Item_Pistol`).
2. Set the bounding limits in the `DraggableItem` inspector:
   * **Bounding Width:** `3`
   * **Bounding Height:** `2`
3. Change the root parent UI RectTransform scale to match those bounds: **Width: 150, Height: 100** ($3\text{x2 cells} \times 50\text{px}$).
4. Attach your shape definition script (e.g., `PistolShape`) alongside it. Ensure the matrix setup initializes on `Start()` matching your dimensions:
   ```csharp
   item.shapeMatrix = new int[] {
       1, 1, 1,
       1, 0, 0
   };

### 4. Hooking Up Level Progression Validation
1. Create a UI > Button named NextLevelButton.

2. Link your scene transitions or level management scripts to fire an event validation when clicked:

  ```csharp
  if (gridInventory.IsInventoryPerfectlyFull()) {
  
      // Logic to trigger loading your next scene index safely
      
  }


🗂️ Script Descriptions

🛡️ `GridInventory.cs`

> Role: Master Grid Controller

* Manages the internal 2D grid matrix backend (`bool[,]`).
* Handles the structural raycast evaluation from screen-space vectors to localized grid map arrays.
* Manages collision validation check flags before drops occur and verifies completion arrays.

📦 `DraggableItem.cs`

> Role: Drag-and-Drop & Input Handler

* Implements the `EventSystems` drag interfaces (`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`).
* Tracks active drag updates and handles spatial anchoring via top-left evaluation vectors.
* Processes visual-only element rotation tracking through the **Unity New Input System**.

🔫 `PistolShape.cs`

> Role: Custom Shape Definition

* An example configuration component used to initialize specialized block maps at runtime.
* Overrides standard block space arrays to configure custom $3 \times 2$ asymmetric dimensions.
