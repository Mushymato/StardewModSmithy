namespace StardewModSmithy.Models.ValueKinds;

public sealed class OptionedValue<T>(IList<T> options, T defaultValue)
{
    private readonly IList<T> options = options;
    public T Value
    {
        get => field;
        set
        {
            if (options.Contains(value))
            {
                field = value;
            }
        }
    } = defaultValue;

    public override string ToString()
    {
        return Value?.ToString() ?? "???";
    }
}
