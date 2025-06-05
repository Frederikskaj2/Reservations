using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Frederikskaj2.Reservations.Emails;

static class Design
{
    static readonly bool[][,] resourceBrushes =
    [
        new[,]
        {
            { false, false, false, false, false, false, false, false },
            { false, true, true, false, false, false, false, false },
            { false, true, true, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false },
            { false, false, false, false, false, true, true, false },
            { false, false, false, false, false, true, true, false },
            { false, false, false, false, false, false, false, false },
        },
        new[,]
        {
            { true, false, false, false, false, false },
            { false, true, false, false, false, false },
            { false, false, true, false, false, false },
            { false, false, false, true, false, false },
            { false, false, false, false, true, false },
            { false, false, false, false, false, true },
        },
        new[,]
        {
            { false, false, false, false, false, true },
            { false, false, false, false, true, false },
            { false, false, false, true, false, false },
            { false, false, true, false, false, false },
            { false, true, false, false, false, false },
            { true, false, false, false, false, false },
        },
    ];

    static readonly Color reservedBackgroundColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(0, 0.5F, 0.67F)).ToVector3()));
    static readonly Color reservedPatternColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(0, 0.5F, 0.8F)).ToVector3()));
    static readonly Color cleaningBackgroundColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(120, 0.67F, 0.4F)).ToVector3()));
    static readonly Color cleaningPatternColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(120, 0.67F, 0.6F)).ToVector3()));

    public const int FontSizeNormal = 12;
    public const int FontSizeSmall = 8;
    public const int BorderWidth = 1;
    public const int CellWidth = 80;
    public const int HeaderCellHeight = 30;
    public const int BodyCellHeight = 72;

    public const FontStyle TableHeaderFontStyle = FontStyle.Bold;
    public const FontStyle TableBodyFontStyle = FontStyle.Regular;
    public const FontStyle LegendFontStyle = FontStyle.Regular;

    public static readonly FontFamily FontFamily = GetFontFamily();

    public static readonly Color InnerBorderColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(240, 0F, 0.9F)).ToVector3()));
    public static readonly Color OuterBorderColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(240, 0F, 0.7F)).ToVector3()));
    public static readonly Color OtherMonthBackgroundColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(240, 0F, 0.8F)).ToVector3()));
    public static readonly Color WeekendBackgroundColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(240, 0.5F, 0.8F)).ToVector3()));
    public static readonly Color WeekdayBackgroundColor = new(new Argb32(ColorSpaceConverter.ToRgb(new Hsl(240, 0.5F, 0.9F)).ToVector3()));
    public static readonly Color TableHeaderForegroundColor = Color.Black;
    public static readonly Color TableBodyForegroundColor = Color.Black;
    public static readonly Color LegendForegroundColor = Color.White;

    public static readonly IReadOnlyList<PatternBrush> ReservedBrushes =
        resourceBrushes.Select(brush => new PatternBrush(reservedPatternColor, reservedBackgroundColor, brush)).ToArray();

    public static readonly IReadOnlyList<PatternBrush> CleaningBrushes =
        resourceBrushes.Select(brush => new PatternBrush(cleaningPatternColor, cleaningBackgroundColor, brush)).ToArray();

    static FontFamily GetFontFamily()
    {
        var collection = new FontCollection();
        AddFont(collection, "Frederikskaj2.Reservations.Emails.CleaningTasks.verdana.ttf");
        // ReSharper disable once StringLiteralTypo
        AddFont(collection, "Frederikskaj2.Reservations.Emails.CleaningTasks.verdanab.ttf");
        return collection.Get("Verdana", CultureInfo.InvariantCulture);
    }

    static void AddFont(FontCollection collection, string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)!;
        collection.Add(stream, CultureInfo.InvariantCulture);
    }
}
