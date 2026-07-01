using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModSmithy.GUI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewModSmithy.Wheels;
using StardewValley;
using StardewValley.Menus;

namespace StardewModSmithy;

public sealed class ModEntry : Mod
{
#if DEBUG
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Debug;
#else
    private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Trace;
#endif

    public const string ModId = "mushymato.StardewModSmithy";
    private const string IconBaseTexture = $"{ModId}/icon/base";
    private const string IconHoverTexture = $"{ModId}/icon/hover";

    private static IMonitor? mon;
    internal static Func<string, IAssetName> ParseAssetName = null!;
    internal static string DirectoryPath = null!;
    internal static string StagingDirectoryPath = null!;
    internal static string InputDirectoryPath = null!;
    internal static string OutputDirectoryPath = null!;
    internal static string ModCreditString = null!;
    internal static IModContentHelper ModContent = null!;
    internal static IGameContentHelper GameContent = null!;
    internal static ModConfig Config = null!;
    internal static IModRegistry ModRegistry = null!;

    // https://gist.github.com/Shockah/ec111245868ee9b7dbf2ca2928dd2896
    #region execute command
    private static Action<string>? AddToRawCommandQueue = null;

    private static Action<string>? Make_AddToRawCommandQueue()
    {
        var scoreType = Type.GetType("StardewModdingAPI.Framework.SCore, StardewModdingAPI")!;
        var commandQueueType = Type.GetType("StardewModdingAPI.Framework.CommandQueue, StardewModdingAPI")!;
        var scoreGetter = scoreType
            .GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetGetMethod(true)!;
        var rawCommandQueueField = scoreType.GetField(
            "RawCommandQueue",
            BindingFlags.NonPublic | BindingFlags.Instance
        )!;
        var commandQueueAddMethod = commandQueueType.GetMethod("Add")!;
        var dynamicMethod = new DynamicMethod("AddToRawCommandQueue", null, [typeof(string)]);
        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Call, scoreGetter);
        il.Emit(OpCodes.Ldfld, rawCommandQueueField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, commandQueueAddMethod);
        il.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<Action<string>>();
    }
    #endregion

    public static bool IsContentPatcherLoaded { get; private set; } = false;
    public static string ContentPatcherVersion { get; internal set; } = "2.1.0";

    private Rectangle titleMenuButtonBounds = new(64, 64, 104, 104);
    private bool titleMenuButtonHovered = false;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        mon = Monitor;
        Config = helper.ReadConfig<ModConfig>();
        Config.doWriteConfig = helper.WriteConfig;
        ModRegistry = helper.ModRegistry;
        ModCreditString = $"by {ModManifest.Name} ({ModManifest.Version}) at ";

        ParseAssetName = helper.GameContent.ParseAssetName;
        DirectoryPath = helper.DirectoryPath;
        StagingDirectoryPath = string.Concat(DirectoryPath, ".Staging");
        InputDirectoryPath = Path.Combine(DirectoryPath, Utils.EDITING_INPUT);
        OutputDirectoryPath = Path.Combine(DirectoryPath, Utils.EDITING_OUTPUT);
        ModContent = helper.ModContent;
        GameContent = helper.GameContent;

        helper.ConsoleCommands.Add("sms-show", "show smithy menu to edit your mods.", ConsoleShowWorkspace);
        helper.ConsoleCommands.Add("sms-pack", "pack a folder of loose textures", ConsolePackTexture);

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
        helper.Events.Input.CursorMoved += OnCursorMoved;
        helper.Events.Content.AssetRequested += OnAssetRequested;

        CreateSmithyDirectories();
    }

    private static void CreateSmithyDirectories()
    {
        try
        {
            Directory.CreateDirectory(InputDirectoryPath);
        }
        catch (Exception err)
        {
            Log($"Failed to create '{InputDirectoryPath}', please manually create this folder\n{err}", LogLevel.Error);
        }
        try
        {
            Directory.CreateDirectory(OutputDirectoryPath);
        }
        catch (Exception err)
        {
            Log($"Failed to create '{OutputDirectoryPath}', please manually create this folder\n{err}", LogLevel.Error);
        }

        if (Utils.StageByCopy)
        {
            Directory.CreateDirectory(StagingDirectoryPath);
        }
        else
        {
            try
            {
                DirectoryInfo dirInfo = new(StagingDirectoryPath);
                if (dirInfo.Exists)
                {
                    if (dirInfo.LinkTarget != OutputDirectoryPath)
                    {
                        Directory.Delete(StagingDirectoryPath, true);
                    }
                    else
                    {
                        return;
                    }
                }
                FileInfo fileInfo = new(StagingDirectoryPath);
                if (fileInfo.Exists && fileInfo.LinkTarget != OutputDirectoryPath)
                {
                    File.Delete(StagingDirectoryPath);
                }
                File.CreateSymbolicLink(StagingDirectoryPath, OutputDirectoryPath);
            }
            catch (Exception err)
            {
                Log(
                    $"Failed to create a symlink from '{OutputDirectoryPath}' to '{StagingDirectoryPath}', content packs made by this mod will not be auto-installed",
                    LogLevel.Error
                );
                Log(err.ToString(), LogLevel.Trace);
            }
        }
    }

    internal static void ReloadAppIcons()
    {
        GameContent.InvalidateCache(IconBaseTexture);
        GameContent.InvalidateCache(IconHoverTexture);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(IconBaseTexture))
        {
            e.LoadFromModFile<Texture2D>($"assets/icon/{Config.IconStyle}/base.png", AssetLoadPriority.Low);
        }
        if (e.NameWithoutLocale.IsEquivalentTo(IconHoverTexture))
        {
            e.LoadFromModFile<Texture2D>($"assets/icon/{Config.IconStyle}/hover.png", AssetLoadPriority.Low);
        }
        //
        if (e.NameWithoutLocale.IsEquivalentTo("aedenthorn.LauncherDrawer/dict"))
        {
            e.Edit(data =>
            {
                data.AsDictionary<string, Dictionary<string, object>>().Data[ModId] = new()
                {
                    ["Name"] = "Smithy",
                    ["Description"] = "StardewModSmithy",
                    ["Action"] = new Action(EditorMenuManager.ShowWorkspace),
                };
            });
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!IsTitleMenuButtonActive())
            return;
        if (Game1.activeClickableMenu is TitleMenu titleM)
        {
            IClickableMenu.drawTextureBox(
                e.SpriteBatch,
                titleM.titleButtonsTexture,
                titleMenuButtonHovered ? new Rectangle(79, 458, 27, 25) : new Rectangle(52, 458, 27, 25),
                titleMenuButtonBounds.X,
                titleMenuButtonBounds.Y,
                titleMenuButtonBounds.Width,
                titleMenuButtonBounds.Height,
                Color.White,
                4,
                false,
                1
            );
            e.SpriteBatch.Draw(
                titleM.titleButtonsTexture,
                new Rectangle(titleMenuButtonBounds.X + 24, titleMenuButtonBounds.Y + 24, 56, 48),
                titleMenuButtonHovered ? new Rectangle(79 + 6, 458 + 6, 1, 1) : new Rectangle(52 + 6, 458 + 6, 1, 1),
                Color.White
            );
            e.SpriteBatch.Draw(
                Game1.content.Load<Texture2D>(titleMenuButtonHovered ? IconHoverTexture : IconBaseTexture),
                new Rectangle(titleMenuButtonBounds.X + 20, titleMenuButtonBounds.Y + 20, 64, 64),
                Color.White
            );
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!IsTitleMenuButtonActive())
            return;
        if (titleMenuButtonBounds.Contains(e.NewPosition.ScreenPixels))
        {
            titleMenuButtonHovered = true;
        }
        else
        {
            titleMenuButtonHovered = false;
        }
    }

    private static bool IsTitleMenuButtonActive()
    {
        return !Context.IsWorldReady
            && Game1.activeClickableMenu is TitleMenu titleM
            && TitleMenu.subMenu == null
            && titleM.titleInPosition
            && !titleM.isTransitioningButtons
            && !EditorMenuManager.showWorkspaceNextTick.Value;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Config.ShowWorkspaceKey.JustPressed())
        {
            EditorMenuManager.ShowWorkspace();
        }
        if (IsTitleMenuButtonActive() && titleMenuButtonHovered && Game1.didPlayerJustLeftClick())
        {
            titleMenuButtonHovered = false;
            EditorMenuManager.showWorkspaceNextTick.Value = true;
        }
        if (Config.QuickPatchReloadKey.JustPressed())
        {
            foreach (string modId in Config.QuickPatchReloadModIds)
            {
                PatchReload(modId);
            }
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        EditorMenuManager.Register(Helper);
        if (Helper.ModRegistry.Get("Pathoschild.ContentPatcher") is IModInfo contentPatcher)
        {
            ContentPatcherVersion = contentPatcher.Manifest.Version.ToString();
            IsContentPatcherLoaded = true;
        }
        AddToRawCommandQueue = Make_AddToRawCommandQueue();
        // iconic framework
        if (Helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons") is IIconicFrameworkApi ifApi)
        {
            ifApi.AddToolbarIcon(
                ModId,
                IconBaseTexture,
                null,
                () => ModManifest.Name,
                null,
                EditorMenuManager.ShowWorkspace
            );
        }
    }

    private static void ExecuteCommand(string command)
    {
        AddToRawCommandQueue?.Invoke(command);
    }

    internal static void PatchReload(string uniqueId)
    {
        if (!IsContentPatcherLoaded)
            return;

        if (!Config.AutoSymlinkAndPatchReload)
            return;

        if (ModRegistry.IsLoaded(uniqueId))
        {
            string cmd = string.Concat("patch reload ", uniqueId);
            Log($"Trying '{cmd}'", LogLevel.Info);
            ExecuteCommand(cmd);
        }
        else
        {
            LogOnce($"Restart the game to enable automatic patch reload on '{uniqueId}'", LogLevel.Info);
        }
    }

    private void ConsoleShowWorkspace(string cmd, string[] args)
    {
        EditorMenuManager.ShowWorkspace();
    }

    private void ConsolePackTexture(string cmd, string[] args)
    {
        if (!ArgUtility.TryGet(args, 0, out string? subdir, out string? error))
        {
            Log(error, LogLevel.Error);
            return;
        }
        SpritePacker.Pack(subdir);
    }

    public static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new IgnoreDefaultOptionalPropertiesResolver(true),
    };

    internal static void WriteJson(string targetPath, string fileName, object content)
    {
        File.WriteAllText(
            Path.Combine(targetPath, fileName),
            JsonConvert.SerializeObject(content, Formatting.Indented, jsonSerializerSettings)
        );
    }

    internal static void WriteJson(string targetFile, object content)
    {
        File.WriteAllText(
            targetFile,
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

    internal static T? ReadJson<T>(string targetFile)
    {
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
