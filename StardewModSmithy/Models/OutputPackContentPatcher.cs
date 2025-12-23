using System.Text;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;
using StardewValley.Extensions;

namespace StardewModSmithy.Models;

public interface IMockPatch
{
    public string Action { get; }
    public Dictionary<string, object>? When { get; set; }
}

public sealed record MockEditData(string Target, Dictionary<string, object> Entries) : IMockPatch
{
    public string Action => "EditData";
    public Dictionary<string, object>? When { get; set; }
}

public record MockLoad(string Target, string FromFile) : IMockPatch
{
    public string Action => "Load";
    public Dictionary<string, object>? When { get; set; }

    public string Priority { get; set; } = AssetLoadPriority.Medium.ToString();
}

public sealed record MockInclude(string FromFile) : IMockPatch
{
    public string Action => "Include";
    public Dictionary<string, object>? When { get; set; }
}

internal record MockContent(List<IMockPatch> Changes);

internal record MockContentFurniture(List<MockEditData> Changes);

internal sealed record MockContentMain(List<IMockPatch> Changes) : MockContent(Changes)
{
#pragma warning disable CA1822 // Mark members as static
    public string Format => ModEntry.ContentPatcherVersion;
#pragma warning restore CA1822 // Mark members as static
}

public sealed class OutputPackContentPatcher(OutputManifest manifest) : IOutputPack
{
    public const string PackFor = "Pathoschild.ContentPatcher";

    public TextureAssetGroup? TextureAsset { get; set; } = null;
    public FurnitureAsset? FurnitureAsset { get; set; } = null;

    public IEnumerable<ILoadableAsset> LoadableAssets
    {
        get
        {
            if (TextureAsset is not null)
                yield return TextureAsset;
        }
    }
    public IEnumerable<IEditableAsset> EditableAssets
    {
        get
        {
            if (FurnitureAsset is not null)
                yield return FurnitureAsset;
        }
    }

    public TranslationStore? Translations = TranslationStore.FromSourceDir(manifest.TranslationFolder);

    public void Save()
    {
        string targetPath = manifest.OutputFolder;
        Directory.CreateDirectory(targetPath);

        manifest.PackFor = PackFor;

        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
        string translationsDir = manifest.TranslationFolder;

        if (Directory.Exists(dataDir))
            Directory.Delete(dataDir, true);
        Directory.CreateDirectory(dataDir);
        if (Directory.Exists(assetsDir))
            Directory.Delete(assetsDir, true);
        Directory.CreateDirectory(assetsDir);

        List<IMockPatch> changes = [];
        // translations
        if (Translations != null)
        {
            Directory.CreateDirectory(translationsDir);
            bool translationRequiresLoad = false;
            foreach (IEditableAsset editable in EditableAssets)
            {
                translationRequiresLoad = editable.GetTranslations(ref Translations) || translationRequiresLoad;
            }
            if (translationRequiresLoad)
            {
                changes.Add(
                    new MockLoad(TranslationString.I18N_Asset, Path.Combine("i18n", TranslationStore.DefaultFilename))
                    {
                        Priority = AssetLoadPriority.Low.ToString(),
                    }
                );
                changes.Add(
                    new MockLoad(TranslationString.I18N_Asset, Path.Combine("i18n", Translations.LocaleFilename))
                    {
                        When = new() { ["HasFile:{{FromFile}}"] = true },
                    }
                );
            }
            // i18n/{langaugecode}.json and i18n/default.json
            ModEntry.WriteJson(translationsDir, Translations.LocaleFilename, Translations.Data);
            ModEntry.WriteJson(translationsDir, TranslationStore.DefaultFilename, Translations.Data);
        }
        // edits
        List<string> descList = [];
        HashSet<IAssetName> requiredAssets = [];
        foreach (IEditableAsset editable in EditableAssets)
        {
            changes.Add(new MockInclude(Path.Combine("data", editable.IncludeName)));
            ModEntry.WriteJson(
                dataDir,
                editable.IncludeName,
                new MockContent([new MockEditData(editable.Target, editable.GetData())])
            );
            descList.Add(editable.Desc);
            requiredAssets.AddRange(editable.GetRequiredAssets());
        }
        manifest.Desc = string.Join(" and ", descList);
        // loads
        foreach (ILoadableAsset loadable in LoadableAssets)
        {
            if (
                loadable.StageAndGetTargetAndFromFile(assetsDir, ref requiredAssets)
                is ValueTuple<string, string> result
            )
            {
                changes.Add(new MockLoad(result.Item1, Path.Join("assets", result.Item2)));
            }
        }

        // content.json
        ModEntry.WriteJson(targetPath, "content.json", new MockContentMain(changes));
        // manifest.json
        ModEntry.WriteJson(targetPath, "manifest.json", manifest);
    }

    public void Load()
    {
        string targetPath = manifest.OutputFolder;

        string dataDir = Path.Combine(targetPath, "data");
        if (!Directory.Exists(dataDir))
            return;

        foreach (string file in Directory.GetFiles(dataDir))
        {
            if (!file.EndsWith(".json"))
            {
                continue;
            }
            string fileName = Path.GetFileName(file);
            if (Path.GetFileName(file) != FurnitureAsset.DefaultIncludeName)
            {
                continue;
            }
            if (
                ModEntry.ReadJson<MockContentFurniture>(dataDir, fileName) is not MockContentFurniture furnitureContent
                || furnitureContent.Changes.FirstOrDefault(patch => patch is not null) is not MockEditData editData
            )
            {
                continue;
            }
            FurnitureAsset = new();
            FurnitureAsset.SetData(editData.Entries);
            FurnitureAsset.SetTranslations(Translations);
            break;
        }
    }
}
