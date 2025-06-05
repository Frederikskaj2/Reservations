using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;

namespace Frederikskaj2.Reservations.Emails;

abstract record Cell
{
    public abstract int Draw(IImageProcessingContext context, int x, int y, int width, int height);

    protected static IPath GetCellPath(int x, int y, int width, int height) =>
        new PathBuilder()
            .SetOrigin(new(x, y))
            .AddLines(new PointF(0, 0), new PointF(width, 0), new PointF(width, height), new PointF(0, height))
            .CloseFigure()
            .Build();
}
