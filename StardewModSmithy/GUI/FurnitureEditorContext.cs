using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy.GUI;

internal sealed partial class FurnitureEditorContext(TextureAsset textureAsset, FurnitureAsset furnitureAsset)
{
    [Notify]
    private SDUISprite furnitureSheet = GetFurnitureSheet(textureAsset);

    [Notify]
    public SDUIEdges furnitureSheetMargin = new(0, 0, 0, 0);

    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) => delimStr.Name;

    [Notify]
    private FurnitureDelimString? selectedFurniture = null;

    public bool HasSelectedFurniture => selectedFurniture != null;

    public string FurnitureTilesheetArea
    {
        get
        {
            if (SelectedFurniture == null)
                return "0px 0px";
            return $"{SelectedFurniture.TilesheetSize.X * 64}px {SelectedFurniture.TilesheetSize.Y * 64}px";
        }
    }

    public IEnumerable<SDUIEdges> FurnitureBoundingSquares
    {
        get
        {
            if (SelectedFurniture == null)
                yield break;
            Point boundingBox = SelectedFurniture.BoundingBoxSize;
            Point tilesheetSize = SelectedFurniture.TilesheetSize;
            for (int x = 0; x < boundingBox.X; x++)
            {
                for (int y = 0; y < boundingBox.Y; y++)
                {
                    yield return new(x * 64, (tilesheetSize.Y - 1 - y) * 64);
                }
            }
        }
    }

    private static SDUISprite GetFurnitureSheet(TextureAsset textureAsset)
    {
        KeyValuePair<IAssetName, string> gatheredTx = textureAsset.GatheredTextures.First();
        Texture2D loadedTx = ModEntry.ModContent.Load<Texture2D>(gatheredTx.Value);
        return new(loadedTx, SourceRect: loadedTx.Bounds, FixedEdges: new(0), SliceSettings: new(Scale: 4))
        {
            AssetName = gatheredTx.Key,
        };
    }

    private Vector2 lastDragPos = new(-1, -1);

    public void SheetDragStart(Vector2 position)
    {
        lastDragPos = position;
    }

    private const int DRAG_STEP = 64;

    public void SheetDrag(Vector2 position)
    {
        Vector2 dragChange = position - lastDragPos;
        int newOffsetX = furnitureSheetMargin.Left;
        int newOffsetY = furnitureSheetMargin.Top;
        bool appliedChange = false;
        if (Math.Abs(dragChange.X) >= DRAG_STEP)
        {
            appliedChange = true;
            newOffsetX += Math.Sign(dragChange.X) * DRAG_STEP;
        }
        if (Math.Abs(dragChange.Y) >= DRAG_STEP)
        {
            appliedChange = true;
            newOffsetY += Math.Sign(dragChange.Y) * DRAG_STEP;
        }

        if (appliedChange)
        {
            lastDragPos = position;
        }
        FurnitureSheetMargin = new(newOffsetX, newOffsetY, 0, 0);
    }

    public void SheetDragEnd(Vector2 position)
    {
        lastDragPos = new(-1, -1);
    }
}
