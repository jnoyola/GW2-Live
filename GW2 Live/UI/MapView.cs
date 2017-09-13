using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GW2_Live.Player;

namespace GW2_Live.UI
{
    class MapView : PictureBox
    {
        private const float KeypointRadius = 8;
        private readonly Brush TriBrush = new SolidBrush(Color.FromArgb(150, 255, 100, 100));
        private readonly Pen RoutePen = new Pen(Brushes.Black, 5);

        public bool IsEditing { get; set; } = false;

        public Plan Plan { get; set; }

        private int offsetX;
        private int offsetY;
        private float playerX = -1;
        private float playerY = -1;
        private float vx = 0;
        private float vy = 0;
        private float scale = 1;
        private float trueScale;

        private HashSet<Plan.Point> selectedKeypoints = new HashSet<Plan.Point>();
        private Plan.Point pointToRemove;
        private Plan.Tri triToRemove;

        public void Reset()
        {
            playerX = -1;
            playerY = -1;
            scale = 1;
            this.Invalidate();
        }

        public void SetPlayerPosition(float x, float y, float vx, float vy)
        {
            playerX = x;
            playerY = y;
            this.vx = vx;
            this.vy = vy;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public void ClearSelection()
        {
            selectedKeypoints.Clear();
            pointToRemove = null;
            triToRemove = null;
        }

        public void Select(int x, int y)
        {
            // Check if we have a Point or Tri to deselect.
            if (pointToRemove == null && triToRemove == null)
            {
                var p = GetPointAtPixel(x, y);

                if (p == null)
                {
                    ClearSelection();
                }
                else
                {
                    selectedKeypoints.Add(p);

                    if (selectedKeypoints.Count == 3)
                    {
                        Plan.AddTri(selectedKeypoints);
                        ClearSelection();
                    }
                }
            }
            else
            {
                ClearSelection();
            }

            this.Invalidate();
        }

        public void SpecialSelect(int x, int y)
        {
            ClearSelection();

            // TODO: this needs to be for a route point
            var p = GetPointAtPixel(x, y);

            if (p != null)
            {
                Plan.SetVendorPoint(p);
            }
        }

        public void Remove(int x, int y)
        {
            selectedKeypoints.Clear();

            var p = GetPointAtPixel(x, y);

            if (p != null)
            {
                if (p == pointToRemove)
                {
                    Plan.RemovePoint(p);
                    pointToRemove = null;
                }
                else
                {
                    pointToRemove = p;
                }
            }
            else
            {
                var t = GetTriAtPixel(x, y);

                if (t != null)
                {
                    if (t == triToRemove)
                    {
                        Plan.RemoveTri(t);
                        triToRemove = null;
                    }
                    else
                    {
                        triToRemove = t;
                    }
                }
            }
        }

        private Plan.Point GetPointAtPixel(int x, int y)
        {
            float rSquared = KeypointRadius * KeypointRadius;

            foreach (var p in Plan.Points)
            {
                // Convert each Point from Percent scale to Pixel scale,
                // where the points on the display are actually circles.
                float xx = (p.X * this.Image.Width + offsetX) * trueScale;
                float yy = (p.Y * this.Image.Height + offsetY) * trueScale;

                float dx = x - xx;
                float dy = y - yy;

                if (dx * dx + dy * dy <= rSquared)
                {
                    return p;
                }
            }

            return null;
        }

        private Plan.Tri GetTriAtPixel(int x, int y)
        {
            // Convert the point from Pixel scale to Percent scale,
            // which the Plan uses to store and compare all values.
            float xx = ((x / trueScale) - offsetX) / this.Image.Width;
            float yy = ((y / trueScale) - offsetY) / this.Image.Height;

            return Plan.GetTriContainingPoint(xx, yy);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.Image != null)
            {
                trueScale = scale * Math.Min((float)this.Width / this.Image.Width, (float)this.Height / this.Image.Height);

                pe.Graphics.ScaleTransform(trueScale, trueScale);

                float cx = playerX < 0 ? 0.5f : playerX;
                float cy = playerY < 0 ? 0.5f : playerY;
                offsetX = (int)(this.Width / (2 * trueScale) - this.Image.Width * cx);
                offsetY = (int)(this.Width / (2 * trueScale) - this.Image.Height * cy);

                pe.Graphics.TranslateTransform(offsetX, offsetY);

                base.OnPaint(pe);

                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawTris(pe.Graphics);
                DrawPlayer(pe.Graphics);
                DrawKeypoints(pe.Graphics);
                DrawRoute(pe.Graphics);
            }
            else
            {
                base.OnPaint(pe);
            }
        }

        private void DrawTris(Graphics g)
        {
            foreach (var t in Plan.Tris)
            {
                int i = 0;
                PointF[] points = new PointF[3];

                foreach (var p in t.Points)
                {
                    float x = this.Image.Width * p.X;
                    float y = this.Image.Height * p.Y;
                    points[i] = new PointF(x, y);
                    ++i;
                }

                g.FillPolygon(
                    t == triToRemove ? Brushes.Black : TriBrush,
                    points);
            }
        }

        private void DrawPlayer(Graphics g)
        {
            double x = this.Image.Width * playerX;
            double y = this.Image.Height * playerY;

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
            foreach (var p in Plan.Points)
            {
                float x = this.Image.Width * p.X;
                float y = this.Image.Height * p.Y;

                g.FillEllipse(
                    Brushes.Black,
                    x - (KeypointRadius),
                    y - (KeypointRadius),
                    2 * (KeypointRadius),
                    2 * (KeypointRadius));

                Brush brush;

                if (p == pointToRemove)
                {
                    brush = Brushes.Black;
                }
                else if (selectedKeypoints.Contains(p))
                {
                    brush = Brushes.LightGreen;
                }
                else
                {
                    brush = Brushes.Red;
                }

                g.FillEllipse(
                    brush,
                    x - (KeypointRadius - 2),
                    y - (KeypointRadius - 2),
                    2 * (KeypointRadius - 2),
                    2 * (KeypointRadius - 2));
            }
        }

        private void DrawRoute(Graphics g)
        {
            if (Plan.Route.Count >= 2)
            {
                PointF[] points = new PointF[Plan.Route.Count];

                for (int i = 0; i < Plan.Route.Count; ++i)
                {
                    var p = Plan.Route[i];
                    points[i] = new PointF(p.X * this.Image.Width, p.Y * this.Image.Height);
                }

                g.DrawLines(RoutePen, points);
            }
        }
    }
}
