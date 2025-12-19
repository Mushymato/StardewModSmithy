using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.GUI.ViewModels;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.GUI.EditorContext;

public enum DragMovementMode
{
    Sheet = 0,
    Bounds = 1,
}

internal sealed partial class DraggableTextureContext(TextureAsset textureAsset)
{
    internal event EventHandler<int>? Dragged;

    [Notify]
    private SDUISprite sheet = GetSheet(textureAsset);

    public EnumSegmentsViewModel<DragMovementMode> MovementMode = new() { SelectedValue = DragMovementMode.Bounds };

    [Notify]
    public SDUIEdges sheetMargin = new(0, 0, 0, 0);

    [Notify]
    public float sheetOpacity = 1f;

    [Notify]
    public SDUIEdges boundsPadding = new(0, 0, 0, 0);

    [Notify]
    public IBoundsProvider? boundsProvider = null;

    [Notify]
    public int spriteIndex = 0;

    private static SDUISprite GetSheet(TextureAsset textureAsset)
    {
        KeyValuePair<IAssetName, string> gatheredTx = textureAsset.GatheredTextures.First();
        Texture2D loadedTx = ModEntry.ModContent.Load<Texture2D>(gatheredTx.Value);
        return new(loadedTx, SourceRect: loadedTx.Bounds, FixedEdges: new(0), SliceSettings: new(Scale: 4))
        {
            AssetName = gatheredTx.Key,
        };
    }

    public void UpdateSpriteIndex(SDUIEdges newSheetMargin, SDUIEdges newBoundsPadding)
    {
        if (boundsProvider == null)
        {
            if (MovementMode.SelectedValue == DragMovementMode.Bounds)
                BoundsPadding = newBoundsPadding;
            else
                SheetMargin = newSheetMargin;
            return;
        }
        int xDelta = Math.Max(
            0,
            Math.Min(
                (newBoundsPadding.Left - newSheetMargin.Left) / Consts.ONE_TILE,
                Sheet.IndexColCnt - boundsProvider.TilesheetSize.X
            )
        );
        int yDelta = Math.Max(
            0,
            Math.Min(
                (newBoundsPadding.Top - newSheetMargin.Top) / Consts.ONE_TILE,
                Sheet.IndexRowCnt - boundsProvider.TilesheetSize.Y
            )
        );
        SpriteIndex = yDelta * Sheet.IndexColCnt + xDelta;

        if (MovementMode.SelectedValue == DragMovementMode.Bounds)
        {
            BoundsPadding = new(
                newSheetMargin.Left + xDelta * Consts.ONE_TILE,
                newSheetMargin.Top + yDelta * Consts.ONE_TILE,
                0,
                0
            );
        }
        else
        {
            SheetMargin = new(
                newBoundsPadding.Left - xDelta * Consts.ONE_TILE,
                newBoundsPadding.Top - yDelta * Consts.ONE_TILE,
                0,
                0
            );
        }
    }

    private Vector2 lastDragPos = new(-1, -1);

    public void SheetDragStart(Vector2 position)
    {
        lastDragPos = position;
        SheetOpacity = 0.7f;
    }

    public void SheetDrag(Vector2 position)
    {
        int newOffsetX;
        int newOffsetY;
        if (MovementMode.SelectedValue == DragMovementMode.Bounds)
        {
            newOffsetX = boundsPadding.Left;
            newOffsetY = boundsPadding.Top;
        }
        else
        {
            newOffsetX = sheetMargin.Left;
            newOffsetY = sheetMargin.Top;
        }

        Vector2 dragChange = position - lastDragPos;
        int dragTileCnt;
        bool changed = false;
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.X) / Consts.ONE_TILE) * Consts.ONE_TILE) > 0)
        {
            dragTileCnt *= Math.Sign(dragChange.X);
            newOffsetX += dragTileCnt;
            lastDragPos.X += dragTileCnt;
            changed = true;
        }
        if ((dragTileCnt = (int)(MathF.Abs(dragChange.Y) / Consts.ONE_TILE) * Consts.ONE_TILE) > 0)
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
                UpdateSpriteIndex(sheetMargin, new(newOffsetX, newOffsetY, 0, 0));
            }
            else
            {
                UpdateSpriteIndex(new(newOffsetX, newOffsetY, 0, 0), boundsPadding);
            }
        }
    }

    public void SheetDragEnd(Vector2 position)
    {
        lastDragPos = new(-1, -1);
        SheetOpacity = 1f;
        if (SpriteIndex < 0)
        {
            SpriteIndex = 0;
            BoundsPadding = new(SheetMargin.Left, SheetMargin.Top, 0, 0);
        }
        Dragged?.Invoke(this, SpriteIndex);
    }

    public void ToggleMovementMode()
    {
        MovementMode.SelectedValue =
            MovementMode.SelectedValue == DragMovementMode.Sheet ? DragMovementMode.Bounds : DragMovementMode.Sheet;
    }

    internal void OnEditorBoundsProviderChanged(object? sender, IBoundsProvider? e)
    {
        BoundsProvider = e;
        UpdateSpriteIndex(SheetMargin, BoundsPadding);
    }
}
