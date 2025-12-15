using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModSmithy.Integration;
using StardewModSmithy.Models;
using StardewValley;

namespace StardewModSmithy.GUI;

internal static class EditorMenuManager
{
    private static IViewEngine viewEngine = null!;
    private const string VIEW_ASSET_PREFIX = $"{ModEntry.ModId}/views";
    private const string VIEW_FURNITURE_EDITOR = $"{VIEW_ASSET_PREFIX}/furniture-editor";
    private static readonly PerScreen<FurnitureEditorContext?> editorContext = new();
    private static IModHelper helper = null!;

    internal static void Register(IModHelper helper)
    {
        EditorMenuManager.helper = helper;
        viewEngine = helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;
        viewEngine.RegisterSprites($"{ModEntry.ModId}/sprites", "assets/sprites");
        viewEngine.RegisterViews(VIEW_ASSET_PREFIX, "assets/views");
#if DEBUG
        viewEngine.EnableHotReloadingWithSourceSync();
#endif
    }

    internal static void ShowFurnitureEditor(TextureAsset textureAsset, FurnitureAsset furnitureAsset)
    {
        editorContext.Value = new(textureAsset, furnitureAsset);
        Game1.activeClickableMenu = viewEngine.CreateMenuFromAsset(VIEW_FURNITURE_EDITOR, editorContext.Value);
        // helper.Events.Display.MenuChanged += OnMenuChanged;
        // helper.Events.Input.ButtonPressed += OnButtonPressed;
        // helper.Events.Input.ButtonReleased += OnButtonReleased;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is null)
        {
            editorContext.Value = null;
            helper.Events.Display.MenuChanged -= OnMenuChanged;
            helper.Events.Input.ButtonPressed -= OnButtonPressed;
            helper.Events.Input.ButtonReleased -= OnButtonReleased;
        }
    }

    private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft && editorContext.Value is FurnitureEditorContext ctx)
        {
            helper.Events.Input.CursorMoved += OnCursorMoved;
            ctx.SheetDragStart(e.Cursor.ScreenPixels);
        }
    }

    private static void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft && editorContext.Value is FurnitureEditorContext ctx)
        {
            helper.Events.Input.CursorMoved -= OnCursorMoved;
            ctx.SheetDragEnd(e.Cursor.ScreenPixels);
        }
    }

    private static void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (editorContext.Value is FurnitureEditorContext ctx)
            ctx.SheetDrag(e.NewPosition.ScreenPixels);
    }
}
