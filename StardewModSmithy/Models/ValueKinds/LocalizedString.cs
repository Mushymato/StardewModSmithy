using System.Text.RegularExpressions;
using StardewModSmithy.Wheels;

namespace StardewModSmithy.Models.ValueKinds;

public enum LocalizedStringKind
{
    LocalizedText = 0,
    ContentPatcherI18N = 1,
}

public sealed class LocalizedString(string key)
{
    public LocalizedStringKind Kind = LocalizedStringKind.LocalizedText;
    public string Key { get; set; } = Sanitize.Key(key);
    public string? Value { get; set; } = null;

    public static readonly Regex localizedTextRE = new(@"\[LocalizedText \{\{ModId\}\}.i18n:\{(\w+)\}\]");
    public static readonly Regex contentPatcherI18NRE = new(@"\{\{i18n: (\w+)\}\}");

    public static LocalizedString? Deserialize(string str)
    {
        if (localizedTextRE.Match(str) is Match match1 && match1.Success)
        {
            return new LocalizedString(match1.Groups[1].Value);
        }
        else if (contentPatcherI18NRE.Match(str) is Match match2 && match2.Success)
        {
            return new LocalizedString(match2.Groups[1].Value);
        }
        return null;
    }

    public string GetToken()
    {
        return Kind switch
        {
            LocalizedStringKind.LocalizedText => $"[LocalizedText {{{{ModId}}}}.i18n:{Key}]",
            LocalizedStringKind.ContentPatcherI18N => string.Concat("{{i18n: ", Key, "}}"),
            _ => throw new NotImplementedException(),
        };
    }
}
