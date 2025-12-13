using StardewModdingAPI;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy.GUI;

internal static class EditorMenuManager
{
    private static IViewEngine viewEngine = null!;
    private const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    private const string VIEW_FURNITURE_EDITOR = $"{VIEW_ASSET_PREFIX}/furniture-editor";

    internal static void Register(IModHelper helper)
    {
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    internal static void ShowFurnitureEditor(TextureAsset textureAsset, FurnitureAsset furnitureAsset)
    {
        FurnitureEditorContext editorContext = new(textureAsset, furnitureAsset);
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_FURNITURE_EDITOR, editorContext);
    }
}
