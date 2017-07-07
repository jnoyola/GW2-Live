using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GW2_Live
{
    class MapView : PictureBox
    {
        private class Keypoint
        {
            public float X;
            public float Y;

            public Keypoint(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        private class Tri
        {
            public HashSet<Keypoint> points;

            public Tri(HashSet<Keypoint> points)
            {
                this.points = new HashSet<Keypoint>(points);
            }
        }

        private const float KeypointRadius = 4;
        private readonly Brush TriBrush = new SolidBrush(Color.FromArgb(150, 255, 100, 100));

        public bool IsEditing { get; set; } = false;

        private int offsetX;
        private int offsetY;
        private double centerX = -1;
        private double centerY = -1;
        private double vx = 0;
        private double vy = 0;
        private float scale = 1;
        private float trueScale;

        private List<Keypoint> keypoints = new List<Keypoint>();
        private List<Tri> tris = new List<Tri>();
        private HashSet<Keypoint> selectedKeypoints = new HashSet<Keypoint>();

        public void Reset()
        {
            centerX = -1;
            centerY = -1;
            scale = 1;
            this.Invalidate();
        }

        public void SetPlayerPosition(double x, double y, double vx, double vy)
        {
            centerX = x;
            centerY = y;
            this.vx = vx;
            this.vy = vy;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void AddKeypoint(float x, float y)
        {
            keypoints.Add(new Keypoint(x, y));
            this.Invalidate();
        }

        public void Select(int x, int y)
        {
            bool foundPoint = false;

            float r = KeypointRadius + 2;
            r = r * r;

            foreach (Keypoint p in keypoints)
            {
                float xx = (p.X * this.Image.Width + offsetX) * trueScale;
                float yy = (p.Y * this.Image.Height + offsetY) * trueScale;

                float dx = x - xx;
                float dy = y - yy;

                if (dx * dx + dy * dy <= r)
                {
                    selectedKeypoints.Add(p);
                    foundPoint = true;
                    break;
                }
            }

            if (foundPoint)
            {
                if (selectedKeypoints.Count == 3)
                {
                    tris.Add(new Tri(selectedKeypoints));
                    selectedKeypoints.Clear();
                }
            }
            else
            {
                selectedKeypoints.Clear();
            }

            this.Invalidate();
        }

        public void Remove(int x, int y)
        {

        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.Image != null)
            {
                trueScale = scale * Math.Min((float)this.Width / this.Image.Width, (float)this.Height / this.Image.Height);

                pe.Graphics.ScaleTransform(trueScale, trueScale);
                
                double cx = centerX < 0 ? 0.5 : centerX;
                double cy = centerY < 0 ? 0.5 : centerY;
                offsetX = (int)(this.Width / (2 * trueScale) - this.Image.Width * cx);
                offsetY = (int)(this.Width / (2 * trueScale) - this.Image.Height * cy);

                pe.Graphics.TranslateTransform(offsetX, offsetY);

                base.OnPaint(pe);

                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawTris(pe.Graphics);
                DrawPlayer(pe.Graphics);
                DrawKeypoints(pe.Graphics);
            }
            else
            {
                base.OnPaint(pe);
            }
        }

        private void DrawTris(Graphics g)
        {
            foreach (Tri t in tris)
            {
                int i = 0;
                PointF[] points = new PointF[3];

                foreach (Keypoint p in t.points)
                {
                    float x = this.Image.Width * p.X;
                    float y = this.Image.Height * p.Y;
                    points[i] = new PointF(x, y);
                    ++i;
                }

                g.FillPolygon(TriBrush, points);
            }
        }

        private void DrawPlayer(Graphics g)
        {
            double x = this.Image.Width * centerX;
            double y = this.Image.Height * centerY;

            double h = Math.Sqrt(vx * vx + vy * vy);
            double c = vx / h;
            double s = vy / h;

            double x0 = -18;
            double y0 = -15;
            double x1 = -18;
            double y1 = 15;
            double x2 = 20;
            double y2 = 0;

            RotatePoint(ref x0, ref y0, c, s);
            RotatePoint(ref x1, ref y1, c, s);
            RotatePoint(ref x2, ref y2, c, s);

            g.FillPolygon(
                Brushes.Black,
                new PointF[]
                {
                    new PointF((float)(x + x0), (float)(y - y0)),
                    new PointF((float)(x + x1), (float)(y - y1)),
                    new PointF((float)(x + x2), (float)(y - y2))
                });

            x0 = -15;
            y0 = -11;
            x1 = -15;
            y1 = 11;
            x2 = 15;
            y2 = 0;

            RotatePoint(ref x0, ref y0, c, s);
            RotatePoint(ref x1, ref y1, c, s);
            RotatePoint(ref x2, ref y2, c, s);

            g.FillPolygon(
                Brushes.LightGreen,
                new PointF[]
                {
                    new PointF((float)(x + x0), (float)(y - y0)),
                    new PointF((float)(x + x1), (float)(y - y1)),
                    new PointF((float)(x + x2), (float)(y - y2))
                });
        }

        private void RotatePoint(ref double x, ref double y, double c, double s)
        {
            double xx = x * c - y * s;
            y = x * s + y * c;
            x = xx;
        }

        private void DrawKeypoints(Graphics g)
        {
            foreach (Keypoint p in keypoints)
            {
                float x = this.Image.Width * p.X;
                float y = this.Image.Height * p.Y;

                g.FillEllipse(
                    Brushes.Black,
                    x - (KeypointRadius + 2),
                    y - (KeypointRadius + 2),
                    2 * (KeypointRadius + 2),
                    2 * (KeypointRadius + 2));

                g.FillEllipse(
                    selectedKeypoints.Contains(p) ? Brushes.LightGreen : Brushes.Red,
                    x - KeypointRadius,
                    y - KeypointRadius,
                    2 * KeypointRadius,
                    2 * KeypointRadius);
            }
        }
    }
}
