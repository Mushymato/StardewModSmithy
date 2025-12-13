using StardewModdingAPI;

namespace StardewModSmithy.Models.Interfaces;

public interface ILoadableAsset
{
    public string Target { get; }
    public string FromFile { get; }
    public void StageFiles(string outputDir);
}
