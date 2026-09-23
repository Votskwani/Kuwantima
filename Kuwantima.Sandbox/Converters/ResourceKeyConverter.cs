using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Kuwantima.Sandbox.Converters
{
    /// <summary>
    /// Looks up a resource by key at bind time — used to turn a NavPage's IconKey string
    /// ("Icon.Home") into the StreamGeometry that KuwantimaStreamIcons.axaml defines.
    ///
    /// Why this exists: a DataTemplate cannot write {StaticResource {Binding IconKey}} — a resource
    /// key must be known at parse time. Keeping the lookup here means the ViewModel carries a
    /// string and never touches Geometry, so it stays free of view types.
    ///
    /// Returns null when the key is missing rather than throwing: an unknown key should leave a
    /// nav button without an icon, not take the window down — and it must not break the previewer,
    /// where resources may not all be loaded.
    /// </summary>
    public class ResourceKeyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string key || Application.Current is not { } app)
                return null;

            return app.TryGetResource(key, app.ActualThemeVariant, out var resource) ? resource : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException($"{nameof(ResourceKeyConverter)} is one-way.");
    }
}
