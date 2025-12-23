using System.Diagnostics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.GUI;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.StardewModSmithy";
    private static IMonitor? mon;
    internal static Func<string, IAssetName> ParseAssetName = null!;
    internal static string DirectoryPath = null!;
    internal static IModContentHelper ModContent = null!;

    public static string ContentPatcherVersion { get; internal set; } = "2.8.0";

    internal const string EDITING_INPUT = "editing_input";
    internal const string EDITING_OUTPUT = "editing_output";

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        ParseAssetName = helper.GameContent.ParseAssetName;
        DirectoryPath = helper.DirectoryPath;
        ModContent = helper.ModContent;

        Directory.CreateDirectory(Path.Combine(DirectoryPath, EDITING_INPUT));
        Directory.CreateDirectory(Path.Combine(DirectoryPath, EDITING_OUTPUT));

        helper.ConsoleCommands.Add("sms-testy", "testy test", ConsoleTesty);
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        EditorMenuManager.Register(Helper);
        if (Helper.ModRegistry.Get("Pathoschild.ContentPatcher") is IManifest contentPatcher)
        {
            ContentPatcherVersion = contentPatcher.Version.ToString();
        }
    }

    private void ConsoleTesty(string cmd, string[] args)
    {
        OutputManifest manifest = new("Mock", "debug");
        OutputPackContentPatcher outputContentPatcher = new(manifest);
        TextureAssetGroup textureAsset = TextureAssetGroup.FromSourceDir(EDITING_INPUT, "furniture");
        outputContentPatcher.Load();
        outputContentPatcher.TextureAsset = textureAsset;
        if (outputContentPatcher.FurnitureAsset == null)
        {
            outputContentPatcher.FurnitureAsset = new();
            outputContentPatcher.FurnitureAsset.Editing["testyFurni1"] = FurnitureDelimString.Deserialize(
                "testyFurni1",
                "testyFurni1/rug/4 2/4 4/1/520/2/[LocalizedText {{ModId}}.i18n:decor.testyFurni1]/0/decor\\petals_pink\\{{ModId}}/false"
            )!;
            outputContentPatcher.FurnitureAsset.Editing["testyFurni2"] = FurnitureDelimString.Deserialize(
                "testyFurni1",
                "testyFurni2/rug/3 3/3 1/1/520/2/[LocalizedText {{ModId}}.i18n:decor.testyFurni2]/0/decor\\petals_white\\{{ModId}}/false"
            )!;
            outputContentPatcher.FurnitureAsset.SetTranslations(outputContentPatcher.Translations);
        }
        EditorMenuManager.ShowFurnitureEditor(textureAsset, outputContentPatcher.FurnitureAsset);
        Game1.activeClickableMenu.exitFunction = () =>
        {
            Log("SAVE");
            outputContentPatcher.Save();
        };

        // OutputManifest manifest = new("Mock", "debug");
        // TranslationStore? translations = TranslationStore.FromSourceDir(manifest.TranslationFolder);

        // FurnitureAsset furnitureAsset = new();
        // furnitureAsset.Editing["testyFurni1"] = FurnitureDelimString.Deserialize(
        //     "testyFurni1",
        //     "testyFurni1/rug/4 2/4 4/1/520/2/[LocalizedText {{ModId}}.i18n:decor.petals_pink]/0/decor\\petals_pink\\{{ModId}}/false"
        // )!;
        // furnitureAsset.Editing["testyFurni2"] = FurnitureDelimString.Deserialize(
        //     "testyFurni1",
        //     "testyFurni2/rug/3 3/3 1/1/520/2/[LocalizedText {{ModId}}.i18n:decor.petals_pink]/0/decor\\petals_white\\{{ModId}}/false"
        // )!;
        // furnitureAsset.SetTranslations(translations);

        // TextureAssetGroup textureAsset = TextureAssetGroup.FromSourceDir(EDITING_INPUT, "furniture");

        // EditorMenuManager.ShowFurnitureEditor(textureAsset, furnitureAsset);
    }

    public static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    internal static void WriteJson(string targetPath, string fileName, object content)
    {
        File.WriteAllText(
            Path.Combine(targetPath, fileName),
            JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings)
        );
    }

    internal static T? ReadJson<T>(string targetPath, string fileName)
    {
        string targetFile = Path.Combine(targetPath, fileName);
        if (!File.Exists(targetFile))
            return default;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(targetFile));
    }

    /// <summary>SMAPI static monitor Log wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void Log(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }

    /// <summary>SMAPI static monitor LogOnce wrapper</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    internal static void LogOnce(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.LogOnce(msg, level);
    }

    /// <summary>SMAPI static monitor Log wrapper, debug only</summary>
    /// <param name="msg"></param>
    /// <param name="level"></param>
    [Conditional("DEBUG")]
    internal static void LogDebug(string msg, LogLevel level = DEFAULT_LOG_LEVEL)
    {
        mon!.Log(msg, level);
    }
}
