using NodaTime;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Globalization;

namespace Frederikskaj2.Reservations.EmailSender;

record BodyCell(int Day, IReadOnlyList<ResourceUsages> Resources, bool IsOtherMonth, IsoDayOfWeek DayOfWeek) : Cell
{
    public override int Draw(IImageProcessingContext context, int xStart, int yStart, int width, int height)
    {
        FillBackground();
        DrawDate();
        var y = 15;
        const int resourceHeight = 16;
        var extend = DayOfWeek is not IsoDayOfWeek.Sunday ? Design.BorderWidth : 0;
        for (var i = 0; i < Resources.Count; i += 1)
        {
            var usage = Resources[i];
            var x = 0;
            if (usage.HasFlag(ResourceUsages.InUseBefore))
                Fill(context, x + xStart, y + yStart, width/4 + extend, resourceHeight, Design.ReservedBrushes[i]);
            else if (usage.HasFlag(ResourceUsages.CleaningBefore))
                Fill(context, x + xStart, y + yStart, width/4 + extend, resourceHeight, Design.CleaningBrushes[i]);
            x += width/4;
            if (usage.HasFlag(ResourceUsages.InUseBetween))
                Fill(context, x + xStart, y + yStart, width/4 + extend, resourceHeight, Design.ReservedBrushes[i]);
            else if (usage.HasFlag(ResourceUsages.CleaningBetween))
                Fill(context, x + xStart, y + yStart, width/4 + extend, resourceHeight, Design.CleaningBrushes[i]);
            x += width/4;
            if (usage.HasFlag(ResourceUsages.InUseAfter))
                Fill(context, x + xStart, y + yStart, width/2 + extend, resourceHeight, Design.ReservedBrushes[i]);
            else if (usage.HasFlag(ResourceUsages.CleaningAfter))
                Fill(context, x + xStart, y + yStart, width/2 + extend, resourceHeight, Design.CleaningBrushes[i]);
            y += resourceHeight;
        }

        return xStart + width;

        void FillBackground()
        {
            var backgroundColor = (IsOtherMonth, DayOfWeek) switch
            {
                (true, _) => Design.OtherMonthBackgroundColor,
                (false, IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday) => Design.WeekendBackgroundColor,
                _ => Design.WeekdayBackgroundColor
            };
            context.Fill(backgroundColor, GetCellPath(xStart, yStart, width, height));
        }

        void DrawDate()
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            var font = Design.FontFamily.CreateFont(Design.FontSizeSmall, Design.TableBodyFontStyle);
            var options = new RichTextOptions(font)
            {
                Origin = new PointF(xStart + 2, yStart + 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Font = font
            };
            context.DrawText(options, Day.ToString(CultureInfo.InvariantCulture), Design.TableBodyForegroundColor);
        }

        static void Fill(IImageProcessingContext context, int x, int y, int width, int height, Brush brush) =>
            context.Fill(brush, GetCellPath(x, y, width, height));
    }
}
