using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.ViewModels;

public interface IBoundsProvider
{
    public int SpriteIndex { get; }
    public Point TilesheetSize { get; }
    public Point BoundingBoxSize { get; }
    public string GUI_TilesheetArea { get; }
    public IEnumerable<SDUIEdges> GUI_BoundingSquares { get; }
    public IAssetName? TextureAssetName { get; set; }
}
