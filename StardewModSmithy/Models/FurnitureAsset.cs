using System.Text;
using Microsoft.Xna.Framework;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;

namespace StardewModSmithy.Models;

public sealed partial class FurnitureDelimString(string Id)
{
    public const char DELIM = '/';
    #region options
    private static readonly string[] type_Options =
    [
        "armchair",
        "bench",
        "chair",
        "couch",
        "long table",
        "table",
        "fireplace",
        "lamp",
        "sconce",
        "torch",
        "window",
        "bed",
        "bed double",
        "bed child",
        "dresser",
        "fishtank",
        "bookcase",
        "decor",
        "painting",
        "randomized_plant",
        "rug",
        "other",
    ];
    private static readonly int[] rotation_Options = [1, 2, 4];
    private static readonly int[] placement_Options = [-1, 0, 1, 2];
#pragma warning disable CA1822 // Mark members as static
    public string[] Type_Options => type_Options;
    public int[] Rotation_Options => rotation_Options;
    public int[] Placement_Options => placement_Options;
#pragma warning restore CA1822 // Mark members as static
    #endregion

    #region fields
    [Notify]
    private string name = Id;

    public OptionedValue<string> TypeImpl { get; } = new(type_Options, "decor");
    public string Type
    {
        get => TypeImpl.Value;
        set
        {
            TypeImpl.Value = value;
            OnPropertyChanged(new(nameof(Type)));
        }
    }

    [Notify]
    public Point tilesheetSize = Point.Zero;
    public string TilesheetSizeName => $"{TilesheetSize.X} {TilesheetSize.Y}";

    [Notify]
    public Point boundingBoxSize = Point.Zero;
    public string BoundingBoxSizeName => $"{BoundingBoxSize.X} {BoundingBoxSize.Y}";

    public OptionedValue<int> RotationImpl { get; } = new(rotation_Options, 1);
    public int Rotation
    {
        get => RotationImpl.Value;
        set
        {
            RotationImpl.Value = value;
            OnPropertyChanged(new(nameof(Rotation)));
        }
    }

    [Notify]
    public uint price = 0;

    public OptionedValue<int> PlacementImpl { get; } = new(placement_Options, 1);
    public int Placement
    {
        get => PlacementImpl.Value;
        set
        {
            PlacementImpl.Value = value;
            OnPropertyChanged(new(nameof(Placement)));
        }
    }

    public LocalizedString DisplayNameImpl { get; set; } = new(string.Concat(Id, "name"));
    public string DisplayName
    {
        get => DisplayNameImpl.Value ?? "???";
        set
        {
            DisplayNameImpl.Value = value;
            OnPropertyChanged(new(nameof(DisplayName)));
        }
    }

    [Notify]
    public uint spriteIndex = 0;

    public IAssetName TextureAssetName { get; set; } = ModEntry.ParseAssetName("TileSheets/furniture");

    [Notify]
    public bool offLimitsForRandomSale = false;
    public HashSet<string> ContextTags { get; set; } = [];
    #endregion

    private static Point PointFromString(string str)
    {
        string[] parts = str.Split(' ');
        if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
        {
            return new(x, y);
        }
        return Point.Zero;
    }

    private static string PointToString(Point point)
    {
        return string.Concat(point.X, ' ', point.Y);
    }

    public static FurnitureDelimString? Deserialize(string id, string str)
    {
        string[] parts = str.Split(DELIM);
        if (parts.Length < 9)
        {
            ModEntry.Log($"Not enough '{DELIM}' delim parts {str}", LogLevel.Warn);
            return null;
        }

        FurnitureDelimString furniDelimString = new(id)
        {
            Name = parts[0],
            TilesheetSize = PointFromString(parts[2]),
            BoundingBoxSize = PointFromString(parts[3]),
        };

        furniDelimString.Type = parts[1];
        if (int.TryParse(parts[4], out int rotation))
        {
            furniDelimString.Rotation = rotation;
        }
        if (uint.TryParse(parts[5], out uint price))
        {
            furniDelimString.Price = price;
        }
        if (int.TryParse(parts[6], out int placement))
        {
            furniDelimString.Placement = placement;
        }
        if (LocalizedString.Deserialize(parts[7]) is LocalizedString displayName)
        {
            furniDelimString.DisplayNameImpl = displayName;
        }
        else
        {
            furniDelimString.DisplayName = parts[7];
        }
        if (uint.TryParse(parts[8], out uint spriteIndex))
        {
            furniDelimString.SpriteIndex = spriteIndex;
        }

        if (parts.Length > 9)
            furniDelimString.TextureAssetName = ModEntry.ParseAssetName(parts[9]);
        if (parts.Length > 10 && bool.TryParse(parts[10], out bool offlim))
            furniDelimString.OffLimitsForRandomSale = offlim;
        if (parts.Length > 11)
            furniDelimString.ContextTags = parts[11].Split(' ').ToHashSet();

        return furniDelimString;
    }

    private static readonly StringBuilder sb = new();

    public string Serialize()
    {
        sb.Append($"{{{{ModId}}}}_{Name}");
        sb.Append(DELIM);

        sb.Append(Type);
        sb.Append(DELIM);

        sb.Append(PointToString(TilesheetSize));
        sb.Append(DELIM);

        sb.Append(PointToString(BoundingBoxSize));
        sb.Append(DELIM);

        sb.Append(Rotation);
        sb.Append(DELIM);

        sb.Append(Price);
        sb.Append(DELIM);

        sb.Append(Placement);
        sb.Append(DELIM);

        sb.Append(DisplayNameImpl.GetToken());
        sb.Append(DELIM);

        sb.Append(SpriteIndex);
        sb.Append(DELIM);

        sb.Append(string.Concat(TextureAssetName.BaseName.Replace(DELIM, '\\')));
        sb.Append(DELIM);

        sb.Append(OffLimitsForRandomSale);
        sb.Append(DELIM);

        if (ContextTags != null)
        {
            sb.AppendJoin(' ', ContextTags.OrderBy(value => value));
        }

        string result = sb.ToString();
        sb.Clear();
        return result;
    }
}

public sealed class FurnitureAsset : IEditableAsset
{
    public string Target => "Data/Furniture";
    public string IncludeName => "furniture.json";
    public Dictionary<string, FurnitureDelimString> Editing = [];

    public Dictionary<string, object> GetData()
    {
        Dictionary<string, object> output = [];
        foreach ((string key, FurnitureDelimString furniDelim) in Editing)
        {
            output[$"{{{{ModId}}}}_{key}"] = furniDelim.Serialize();
        }
        return output;
    }
}
