using System.Globalization;

namespace FrontOfficeERP.Converters;

/// <summary>
/// Converts a ShiftType string to a gradient brush for duty cards.
/// General: Green, Morning: Blue, Evening: Orange, Night: Dark Purple/Indigo.
/// </summary>
public class ShiftTypeToGradientConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var shiftType = (value as string)?.ToLowerInvariant() ?? "general";

        return shiftType switch
        {
            "morning" => new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#1565C0"), 0.0f),
                    new GradientStop(Color.FromArgb("#42A5F5"), 0.5f),
                    new GradientStop(Color.FromArgb("#90CAF9"), 1.0f)
                }
            },
            "evening" => new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#E65100"), 0.0f),
                    new GradientStop(Color.FromArgb("#FB8C00"), 0.5f),
                    new GradientStop(Color.FromArgb("#FFB74D"), 1.0f)
                }
            },
            "night" => new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#1A237E"), 0.0f),
                    new GradientStop(Color.FromArgb("#4A148C"), 0.5f),
                    new GradientStop(Color.FromArgb("#7B1FA2"), 1.0f)
                }
            },
            _ => new LinearGradientBrush // General / default: Green
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#1B5E20"), 0.0f),
                    new GradientStop(Color.FromArgb("#2E7D32"), 0.5f),
                    new GradientStop(Color.FromArgb("#66BB6A"), 1.0f)
                }
            }
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a ChangeType enum value to a color for cell diff display.
/// </summary>
public class ChangeTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FrontOfficeERP.Models.ChangeType changeType)
        {
            return changeType switch
            {
                FrontOfficeERP.Models.ChangeType.Addition => Color.FromArgb("#2E7D32"),
                FrontOfficeERP.Models.ChangeType.Deletion => Color.FromArgb("#C62828"),
                FrontOfficeERP.Models.ChangeType.ValueChange => Color.FromArgb("#E65100"),
                _ => Color.FromArgb("#666666")
            };
        }
        return Color.FromArgb("#666666");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
