# Unity Perfect-Fit Grid Inventory System

A puzzle-focused, Tetris-style grid inventory system built for Unity using the **New Input System** and **Unity UI (uGUI)**. Items must be organized to fit perfectly inside the inventory dimensions, with **zero empty cells allowed** before players can advance to the next level. 

This repository leverages **ScriptableObjects** and a **Custom Grid Inspector Tool** to design irregular asset structures (like L-shaped pistols or T-shaped blocks) completely code-free.

---

## 🗂️ Script Descriptions

### 🛡️ `GridInventory.cs`
> **Role:** Master Grid Controller
* Manages the internal 2D grid matrix backend (`bool[,]`).
* Handles the structural raycast evaluation from screen-space vectors to localized grid map arrays.
* Manages collision validation check flags before drops occur and verifies completion arrays (`IsInventoryPerfectlyFull()`).

### 📦 `DraggableItem.cs`
> **Role:** Drag-and-Drop & Input Handler
* Implements the `EventSystems` drag interfaces (`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`).
* Tracks active drag updates and handles spatial anchoring via top-left evaluation vectors rather than the offset mouse pointer.
* Processes visual-only element rotation tracking through the **Unity New Input System** in a responsive frame loop.

### 📄 `ItemShapeData.cs`
> **Role:** Data Model Asset
* The data model ScriptableObject backend. 
* Holds structural bounding box configurations and flat array layouts. 
* Instances of this asset represent individual item form-factors dynamically loaded at runtime.

### 🛠️ `ItemShapeDataEditor.cs` (Located in `/Editor` folder)
> **Role:** Custom Shape Developer Tool
* A developer workflow extension script that overrides the standard inspector rendering.
* Replaces the messy default array listing with an intuitive, visual 2D matrix layout of clickable checkboxes.

---

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
5. *Optional Visual Gaps:* Add a **Grid Layout Group** component to `InventoryBoard` with a cell size of `50x50` and populate it with 16 square images to represent background tiles. **Crucial:** Disable or remove the Grid Layout Group component once the layout looks right so it doesn't hijack the manual script anchoring.

### 2. Designing an Item Shape Asset
Thanks to the Custom Editor, you can draw your shape profiles visually:
1. In your project files, right-click and choose **Create > Inventory > Item Shape**. Name it `Shape_Pistol`.
2. Select the asset. In the Inspector, define your outer boundaries:
   * **Bounding Width:** `3`
   * **Bounding Height:** `2`
3. Click the interactive checkbox grid to model your shape footprint:
   ```text
   [ ✅ ] [ ✅ ] [ ✅ ]  <-- Top barrel line (1, 1, 1)
   [ ✅ ] [ 🔲 ] [ 🔲 ]  <-- Lower grip line   (1, 0, 0)

### 3. Creating a Draggable UI Item Component
To keep the structural bounding calculations upright while letting your graphic animations spin freely, use a decoupled parent-child hierarchy layout:

1. Right-click outside the board and select UI > Image. Rename it to your item name (e.g., Item_Pistol).

2. Remove the Image component from this root parent object.

3. Attach the DraggableItem component and a Canvas Group component to this object.

4. Right-click your new item object and select UI > Image to create a nested child object. Name it VisualSprite.

    * Apply your actual texture/sprite artwork here.
  
    * Set its anchor configurations to Stretch / Stretch (Left: 0, Right: 0, Top: 0, Bottom: 0).

5. Head back to the parent item inspector:

    * Drag your custom scriptable asset (Shape_Pistol) into the Shape Data field.
  
    * Drag your child VisualSprite object into the Visual Sprite Element slot.

🎮 How It Functions In-Game

  * Drag & Drop: Pick up items with Left-Click. The system handles raycast sampling from the item's true upper-left corner bounding region. It automatically snaps directly into position upon grid clearance.
  
  * Responsive Rotation: Tap Right-Click while dragging to instantly shift the orientation. The logic maps structural boundary swaps while visually spinning the child sprite element, keeping the data layer completely aligned.
  
  * Puzzle Check Validation: Hook your UI Level Progression buttons to verify the board array parameters. If any single slot reads false (empty space), progression stays locked!
