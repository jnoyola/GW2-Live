using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    static class GraphicsUtils
    {
        private const int Stride = 1;

        public static Rect FindWindow(Bitmap before, Bitmap after)
        {
            int width = before.Width / Stride;
            int height = before.Height / Stride;

            bool[] changedPixels = new bool[width * height];
            int x, y;
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    x = i * Stride;
                    y = j * Stride;
                    changedPixels[(i * height) + j] = before.GetPixel(x, y) != after.GetPixel(x, y);
                }
            }

            ToBitmap(changedPixels, width, height).Save($"c:\\users\\Jonathan\\Desktop\\changed.png", System.Drawing.Imaging.ImageFormat.Png);

            return FindLargestRectangle(changedPixels, width, height);
        }

        private struct ColorARGB
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public ColorARGB(Color color)
            {
                A = color.A;
                R = color.R;
                G = color.G;
                B = color.B;
            }

            public ColorARGB(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            public Color ToColor()
            {
                return Color.FromArgb(A, R, G, B);
            }
        }

        private static unsafe Bitmap ToBitmap(bool[] pixels, int width, int height)
        {
            Bitmap Image = new Bitmap(width, height);
            BitmapData bitmapData = Image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );
            ColorARGB* startingPosition = (ColorARGB*)bitmapData.Scan0;


            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    ColorARGB* position = startingPosition + i + j * width;
                    position->A = 255;
                    position->R = 0;
                    position->G = 0;
                    position->B = pixels[(i * height) + j] ? (byte)255 : (byte)0;
                }

            Image.UnlockBits(bitmapData);
            return Image;
        }

        private struct SubRect
        {
            public int y;
            public int width;
        }

        private static Rect FindLargestRectangle(bool[] grid, int width, int height)
        {
            //http://www.drdobbs.com/database/the-maximal-rectangle-problem/184410529

            // These variable will hold the best results.
            int x0 = 0;
            int y0 = 0;
            int x1 = 0;
            int y1 = 0;

            // Initialize cache with 0s.
            // This will hold the number of 'true' values in this cell or to the right, for each cell in a column.
            int[] cache = new int[height];

            Stack<SubRect> stack = new Stack<SubRect>();
            SubRect subRect = new SubRect();

            int x, y, w;
            for (x = width - 1; x >= 0; --x)
            {
                // Update cache.
                for (y = 0; y < height; ++y)
                {
                    if (grid[(x * height) + y])
                    {
                        ++cache[y];
                    }
                    else
                    {
                        cache[y] = 0;
                    }
                }

                w = 0;
                for (y = 0; y < height; ++y)
                {
                    if (cache[y] > w)
                    {
                        stack.Push(new SubRect { y = y, width = w});
                        w = cache[y];
                    }
                    else if (cache[y] < w)
                    {
                        do
                        {
                            subRect = stack.Pop();

                            // Note that the area is actually missing one pixel from each row and column to be more optimal.
                            if (w * (y - subRect.y) > (x1 - x0) * (y1 - y0))
                            {
                                x0 = x;
                                y0 = subRect.y;
                                x1 = x + w - 1;
                                y1 = y - 1;
                            }

                            w = subRect.width;
                        } while (cache[y] < w);

                        w = cache[y];
                        if (w != 0)
                        {
                            stack.Push(subRect);
                        }
                    }
                }
            }

            return new Rect(x0 * Stride, y0 * Stride, x1 * Stride, y1 * Stride);
        }
    }
}
