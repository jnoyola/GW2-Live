using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    //static class PointContext
    //{
    //    public static float ImageOffsetX { get; private set; }
    //    public static float ImageOffsetY { get; private set; }
    //    public static float ImageWidth { get; private set; }
    //    public static float ImageHeight { get; private set; }
    //    public static float ImageScale { get; private set; }

    //    public static float MapX { get; private set; }
    //    public static float MapY { get; private set; }
    //    public static float MapWidth { get; private set; }
    //    public static float MapHeight { get; private set; }
    //}

    //struct Point
    //{
    //    const float MetersPerInch = 0.0254f;

    //    private float x;
    //    private float y;
    //    private PointType type;

    //    public float xGame
    //    {
    //        get
    //        {
    //            switch (type)
    //            {
    //                case PointType.Game: return x;
    //                case PointType.Mumble: return x * MetersPerInch;
    //                case PointType.Percent: return (x - PointContext.MapX) / PointContext.MapWidth;
    //                case PointType.Pixel: x
    //            }
    //        }
    //    }

    //    public float xMumble
    //    {
    //        get
    //        {
    //            return 0;
    //        }
    //    }

    //    public float xPercent
    //    {
    //        get
    //        {
    //            return 0;
    //        }
    //    }

    //    public float xPixel
    //    {
    //        get
    //        {
    //            return 0;
    //        }
    //    }

    //    public static Point GamePoint(float x, float y)
    //    {
    //        return new Point { x = x, y = y, Type = PointType.Game };
    //    }
    //}

    //class GamePoint : Point
    //{
    //    public GamePoint(float x, float y) : base(x, y) {}

    //    public GamePoint ToGame() => this;
    //    public MumblePoint ToMumble()
    //    {
    //        return new MumblePoint(X * MetersPerInch, Y * MetersPerInch);
    //    }
    //    public PercentPoint ToPercent(Rectangle mapRect)
    //    {
    //        return new PercentPoint((X - mapRect.X) / mapRect.Width, (Y - mapRect.Y) / mapRect.Height);
    //    }
    //    public PixelPoint ToPixel()
    //}

    //class MumblePoint : Point
    //{
    //    public MumblePoint(float x, float y) : base(x, y) { }
    //}

    //class PercentPoint : Point
    //{
    //    public PercentPoint(float x, float y) : base(x, y) { }

    //    public PixelPoint ToPixel()
    //}

    //class PixelPoint : Point
    //{
    //    public PixelPoint(float x, float y) : base(x, y) { }
    //}

    //enum PointType
    //{
    //    Game,
    //    Mumble,
    //    Percent,
    //    Pixel
    //}
}
