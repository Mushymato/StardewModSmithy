/*
Copied from:
https://github.com/focustense/StardewControllers/blob/dev/StarControl/UI/EnumSegmentsViewModel.cs

MIT License

Copyright (c) 2024 focustense

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace StardewModSmithy.GUI;

/// <summary>
/// View model for displaying and choosing an enum value using a segmented control.
/// </summary>
/// <typeparam name="T">Type of enum.</typeparam>
/// <param name="translations">The translation helper for retrieving enum display names and
/// descriptions.</param>
public partial class EnumSegmentsViewModel<T>() : INotifyPropertyChanged
    where T : struct, Enum
{
    /// <summary>
    /// Raised when the <see cref="SelectedValue"/> changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    private static readonly T[] AllValues = Enum.GetValues<T>();

    /// <summary>
    /// List of all segments, one per enum value.
    /// </summary>
    public IReadOnlyList<Segment> Segments { get; } =
        AllValues.Select((v, i) => new Segment(v, GetName(v), GetDescription(v)) { Selected = i == 0 }).ToList();

    /// <summary>
    /// The enum value that is currently selected.
    /// </summary>
    public T SelectedValue
    {
        get => AllValues[SelectedIndex];
        set => SelectedIndex = Array.IndexOf(AllValues, value);
    }

    /// <summary>
    /// The index of the selected enum value.
    /// </summary>
    [Notify]
    private int selectedIndex;

    private static string GetDescription(T value)
    {
        return I18n.GetByKey($"gui.{typeof(T).Name}.{value}.desc").UsePlaceholder(false);
    }

    private static string GetName(T value)
    {
        return I18n.GetByKey($"gui.{typeof(T).Name}.{value}.name");
    }

    private void OnSelectedIndexChanged(int oldValue, int newValue)
    {
        Segments[oldValue].Selected = false;
        Segments[newValue].Selected = true;
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Model for a single segment representing a single enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <param name="name">Name to display in the segment as text.</param>
    /// <param name="description">Optional description to display as a tooltip.</param>
    public partial class Segment(T value, string name, string description)
    {
        /// <summary>
        /// Optional description to display as a tooltip.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Name to display in the segment as text.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The enum value.
        /// </summary>
        public T Value => value;

        /// <summary>
        /// Whether this is the selected segment.
        /// </summary>
        [Notify]
        private bool selected;
    }
}
