using StardewModdingAPI;

namespace StardewModSmithy.Wheels;

internal static class Sanitize
{
    public const string ModIdPrefixValue = "{{ModId}}_";

    public static string SanitizeImpl(string value, char replacement, char[] illegal)
    {
        return string.Join(replacement, value.Split(illegal));
    }

    public static readonly char[] IllegalKeyChars = ['{', '}', '[', ']', '(', ')', ':', '/', ',', ' '];

    public static string Key(string key)
    {
        return SanitizeImpl(key, '.', IllegalKeyChars);
    }

    public static string Path(string path)
    {
        return SanitizeImpl(path, '_', System.IO.Path.GetInvalidFileNameChars());
    }

    public static string AssetName(IAssetName assetName)
    {
        return assetName.BaseName.Replace('/', '\\');
    }

    public static string ModIdPrefix(string name)
    {
        if (name.StartsWith(ModIdPrefixValue))
        {
            return name.Replace(ModIdPrefixValue, "");
        }
        return name;
    }
}
