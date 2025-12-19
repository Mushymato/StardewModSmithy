namespace StardewModSmithy.GUI.EditorContext;

internal sealed class BaseEditorContext
{
    public DraggableTextureContext TextureContext { get; private set; }
    public AbstractEditableAssetContext EditableContext { get; private set; }

    public BaseEditorContext(DraggableTextureContext textureContext, AbstractEditableAssetContext editableContext)
    {
        TextureContext = textureContext;
        EditableContext = editableContext;

        TextureContext.Dragged += EditableContext.SetSpriteIndex;
        EditableContext.BoundsProviderChanged += TextureContext.OnEditorBoundsProviderChanged;
    }
}
