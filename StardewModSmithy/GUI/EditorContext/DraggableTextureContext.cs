using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
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

public partial class DraggableTextureContext(TextureAssetGroup textureAssetGroup)
{
    public event EventHandler<int>? Dragged;
    public event EventHandler<TextureAsset>? TextureChanged;

    [Notify]
    public TextureAsset selected = textureAssetGroup.GatheredTextures.First().Value;

    public TextureAsset? SelectedFront
    {
        get => Selected.Front;
        set
        {
            Selected.Front = value;
            foreach (TextureAsset asset in textureAssetGroup.GatheredTextures.Values)
            {
                asset.IsSelectedFront = asset == Selected.Front;
            }
            OnPropertyChanged(new(nameof(SelectedFront)));
        }
    }

    public SDUISprite Sheet => Selected.UISprite;

    [DependsOn(nameof(SelectedFront))]
    public bool HasSheetFront => SelectedFront != null;

    [DependsOn(nameof(SelectedFront))]
    public SDUISprite? SheetFront => SelectedFront?.UISprite;

    public EnumSegmentsViewModel<DragMovementMode> MovementMode = new() { SelectedValue = DragMovementMode.Bounds };

    [Notify]
    public SDUIEdges sheetMargin = new(0, 0, 0, 0);

    [Notify]
    public float sheetOpacity = 1f;

    public float SheetOpacityFront => SheetOpacity * 0.5f;

    [Notify]
    public SDUIEdges boundsPadding = new(0, 0, 0, 0);

    [Notify]
    private IBoundsProvider? boundsProvider = null;

    [Notify]
    public int spriteIndex = 0;

    [Notify]
    private bool showingTextureSelector = false;

    public IEnumerable<TextureAsset> Textures => textureAssetGroup.GatheredTextures.Values;

    public void SelectTextureAsset(TextureAsset selectedAsset)
    {
        foreach (TextureAsset asset in textureAssetGroup.GatheredTextures.Values)
        {
            asset.IsSelected = asset == selectedAsset;
            asset.IsSelectedFront = asset == selectedAsset.Front;
        }
        selectedAsset.IsSelectedFront = false;
        Selected = selectedAsset;
        TextureChanged?.Invoke(this, selectedAsset);
    }

    public void SelectTextureAssetFront(TextureAsset selectedAsset)
    {
        if (!textureAssetGroup.EnableFront)
            return;
        if (selectedAsset.IsSelected)
            return;
        if (selectedAsset == SelectedFront)
        {
            SelectedFront = null;
        }
        else
        {
            SelectedFront = selectedAsset;
        }
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

    public void SetSpriteIndexForBoundsProvider(IBoundsProvider? boundsProvider)
    {
        if (boundsProvider == null)
            return;
        SpriteIndex = boundsProvider.SpriteIndex;
        int xPos = SpriteIndex % Sheet.IndexColCnt;
        int yPos = SpriteIndex / Sheet.IndexColCnt;

        if (MovementMode.SelectedValue == DragMovementMode.Bounds)
        {
            BoundsPadding = new(
                SheetMargin.Left + xPos * Consts.ONE_TILE,
                SheetMargin.Top + yPos * Consts.ONE_TILE,
                0,
                0
            );
        }
        else
        {
            SheetMargin = new(
                BoundsPadding.Left - xPos * Consts.ONE_TILE,
                BoundsPadding.Top - yPos * Consts.ONE_TILE,
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

    public void OnEditorBoundsProviderChanged(object? sender, IBoundsProvider? e)
    {
        BoundsProvider = e;
        SetSpriteIndexForBoundsProvider(e);
        if (e == null)
            return;
        if (
            e.TextureAssetName != null
            && textureAssetGroup.GatheredTextures.TryGetValue(e.TextureAssetName, out TextureAsset? desiredAsset)
        )
        {
            SelectTextureAsset(desiredAsset);
        }
        else
        {
            e.TextureAssetName = Selected.AssetName;
        }
    }

    public void ToggleTextureSelector()
    {
        ShowingTextureSelector = !ShowingTextureSelector;
    }
}
