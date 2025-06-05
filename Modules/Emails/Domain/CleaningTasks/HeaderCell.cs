using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Frederikskaj2.Reservations.Emails;

record HeaderCell(string Text) : Cell
{
    public override int Draw(IImageProcessingContext context, int x, int y, int width, int height)
    {
        context.Fill(Color.White, GetCellPath(x, y, width, height));
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        var font = Design.FontFamily.CreateFont(Design.FontSizeNormal, Design.TableHeaderFontStyle);
        var options = new RichTextOptions(font)
        {
            Origin = new PointF(x + width/2F, y + height/2F),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Font = font,
        };
        context.DrawText(options, Text, Design.TableHeaderForegroundColor);
        return x + width;
    }
}
