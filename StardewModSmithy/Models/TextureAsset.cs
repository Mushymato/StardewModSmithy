using StardewModdingAPI;
using StardewModSmithy.Models.Interfaces;

namespace StardewModSmithy.Models;

public sealed class TextureAsset(string sourceDir, string group, Dictionary<IAssetName, string> gatheredTextures)
    : ILoadableAsset
{
    public string Group { get; set; } = group;

    public Dictionary<IAssetName, string> GatheredTextures { get; set; } = gatheredTextures;

    public string Target => string.Join(',', GatheredTextures.Keys.Select(target => target.BaseName));
    public string FromFile => Path.Join(Group, "{{ModId}}", "{{TargetWithoutPath}}");

    public static IAssetName FormAssetNameForGroup(string group, string fileName) =>
        ModEntry.ParseAssetName(Path.Join(group, "{{ModId}}", Path.GetFileNameWithoutExtension(fileName)));

    public static TextureAsset FromSourceDir(string sourceDir, string group)
    {
        Dictionary<IAssetName, string> gatheredTextures = [];
        foreach (string file in Directory.GetFiles(Path.Combine(ModEntry.DirectoryPath, sourceDir)))
        {
            if (!file.EndsWith(".png"))
                continue;
            string relFile = Path.GetRelativePath(ModEntry.DirectoryPath, file);
            ModEntry.Log(relFile);
            gatheredTextures[FormAssetNameForGroup(group, file)] = relFile;
        }
        return new TextureAsset(sourceDir, group, gatheredTextures);
    }

    public void StageFiles(string targetPath)
    {
        string targetGroupDir = Path.Combine(targetPath, Group);
        Directory.CreateDirectory(targetGroupDir);
        foreach ((IAssetName assetName, string sourceFile) in GatheredTextures)
        {
            File.Copy(
                Path.Combine(sourceDir, sourceFile),
                Path.Combine(targetGroupDir, Path.GetFileName(assetName.BaseName) + ".png")
            );
        }
    }
}
