using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using StardewModdingAPI;
using StardewModSmithy.Models;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;
using StardewValley.Extensions;

namespace StardewModSmithy.GUI.EditorContext;

public record PackDisplayEntry(IOutputPack Pack) : INotifyPropertyChanged
{
    public string PackUniqueID => Pack.Manifest.UniqueID;

    public string PackAuthor
    {
        get => Pack.Manifest.Author;
        set
        {
            Pack.Manifest.Author = value;
            OnPropertyChanged(new(nameof(PackAuthor)));
        }
    }

    public string PackName
    {
        get => Pack.Manifest.Name;
        set
        {
            Pack.Manifest.Name = value;
            OnPropertyChanged(new(nameof(PackName)));
        }
    }

    public string PackDescription
    {
        get => Pack.Manifest.Description;
        set
        {
            Pack.Manifest.Description = value;
            OnPropertyChanged(new(nameof(PackDescription)));
        }
    }

    public string PackVersion
    {
        get => Pack.Manifest.Version;
        set
        {
            Pack.Manifest.Version = value;
            OnPropertyChanged(new(nameof(PackVersion)));
        }
    }

    public string PackNexusID
    {
        get => Pack.Manifest.NexusID;
        set
        {
            Pack.Manifest.NexusID = value;
            OnPropertyChanged(new(nameof(PackNexusID)));
        }
    }

