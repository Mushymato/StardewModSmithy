using Microsoft.Xna.Framework;
using StardewModSmithy.Integration;

namespace StardewModSmithy.GUI.ViewModels;

internal interface IBoundsProvider
{
    public Point TilesheetSize { get; }
    public string GUI_TilesheetArea { get; }
    public Point BoundingBoxSize { get; }
    public IEnumerable<SDUIEdges> GUI_BoundingSquares { get; }
}
