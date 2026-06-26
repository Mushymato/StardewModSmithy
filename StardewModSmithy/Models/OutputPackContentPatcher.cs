using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Models.ValueKinds;
using StardewModSmithy.Wheels;
using StardewValley.Extensions;

namespace StardewModSmithy.Models;

#pragma warning disable CA1822, CS0649
public interface IMockPatch
{
    public string Action { get; }
    public Dictionary<string, object>? When { get; set; }
}

public sealed record MockEditData(string Target, Dictionary<string, object> Entries) : IMockPatch
{
    public string Action => "EditData";
    public Dictionary<string, object>? When { get; set; }
    public string[]? TargetField { get; set; }
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

internal record MockContent(List<IMockPatch> Changes)
{
    [JsonProperty("$schema")]
    public string JsonSchema => "https://smapi.io/schemas/content-patcher.json";
}

internal sealed record MockContentMain(List<IMockPatch> Changes) : MockContent(Changes)
{
    public string Format => ModEntry.ContentPatcherVersion;
    public object? ConfigSchema { get; set; }
    public object? DynamicTokens { get; set; }
}

internal sealed class MockContentPrior
{
    public object? Format;
    public object? ConfigSchema;
    public object? DynamicTokens;
    public object? Changes;
}

internal record MockContentEditDataInclude(List<MockEditData> Changes);
#pragma warning restore CA1822, CS0649

public sealed class OutputPackContentPatcher : IOutputPack
{
    public const string PackFor = "Pathoschild.ContentPatcher";
    public OutputManifest Manifest { get; private set; }
    public TranslationStore? Translations;

    public OutputPackContentPatcher(OutputManifest manifest)
    {
        Manifest = manifest;
        Manifest.PackFor = PackFor;
        Translations = new TranslationStore(manifest.TranslationFolder);
    }

    public TextureAssetGroup? TxAssetGroup { get; set; } = null;
    public FurnitureAsset? FurniAsset { get; set; } = null;
    public WallpaperFlooringAsset? WallAndFloorAsset { get; set; } = null;

    internal MockContentPrior? PriorContent = null;

    public IEnumerable<ILoadableAsset> LoadableAssets
    {
        get
        {
            if (TxAssetGroup is not null)
                yield return TxAssetGroup;
        }
    }
    public IEnumerable<IEditableAsset> EditableAssets
    {
        get
        {
            if (FurniAsset is not null)
                yield return FurniAsset;
            if (WallAndFloorAsset is not null)
                yield return WallAndFloorAsset;
        }
    }

    public void Save()
    {
        string targetPath = Manifest.OutputFolder;
        Directory.CreateDirectory(targetPath);

        Manifest.PackFor = PackFor;

        string dataDir = Path.Combine(targetPath, Utils.DATA_DIR);
        string assetsDir = Path.Combine(targetPath, Utils.ASSETS_DIR);

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
            bool translationRequiresLoad = false;
            foreach (IEditableAsset editable in EditableAssets)
            {
                translationRequiresLoad =
                    editable.GetTranslations(ref Translations, Manifest.Name) || translationRequiresLoad;
            }
            if (translationRequiresLoad)
            {
                changes.Add(
                    new MockLoad(
                        TranslationString.I18N_Asset,
                        Path.Combine(Utils.TL_DIR, TranslationStore.DefaultFilename)
                    )
                    {
                        Priority = AssetLoadPriority.Low.ToString(),
                    }
                );
                changes.Add(
                    new MockLoad(TranslationString.I18N_Asset, Path.Combine(Utils.TL_DIR, "{{Language}}.json"))
                    {
                        When = new() { ["HasFile:{{FromFile}}"] = true },
                    }
                );
            }
            Translations.WriteI18NData();
        }
        // edits
        HashSet<IAssetName> requiredAssets = [];
        foreach (IEditableAsset editable in EditableAssets)
        {
            List<IMockPatch> patches = editable.GetPatches(this).ToList();
            if (patches.Count == 0)
            {
                File.Delete(Path.Combine(dataDir, editable.IncludeName));
                continue;
            }
            changes.Add(new MockInclude(Path.Combine(Utils.DATA_DIR, editable.IncludeName)));
            ModEntry.WriteJson(dataDir, editable.IncludeName, new MockContent(patches));
            requiredAssets.AddRange(editable.GetRequiredAssets());
        }
        // loads
        foreach (ILoadableAsset loadable in LoadableAssets)
        {
            if (
                loadable.StageAndGetTargetAndFromFile(assetsDir, ref requiredAssets)
                is ValueTuple<string, string> result
            )
            {
                changes.Add(new MockLoad(result.Item1, Path.Join(Utils.ASSETS_DIR, result.Item2)));
            }
        }

