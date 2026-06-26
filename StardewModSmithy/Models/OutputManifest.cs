using StardewModSmithy.Wheels;
using StardewValley.Extensions;

namespace StardewModSmithy.Models;

public sealed record SmithyInfo()
{
    public List<string> Generated { get; set; } = [];
    public List<string> Custom { get; set; } = [];
    public string Exported => string.Concat(ModEntry.ModCreditString, DateTime.Now.ToString());
};

public sealed class OutputManifest()
{
    internal string PackFor { get; set; } = Utils.DEFAULT_STR;
    internal string OutputFolder => Path.Combine(ModEntry.OutputDirectoryPath, Sanitize.Path(UniqueID));
    internal string TranslationFolder => Path.Combine(OutputFolder, Utils.TL_DIR);
    internal string NexusID { get; set; } = string.Empty;

    public string Author { get; set; } = "";
    public string Name { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string UniqueID { get; set; } = string.Empty;
    public string Description { get; set; } = "New mod made with StardewModSmithy";
    public object ContentPackFor => new { UniqueID = PackFor };
    public List<object>? Dependencies { get; set; } = null;
    public List<string>? UpdateKeys
    {
        get
        {
            List<string>? updateKeys = null;
            if (!string.IsNullOrEmpty(NexusID))
            {
                updateKeys ??= [];
                updateKeys.Add(string.Concat("Nexus:", NexusID));
            }
            return updateKeys;
        }
        set
        {
            if (value == null)
            {
                NexusID = string.Empty;
                return;
            }
            foreach (string updateKey in value)
            {
                if (updateKey.StartsWithIgnoreCase("nexus:"))
                {
                    NexusID = updateKey[6..];
                    return;
                }
            }
        }
    }
    public SmithyInfo StardewModSmithyInfo { get; set; } = new();

    public void Save()
    {
        ModEntry.WriteJson(OutputFolder, Utils.MANIFEST_FILE, this);
    }

    public static IEnumerable<OutputManifest> LoadAllFromOutputFolder()
    {
        foreach (string subdir in Directory.GetDirectories(ModEntry.OutputDirectoryPath))
        {
            string manifestPath = Path.Combine(subdir, Utils.MANIFEST_FILE);
            if (!File.Exists(manifestPath))
                continue;
            OutputManifest? manifest = ModEntry.ReadJson<OutputManifest>(manifestPath);
            if (manifest == null)
                continue;
            yield return manifest;
        }
    }
}
