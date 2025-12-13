namespace StardewModSmithy.Models.Interfaces;

public interface IEditableAsset
{
    public string Target { get; }
    public string IncludeName { get; }
    public Dictionary<string, object> GetData();
}
