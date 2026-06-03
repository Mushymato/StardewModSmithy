using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models;

public record TxAtlasEntry(string RelPath, Rectangle Area);

public sealed partial record TextureAsset(IAssetName AssetName, string PathOnDisk)
{
    private Texture2D? texture = null;
    public Texture2D Texture
    {
        get
        {
            texture ??= ModEntry.ModContent.Load<Texture2D>(PathOnDisk);
            return texture;
        }
    }

    public List<TxAtlasEntry>? TextureAtlas = TryGetTextureAtlas(PathOnDisk);

    private static List<TxAtlasEntry>? TryGetTextureAtlas(string PathOnDisk)
    {
        string atlasPath = Path.Combine(
            ModEntry.DirectoryPath,
            Path.GetDirectoryName(PathOnDisk) ?? "",
            string.Concat(Path.GetFileNameWithoutExtension(PathOnDisk), Utils.ATLAS_SUFFIX)
        );
        if (!File.Exists(atlasPath))
            return null;
        return ModEntry.ReadJson<List<TxAtlasEntry>>(atlasPath);
    }

    public SDUISprite GetUISprite(float scale) =>
        new(Texture, SourceRect: Texture.Bounds, FixedEdges: new(0), SliceSettings: new(Scale: scale))
        {
            AssetName = AssetName,
        };

    public TextureAsset? Front = null;

    public SDUISprite UISprite => GetUISprite(4);
    public SDUISprite UISpriteSmall => GetUISprite(1);

    public string UILabel =>
        TextureAtlas == null
            ? Path.GetFileName(PathOnDisk)
            : string.Concat(Path.GetFileNameWithoutExtension(PathOnDisk), I18n.Gui_Tooltip_TxAtlas(TextureAtlas.Count));
    private static readonly StringBuilder sb = new();
    public string UITooltip
    {
        get
        {
            sb.Append(UILabel);
            sb.Append('\n');
            sb.Append(I18n.Gui_Tooltip_TxSize(Texture.Width, Texture.Height));
            if (Front != null)
            {
                sb.Append('\n');
                sb.Append(I18n.Gui_Tooltip_TxFront(Front.UILabel));
            }
            string tooltip = sb.ToString();
            sb.Clear();
            return tooltip;
        }
    }

    [Notify]
    public bool isSelected = false;

    [Notify]
    public bool isSelectedFront = false;
}

public sealed class TextureAssetGroup() : ILoadableAsset
{
    private const string FRONT = "front.json";
    private Dictionary<IAssetName, TextureAsset>? gatheredTextures = null;
    public Dictionary<IAssetName, TextureAsset> GatheredTextures => gatheredTextures ??= FormGatheredTextures();

    private static Dictionary<IAssetName, TextureAsset> FormGatheredTextures()
    {
        Dictionary<IAssetName, TextureAsset> newlyGathered = [];
        foreach (string dir in Directory.GetDirectories(ModEntry.InputDirectoryPath))
        {
            if (File.Exists(string.Concat(dir, Utils.ATLAS_SUFFIX)))
                continue;
            SpritePacker.Pack(dir);
        }
        foreach (string file in Directory.GetFiles(ModEntry.InputDirectoryPath))
        {
            if (!file.EndsWith(".png"))
                continue;
            string relFile = Path.GetRelativePath(ModEntry.DirectoryPath, file);
            SpritePacker.Normalize(relFile);
            IAssetName assetName = FormAssetName(file);
            newlyGathered[assetName] = new(assetName, relFile);
        }
        if (
            ModEntry.ReadJson<Dictionary<string, string>>(ModEntry.InputDirectoryPath, FRONT)
            is Dictionary<string, string> fronts
        )
        {
            foreach ((string baseName, string frontName) in fronts)
            {
                IAssetName baseAssetName = ModEntry.ParseAssetName(baseName);
                IAssetName frontAssetName = ModEntry.ParseAssetName(frontName);
                if (
                    newlyGathered.TryGetValue(baseAssetName, out TextureAsset? baseAsset)
                    && newlyGathered.TryGetValue(frontAssetName, out TextureAsset? frontAsset)
                )
                {
                    baseAsset.Front = frontAsset;
                }
            }
        }
        if (newlyGathered.Any())
        {
            newlyGathered.First().Value.IsSelected = true;
        }
        return newlyGathered;
    }

    public void Invalidate()
    {
        gatheredTextures = null;
    }

    public ValueTuple<string, string>? StageAndGetTargetAndFromFile(
        string targetPath,
        ref HashSet<IAssetName> requiredAssets
    )
    {
        Dictionary<string, string> frontInfo = [];
        StringBuilder targetSB = new();
        foreach (IAssetName key in requiredAssets.Reverse())
        {
            if (!GatheredTextures.TryGetValue(key, out TextureAsset? txAsset))
            {
                continue;
            }
            File.Copy(
                Path.Combine(ModEntry.DirectoryPath, txAsset.PathOnDisk),
                Path.Combine(targetPath, Path.GetFileName(key.BaseName) + ".png")
            );
            targetSB.Append(',');
            targetSB.Append(key.BaseName);
            requiredAssets.Remove(key);
            if (txAsset.Front is TextureAsset txAssetFront)
            {
                File.Copy(
                    Path.Combine(ModEntry.DirectoryPath, txAssetFront.PathOnDisk),
                    Path.Combine(targetPath, Path.GetFileName(key.BaseName) + "Front.png")
                );
                targetSB.Append(',');
                targetSB.Append(key.BaseName);
                targetSB.Append("Front");
                frontInfo[key.BaseName] = txAssetFront.AssetName.BaseName;
            }
        }
        ModEntry.WriteJson(Path.Combine(ModEntry.InputDirectoryPath, FRONT), frontInfo);
        if (targetSB.Length > 1)
        {
            targetSB.Remove(0, 1);
            return new(targetSB.ToString(), "{{TargetWithoutPath}}.png");
        }
        return null;
    }

    public static IAssetName FormAssetName(string fileName) =>
        ModEntry.ParseAssetName(Path.Join("{{ModId}}", Path.GetFileNameWithoutExtension(fileName)));
}
