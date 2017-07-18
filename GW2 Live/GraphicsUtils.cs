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
        private const byte DiffThreshold = 0;
        private const byte LightThreshold = 80;
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
                    changedPixels[(i * height) + j] = IsDifferent(before.GetPixel(x, y), after.GetPixel(x, y));
                }
            }

            changedPixels = Dilate(changedPixels, width, height);

            ToBitmap(changedPixels, width, height).Save($"c:\\users\\Jonathan\\Desktop\\changed.png", System.Drawing.Imaging.ImageFormat.Png);

            return FindLargestRectangle(changedPixels, width, height);
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

        public static bool[] ToLightmap(Bitmap bitmap, int x, int y, int width, int height)
        {
            bool[] lightmap = new bool[width * height];

            byte max = 0;

            int k = -1;
            for (int i = x; i < x + width; ++i)
            {
                for (int j = y; j < y + height; ++j)
                {
                    var p = bitmap.GetPixel(i, j);
                    if (p.R > max)
                        max = p.R;
                    lightmap[++k] = IsLight(bitmap.GetPixel(i, j));
                }
            }

            return lightmap;
        }

        public struct Blob
        {
            int x0;
            int y0;
            int x1;
            int y1;
            public int count;

            public void AddPoint(int x, int y)
            {
                if (count == 0)
                {
                    x0 = x;
                    y0 = y;
                    x1 = x;
                    y1 = y;
                    count = 1;
                }
                else
                {
                    if (x < x0)
                    {
                        x0 = x;
                    }
                    else if (x > x1)
                    {
                        x1 = x;
                    }

                    if (y < y0)
                    {
                        y0 = y;
                    }
                    else if (y > y1)
                    {
                        y1 = y;
                    }

                    ++count;
                }
            }
        }

        public static List<Blob> FindBlobs(bool[] pixels, int width, int height, int dilation = 2)
        {
            List<Blob> blobs = new List<Blob>();

            Stack<Point> stack = new Stack<Point>();

            bool[] isSeen = new bool[pixels.Length];

            for (int x = dilation; x < width - dilation; ++x)
            {
                for (int y = dilation; y < height - dilation; ++y)
                {
                    int z = x * height + y;
                    if (!isSeen[z] && pixels[z])
                    {
                        stack.Clear();

                        Blob blob = new Blob();
                        blob.AddPoint(x, y);
                        stack.Push(new Point(x, y));
                        isSeen[z] = true;

                        while (stack.Count > 0)
                        {
                            Point p = stack.Pop();

                            for (int i = p.X - dilation; i <= p.X + dilation; ++i)
                            {
                                for (int j = p.Y - dilation; j <= p.Y + dilation; ++j)
                                {
                                    if (i < 0 || i >= width || j < 0 || j >= height)
                                    {
                                        continue;
                                    }

                                    int k = i * height + j;
                                    if (!isSeen[k] && pixels[k])
                                    {
                                        blob.AddPoint(i, j);
                                        stack.Push(new Point(i, j));
                                        isSeen[k] = true;
                                    }
                                }
                            }
                        }

                        if (blob.count > 20)
                        {
                            blobs.Add(blob);
                        }
                    }
                    else
                    {
                        isSeen[z] = true;
                    }
                }
            }

            return blobs;
        }

        private static bool[] Dilate(bool[] pixels, int width, int height, int dilation = 1)
        {
            bool[] newPixels = new bool[pixels.Length];

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    bool found = false;
                    for (int i = x - dilation; i <= x + dilation; ++i)
                    {
                        for (int j = y - dilation; j <= y + dilation; ++j)
                        {
                            if (i >= 0 && i < width && j >= 0 && j < height && pixels[i * height + j])
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            break;
                        }
                    }

                    if (found)
                    {
                        newPixels[x * height + y] = true;
                    }
                }
            }

            return newPixels;
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

        public static unsafe Bitmap ToBitmap(bool[] pixels, int width, int height)
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

        private static bool IsDifferent(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) > DiffThreshold || Math.Abs(a.G - b.G) > DiffThreshold || Math.Abs(a.B - b.B) > DiffThreshold;
        }

        private static bool IsLight(Color c)
        {
            return c.R > LightThreshold && c.G > LightThreshold && c.B > LightThreshold;
        }
    }
}
