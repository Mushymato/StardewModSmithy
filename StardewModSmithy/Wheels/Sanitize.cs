namespace StardewModSmithy.Wheels;

internal static class Sanitize
{
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
        return string.Join(path, path.Split(System.IO.Path.GetInvalidFileNameChars(), '_'));
    }
}