        List<string> includeList = [];
        foreach (IMockPatch mockPatch in changes)
        {
            if (mockPatch is MockInclude inc)
            {
                includeList.Add(inc.FromFile);
            }
        }
        Manifest.StardewModSmithyInfo.Generated = [Utils.MANIFEST_FILE, Utils.CONTENT_JSON, .. includeList];
        Manifest.StardewModSmithyInfo.Custom = [];
        string customDir = Path.Combine(targetPath, Utils.CUSTOM_DIR);
        if (Directory.Exists(customDir))
        {
            List<string> customIncludes = [];
            foreach (string file in Directory.GetFiles(customDir))
            {
                if (!file.EndsWith(".json"))
                    continue;
                string fileName = Path.GetFileName(file);
                customIncludes.Add(Path.Combine(Utils.CUSTOM_DIR, fileName));
                Manifest.StardewModSmithyInfo.Custom.Add(fileName);
            }
            changes.Add(new MockInclude(string.Join(',', customIncludes)));
        }
        else
        {
            Directory.CreateDirectory(customDir);
        }
        if (changes.Count == 0)
        {
            // ensure that content patcher will try to load this as pack
            changes.Add(new MockInclude("StardewModSmithy PLACEHOLDER"));
        }

        // content.json
        ModEntry.WriteJson(
            targetPath,
            Utils.CONTENT_JSON,
            new MockContentMain(changes)
            {
                ConfigSchema = PriorContent?.ConfigSchema,
                DynamicTokens = PriorContent?.DynamicTokens,
            }
        );
        // manifest.json
        Manifest.Save();

        if (Utils.StageByCopy)
        {
            string stagingDir = Path.Combine(ModEntry.StagingDirectoryPath, Path.GetFileName(Manifest.OutputFolder));
            try
            {
                if (Directory.Exists(stagingDir))
                {
                    string configFile = Path.Combine(stagingDir, Utils.CONFIG_JSON);
                    if (File.Exists(configFile))
                    {
                        File.Move(configFile, Path.Combine(Manifest.OutputFolder, Utils.CONFIG_JSON), overwrite: true);
                    }
                    Directory.Delete(stagingDir, true);
                }
                Utils.CopyDirectory(Manifest.OutputFolder, stagingDir, true);
            }
            catch (Exception err)
            {
                ModEntry.Log($"Failed to copy '{Manifest.OutputFolder}' to '{stagingDir}'\n{err}", LogLevel.Warn);
            }
        }

        ModEntry.PatchReload(Manifest.UniqueID);
    }

    public void Load()
    {
        string targetPath = Manifest.OutputFolder;

        string dataDir = Path.Combine(targetPath, Utils.DATA_DIR);
        if (!Directory.Exists(dataDir))
            return;

        PriorContent = ModEntry.ReadJson<MockContentPrior>(Path.Combine(targetPath, Utils.CONTENT_JSON));

        foreach (string file in Directory.GetFiles(dataDir))
        {
            if (!file.EndsWith(".json"))
            {
                continue;
            }
            string fileName = Path.GetFileName(file);
            if (
                ModEntry.ReadJson<MockContentEditDataInclude>(dataDir, fileName)
                    is not MockContentEditDataInclude editDataContent
                || editDataContent.Changes.FirstOrDefault(patch => patch is not null) is not MockEditData editData
            )
            {
                continue;
            }
            switch (fileName)
            {
                case FurnitureAsset.DEFAULT_INCLUDE_NAME:
                    InitializeFurnitureAsset(editData.Entries);
                    break;
                case WallpaperFlooringAsset.DEFAULT_INCLUDE_NAME:
                    InitializeWallpaperAndFlooringAsset(editData.Entries);
                    break;
            }
            continue;
        }
    }

    public void InitializeFurnitureAsset(Dictionary<string, object> entries)
    {
        FurniAsset = new();
        FurniAsset.SetData(entries);
        FurniAsset.SetTranslations(Translations);
    }

    public void InitializeWallpaperAndFlooringAsset(Dictionary<string, object> entries)
    {
        WallAndFloorAsset = new();
        WallAndFloorAsset.SetData(entries);
        WallAndFloorAsset.SetTranslations(Translations);
    }
}
