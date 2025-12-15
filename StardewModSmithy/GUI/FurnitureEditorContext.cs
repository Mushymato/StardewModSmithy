using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;

namespace StardewModSmithy.GUI;

public enum DragMovementMode
{
    Sheet = 0,
    Bounds = 1,
}

internal sealed partial class FurnitureEditorContext(TextureAsset textureAsset, FurnitureAsset furnitureAsset)
{
    public const int ONE_TILE = 64;

    [Notify]
    private SDUISprite furnitureSheet = GetFurnitureSheet(textureAsset);

    public EnumSegmentsViewModel<DragMovementMode> MovementMode = new() { SelectedValue = DragMovementMode.Sheet };

    [Notify]
    public SDUIEdges furnitureSheetMargin = new(0, 0, 0, 0);

    [Notify]
    public float furnitureSheetOpacity = 1f;

    [Notify]
    public SDUIEdges selectionBoundsPadding = new(0, 0, 0, 0);

    public void UpdateSpriteIndex(SDUIEdges furniSheet, SDUIEdges selectBounds)
    {
        if (SelectedFurniture == null)
        {
            if (MovementMode.SelectedValue == DragMovementMode.Bounds)
                SelectionBoundsPadding = selectBounds;
            else
                FurnitureSheetMargin = furniSheet;
            return;
        }
        int xDelta = Math.Max(
            0,
            Math.Min(
                (selectBounds.Left - furniSheet.Left) / ONE_TILE,
                FurnitureSheet.IndexColCnt - SelectedFurniture.TilesheetSize.X
            )
        );
        int yDelta = Math.Max(
            0,
            Math.Min(
                (selectBounds.Top - furniSheet.Top) / ONE_TILE,
                FurnitureSheet.IndexRowCnt - SelectedFurniture.TilesheetSize.Y
            )
        );
        SelectedFurniture.SpriteIndex = yDelta * FurnitureSheet.IndexColCnt + xDelta;

        if (MovementMode.SelectedValue == DragMovementMode.Bounds)
        {
            SelectionBoundsPadding = new(furniSheet.Left + xDelta * ONE_TILE, furniSheet.Top + yDelta * ONE_TILE, 0, 0);
        }
        else
        {
            FurnitureSheetMargin = new(
                selectBounds.Left - xDelta * ONE_TILE,
                selectBounds.Top - yDelta * ONE_TILE,
                0,
                0
            );
        }
    }

    [DependsOn(nameof(FurnitureSheetMargin), nameof(SelectionBoundsPadding))]
    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) => delimStr.Name;

    private FurnitureDelimString? selectedFurniture;
    public FurnitureDelimString? SelectedFurniture
    {
        get => selectedFurniture;
        set
        {
            selectedFurniture = value;
            OnPropertyChanged(new(nameof(SelectedFurniture)));
            OnPropertyChanged(new(nameof(HasSelectedFurniture)));
            UpdateSpriteIndex(furnitureSheetMargin, selectionBoundsPadding);
        }
    }

    public bool HasSelectedFurniture => SelectedFurniture != null;

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
        FurnitureSheetOpacity = 0.7f;
    }

    public void SheetDrag(Vector2 position)
    {
        int newOffsetX;
        int newOffsetY;
        if (MovementMode.SelectedValue == DragMovementMode.Bounds)
        {
            newOffsetX = selectionBoundsPadding.Left;
            newOffsetY = selectionBoundsPadding.Top;
        }
        else
        {
            newOffsetX = furnitureSheetMargin.Left;
            newOffsetY = furnitureSheetMargin.Top;
        }

        Vector2 dragChange = position - lastDragPos;
        int dragTileCnt;
        bool changed = false;
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.X) / ONE_TILE) * ONE_TILE) > 0)
        {
            dragTileCnt *= Math.Sign(dragChange.X);
            newOffsetX += dragTileCnt;
            lastDragPos.X += dragTileCnt;
            changed = true;
        }
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.Y) / ONE_TILE) * ONE_TILE) > 0)
        {
            dragTileCnt *= Math.Sign(dragChange.Y);
            newOffsetY += dragTileCnt;
            lastDragPos.Y += dragTileCnt;
            changed = true;
        }

        if (changed)
        {
            if (MovementMode.SelectedValue == DragMovementMode.Bounds)
            {
                UpdateSpriteIndex(furnitureSheetMargin, new(newOffsetX, newOffsetY, 0, 0));
            }
            else
            {
                UpdateSpriteIndex(new(newOffsetX, newOffsetY, 0, 0), selectionBoundsPadding);
            }
        }
    }

    public void SheetDragEnd(Vector2 position)
    {
        lastDragPos = new(-1, -1);
        FurnitureSheetOpacity = 1f;
        if (SelectedFurniture?.SpriteIndex < 0)
        {
            SelectedFurniture.SpriteIndex = 0;
            SelectionBoundsPadding = new(FurnitureSheetMargin.Left, FurnitureSheetMargin.Top, 0, 0);
        }
    }

    public void SheetWheel()
    {
        MovementMode.SelectedValue =
            MovementMode.SelectedValue == DragMovementMode.Sheet ? DragMovementMode.Bounds : DragMovementMode.Sheet;
    }

    public void SheetWheelNull() { }
}
