namespace Kuwantima.Sandbox.ViewModels
{
    /// <summary>
    /// The Color Palette Picker's 5 accent choices. Member names match the suffix of the
    /// KuwantimaPalette{X} theme resource keys exactly, so MainWindowViewModel can look up the
    /// actual color by name instead of duplicating hex values here.
    /// </summary>
    public enum AccentColorOption
    {
        Blue,
        Orange,
        Purple,
        Green,
        Rose,
    }
}
