namespace StardewModSmithy.Content;

internal interface IOutputPack
{
    public void Save(string targetPath);
    public void Load(string targetPath);
}