    private bool isExpanded = false;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded && !value)
            {
                Pack.Manifest.Save();
            }
            isExpanded = value;
            OnPropertyChanged(new(nameof(IsExpanded)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(PropertyChangedEventArgs value) => PropertyChanged?.Invoke(this, value);

    public bool IsLoaded => ModEntry.ModRegistry.IsLoaded(Pack.Manifest.UniqueID);

    public void BrowsePackFolder()
    {
        IsExpanded = false;
        Utils.BrowseFolder(Pack.Manifest.OutputFolder, false);
    }

    public bool CanShowEdit_Furniture =>
        Pack is OutputPackContentPatcher && (Pack.TxAssetGroup?.GatheredTextures.Any() ?? false);

    public void ShowEdit_Furniture()
    {
        if (!CanShowEdit_Furniture)
            return;
        IsExpanded = false;
        if (Pack is OutputPackContentPatcher outputPackContentPatcher && outputPackContentPatcher.TxAssetGroup != null)
        {
            outputPackContentPatcher.FurniAsset ??= new();
            outputPackContentPatcher.FurniAsset.SetTranslations(outputPackContentPatcher.Translations);
            EditorMenuManager.ShowFurnitureEditor(
                outputPackContentPatcher.TxAssetGroup,
                outputPackContentPatcher.FurniAsset,
                outputPackContentPatcher.Save
            );
        }
        else
        {
            ModEntry.Log($"Editor not implemented for this pack!", LogLevel.Error);
        }
    }

    public bool CanShowEdit_WallFloor =>
        Pack is OutputPackContentPatcher
        && (Pack.TxAssetGroup?.GatheredTextures.Values.Any(WallpaperFlooringAsset.TextureFilter) ?? false);

    public void ShowEdit_WallFloor()
    {
        if (!CanShowEdit_WallFloor)
            return;
        IsExpanded = false;
        if (Pack is OutputPackContentPatcher outputPackContentPatcher && outputPackContentPatcher.TxAssetGroup != null)
        {
            outputPackContentPatcher.WallAndFloorAsset ??= new();
            outputPackContentPatcher.WallAndFloorAsset.SetTranslations(outputPackContentPatcher.Translations);
            EditorMenuManager.ShowWallpaperAndFlooringEditor(
                outputPackContentPatcher.TxAssetGroup,
                outputPackContentPatcher.WallAndFloorAsset,
                outputPackContentPatcher.Save
            );
        }
    }
}

public partial class PackListingContext(TextureAssetGroup textureAssetGroup, List<IOutputPack> editablePacks)
{
    private readonly List<PackDisplayEntry> packDisplayList = editablePacks
        .Select(pack => new PackDisplayEntry(pack))
        .ToList();

    public IEnumerable<PackDisplayEntry> PackDisplayList => packDisplayList;

    public IEnumerable<TextureAsset> Textures => textureAssetGroup.GatheredTextures.Values;

    public bool HasTextures => Textures.Any();

    public string PutTexturesMessage => I18n.Message_PutTexture(ModEntry.InputDirectoryPath);

    public void ReloadTextures()
    {
        textureAssetGroup.Invalidate();
        OnPropertyChanged(new(nameof(Textures)));
        OnPropertyChanged(new(nameof(HasTextures)));
    }

    public void BrowseTextureFolder() => Utils.BrowseFolder(ModEntry.InputDirectoryPath);

    [Notify]
    private string newModName = string.Empty;

    [DependsOn(nameof(NewModName))]
    public string NewModId => MakeUniqueID(ModEntry.Config.GetAuthorName());

    public string NewModTooltip
    {
        get
        {
            if (textureAssetGroup.GatheredTextures.Count == 0)
            {
                return I18n.Message_CreateMod_NeedTextures();
            }
            if (string.IsNullOrEmpty(Sanitize.UniqueID(NewModName)))
            {
                return I18n.Message_CreateMod_NeedName();
            }
            string uniqueID = NewModId;
            if (!IsValidUniqueID(uniqueID))
            {
                return I18n.Message_CreateMod_IdNotUnique(uniqueID);
            }
            return I18n.Message_CreateMod_WillCreate(uniqueID);
        }
    }

    public float NewModErrorOpacity
    {
        get
        {
            if (textureAssetGroup.GatheredTextures.Count == 0 || string.IsNullOrEmpty(Sanitize.UniqueID(NewModName)))
            {
                return 0.5f;
            }
            string uniqueID = NewModId;
            if (!IsValidUniqueID(uniqueID))
            {
                return 0.5f;
            }
            return 1f;
        }
    }

    internal static PackListingContext Initialize()
    {
        TextureAssetGroup textureAssetGroup = new();
        List<IOutputPack> outputPacks = [];
        foreach (
            OutputManifest manifest in OutputManifest.LoadAllFromOutputFolder().OrderBy(manifest => manifest.UniqueID)
        )
        {
            // TODO: assume content patcher for now
            OutputPackContentPatcher outputContentPatcher = new(manifest);
            outputContentPatcher.Load();
            outputContentPatcher.TxAssetGroup = textureAssetGroup;
            outputPacks.Add(outputContentPatcher);
        }
        return new PackListingContext(textureAssetGroup, outputPacks);
    }

    public void CreateAndEdit()
    {
        string authorName = ModEntry.Config.AuthorNameWithDefault;
        string uniqueID = MakeUniqueID(authorName);
        if (!IsValidUniqueID(uniqueID))
        {
            return;
        }
        OutputManifest manifest = new()
        {
            Author = authorName,
            Name = NewModName,
            UniqueID = uniqueID,
        };
        NewModName = string.Empty;
        OutputPackContentPatcher outputPackContentPatcher = new(manifest) { TxAssetGroup = textureAssetGroup };
        outputPackContentPatcher.Save();
        editablePacks.Add(outputPackContentPatcher);
        PackDisplayEntry packDisplay = new(outputPackContentPatcher);
        packDisplayList.Add(packDisplay);
        PropertyChanged?.Invoke(this, new(nameof(PackDisplayList)));
    }

    private string MakeUniqueID(string authorName) =>
        string.Concat(Sanitize.UniqueID(authorName), '.', Sanitize.UniqueID(NewModName));

    private bool IsValidUniqueID(string uniqueID)
    {
        if (
            ModEntry.ModRegistry.IsLoaded(uniqueID)
            || editablePacks.Any(output => output.Manifest.UniqueID.EqualsIgnoreCase(uniqueID))
        )
        {
            return false;
        }
        return true;
    }
}
