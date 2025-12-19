using StardewModSmithy.Models;

namespace StardewModSmithy.GUI.EditorContext;

internal sealed partial class FurnitureAssetContext(FurnitureAsset furnitureAsset) : AbstractEditableAssetContext()
{
    public IReadOnlyList<FurnitureDelimString> FurnitureDataList => furnitureAsset.Editing.Values.ToList();

    public Func<FurnitureDelimString, string> FurnitureDataName = (delimStr) => delimStr.Name;

    public FurnitureDelimString? SelectedFurniture => (FurnitureDelimString?)BoundsProvider;

    public override void SetSpriteIndex(object? sender, int spriteIndex)
    {
        if (SelectedFurniture != null && spriteIndex >= 0)
        {
            SelectedFurniture.SpriteIndex = spriteIndex;
        }
    }
}
