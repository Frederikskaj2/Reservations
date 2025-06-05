using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

static class CleaningSchedulePictureGenerator
{
    public static async ValueTask<Picture> GenerateMonthPicture(Table table, CancellationToken cancellationToken)
    {
        using var image = CreateMonthImage(table);
        return await CreatePicture(image, cancellationToken);
    }

    public static async ValueTask<Picture> GenerateLegend(IReadOnlyList<string> orderedResourceNames, CancellationToken cancellationToken)
    {
        using var image = CreateLegendImage(orderedResourceNames);
        return await CreatePicture(image, cancellationToken);
    }

    static Image<Rgba32> CreateMonthImage(Table table)
    {
        var image = new Image<Rgba32>(GetTotalWidth(), GetTotalHeight(table));
        var y = table.Body.Aggregate(
            DrawRow(table.Header, image, 0, Design.HeaderCellHeight, Design.OuterBorderColor),
            (current, row) => DrawRow(row, image, current, Design.BodyCellHeight, Design.InnerBorderColor));
        DrawHorizontalLine(image, y, GetTotalWidth(), Design.OuterBorderColor);
        return image;

        static int GetTotalHeight(Table table)
        {
            return table.Header.Height + table.Body.Sum(row => row.Height + Design.BorderWidth) + 2*Design.BorderWidth;
        }

        static int DrawRow(Row row, Image image, int y, int height, Color borderColor)
        {
            y = DrawHorizontalLine(image, y, GetTotalWidth(), borderColor);
            var x = 0;
            for (var i = 0; i < row.Cells.Count; i += 1)
            {
                x = DrawVerticalLine(image, x, y, row.Height, i is 0 ? Design.OuterBorderColor : borderColor);
                x += Design.CellWidth;
            }

            x = 0;
            for (var i = 0; i < row.Cells.Count; i += 1)
            {
                x += Design.BorderWidth;
                // ReSharper disable AccessToModifiedClosure
                image.Mutate(context => x = row.Cells[i].Draw(context, x, y, Design.CellWidth, height));
                // ReSharper restore AccessToModifiedClosure
            }

            DrawVerticalLine(image, x, y, row.Height, Design.OuterBorderColor);
            return y + row.Height;
        }

        static int DrawHorizontalLine(Image image, int y, int width, Color color)
        {
            image.Mutate(context => context.Draw(
                color,
                Design.BorderWidth,
                new RectangleF(0, y + Design.BorderWidth/2F, width, y + Design.BorderWidth/2F)));
            return y + Design.BorderWidth;
        }

        static int DrawVerticalLine(Image image, int x, int y, int height, Color color)
        {
            image.Mutate(context => context.Draw(
                color,
                Design.BorderWidth,
                new RectangleF(x + Design.BorderWidth/2F, y, x + Design.BorderWidth/2F, y + height)));
            return x + Design.BorderWidth;
        }
    }

    static Image<Rgba32> CreateLegendImage(IReadOnlyList<string> orderedResourceNames)
    {
        var width = GetTotalWidth();
        const int height = 48;
        const int boxWidth = 160;
        const int boxHeight = 28;
        var horizontalMargin = (width - 3*boxWidth)/4;
        const int verticalMargin = (height - boxHeight)/2;

        var image = new Image<Rgba32>(width, height);

        FillBackground();

        var x = 0;
        for (var i = 0; i < orderedResourceNames.Count; i += 1)
            x = DrawBox(image, x + horizontalMargin, verticalMargin, boxWidth, boxHeight, orderedResourceNames[i], Design.CleaningBrushes[i]);

        return image;

        void FillBackground()
        {
            var path = new PathBuilder()
                .AddLines(new PointF(0, 0), new PointF(width, 0), new PointF(width, height), new PointF(0, height))
                .CloseFigure()
                .Build();
            image.Mutate(context => context.Fill(Color.White, path));
        }

        static int DrawBox(Image image, int x, int y, int width, int height, string text, Brush brush)
        {
            var path = new PathBuilder()
                .SetOrigin(new(x, y))
                .AddLines(new PointF(0, 0), new PointF(width, 0), new PointF(width, height), new PointF(0, height))
                .CloseFigure()
                .Build();
            image.Mutate(context => context.Fill(brush, path));
            var font = Design.FontFamily.CreateFont(Design.FontSizeNormal, Design.LegendFontStyle);
            var options = new RichTextOptions(font)
            {
                Origin = new PointF(x + width/2F, y + height/2F),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Font = font,
            };
            image.Mutate(context => context.DrawText(options, text, Design.LegendForegroundColor));
            return x + width;
        }
    }

    static async ValueTask<Picture> CreatePicture(Image image, CancellationToken cancellationToken)
    {
        var encoder = new PngEncoder { BitDepth = PngBitDepth.Bit8 };
        await using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream, encoder, cancellationToken);
        return new(image.Width, image.Height, "image/png", stream.ToArray().AsImmutableArrayUnsafe());
    }

    static int GetTotalWidth() =>
        7*Design.CellWidth + 8*Design.BorderWidth;
}
