using Newtonsoft.Json;
using StardewModSmithy.Models.Interfaces;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Content;

public interface IMockPatch
{
    public string Action { get; }
}

public sealed record MockEditData(string Target, Dictionary<string, object> Entries) : IMockPatch
{
    public string Action => "EditData";
}

public sealed record MockLoad(string Target, string FromFile) : IMockPatch
{
    public string Action => "Load";
    public string Priority => "Medium";
}

public sealed record MockInclude(string FromFile) : IMockPatch
{
    public string Action => "Include";
}

internal record MockContent(List<IMockPatch> Changes);

internal sealed record MockContentMain(List<IMockPatch> Changes) : MockContent(Changes)
{
#pragma warning disable CA1822 // Mark members as static
    public string Format => ModEntry.ContentPatcherVersion;
#pragma warning restore CA1822 // Mark members as static
}

public sealed record MockManifest(string Name, string Author)
{
    public string Version = "1.0.0";
    public string UniqueID = Sanitize.Key(string.Concat(Author, '.', Name));
    public string Description => UniqueID;
    public object ContentPackFor = new { UniqueID = "Pathoschild.ContentPatcher" };
    public List<string> UpdateKeys = [];
}

public sealed class OutputContentPack(MockManifest manifest) : IOutputPack
{
    public List<ILoadableAsset> LoadableAssets = [];
    public List<IEditableAsset> EditableAssets = [];

    public void Save(string targetPath)
    {
        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
        Directory.CreateDirectory(dataDir);
        Directory.CreateDirectory(assetsDir);
        List<IMockPatch> Changes = [];
        // loads
        foreach (ILoadableAsset loadable in LoadableAssets)
        {
            Changes.Add(new MockLoad(loadable.Target, loadable.FromFile));
            loadable.StageFiles(assetsDir);
        }
        // edits
        foreach (IEditableAsset editable in EditableAssets)
        {
            Changes.Add(new MockInclude(Path.Combine("data", editable.IncludeName)));
            WriteJson(
                dataDir,
                editable.IncludeName,
                new MockContent([new MockEditData(editable.Target, editable.GetData())])
            );
        }
        // content.json
        WriteJson(targetPath, "content.json", new MockContentMain(Changes));
        // manifest.json
        WriteJson(targetPath, "manifest.json", manifest);
    }

    private static void WriteJson(string targetPath, string fileName, object content)
    {
        File.WriteAllText(
            Path.Combine(targetPath, fileName),
            JsonConvert.SerializeObject(content, Formatting.Indented)
        );
    }

    public void Load(string targetPath)
    {
        string dataDir = Path.Combine(targetPath, "data");
        string assetsDir = Path.Combine(targetPath, "assets");
    }
}
