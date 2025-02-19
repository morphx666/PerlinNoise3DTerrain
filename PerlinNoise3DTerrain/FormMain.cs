using MorphxLibs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PerlinNoise3DTerrain {
    public partial class FormMain : Form {
        private double AngleX = 0.0;
        private double AngleY = 0.0;
        private double AngleZ = 0.0;

        private double fov = 512;
        private double distance = 300;

        private enum RenderModes {
            Filled = 0,
            BoundingBox = 1,
            WireFrame = 2
        }
        private RenderModes renderMode = RenderModes.Filled;

        private Point3d camera;

        private int w;
        private int h;

        private int scale = 10;
        private double sw;
        private double sw2;
        private double sh;
        private float ty;

        double distanceFactor;

        private double pNoiseXFactor = 200.0;
        private double pNoiseXOffset = 0.0;
        private double pNoiseYFactor = 200.0;
        private double pNoiseYOffset = 0.0;
        private double pNoiseZFactor = 300.0;
        private double pNoiseZOffset = 0.0;
        private double pNoiseFactorSpeed = 0.08;
        private double pNoiseZScale = 300.0;

        private int xMove = 0;
        private int yMove = 0;
        private int zMove = 0;
        private int zScale = 0;

        private readonly List<Line3d> ls = new List<Line3d>();
        private (SolidBrush Brush, Pen Pen)[] zPalette;

        private Point3d cp;
        private const double minAngle = 30 * Point3d.ToRad;
        private const double maxAngle = 140 * Point3d.ToRad;

        private bool hasChanged = true;
        private object lckObj = new object();
        private bool isInit = false;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Resize += (_, __) => Init();
            Init();

            CreateZPalette();
            PerlinNoise.Init();
            AddEventHandlers();
        }

        private void Init() {
            w = this.DisplayRectangle.Width;
            h = this.DisplayRectangle.Height;

            if(!isInit) {
                Random rnd = new Random();
                pNoiseXOffset = rnd.Next(w / 2);
                pNoiseYOffset = rnd.Next(h / 2);
                isInit = true;
            }

            sw = w * 2;
            sw2 = sw / 2;
            sh = h * 2;
            ty = h - h / 2 + (float)(pNoiseZFactor * 0.95);

            camera = new Point3d(0, 0, -100);

            distanceFactor = distance / 100 * camera.Z;
            AngleX = -3.7;// (90.0 + 45.0) * Point3d.ToRad;

            CreateGrid();
        }

        private Point mousePos;
        private bool isMouseDown;
        private void AddEventHandlers() {
            const int factor = 4;

            this.KeyDown += (object s, KeyEventArgs e) => {
                RenderModes currentRenderMode = renderMode;

                switch(e.KeyCode) {
                    case Keys.Left:
                        xMove = -factor;
                        break;
                    case Keys.Right:
                        xMove = factor;
                        break;
                    case Keys.Up:
                        yMove = factor;
                        break;
                    case Keys.Down:
                        yMove = -factor;
                        break;
                    case Keys.Q:
                        zMove = factor;
                        break;
                    case Keys.A:
                        zMove = -factor;
                        break;
                    case Keys.Add:
                        zScale = 6 * factor;
                        break;
                    case Keys.Subtract:
                        zScale = -6 * factor;
                        break;
                    case Keys.R:
                        renderMode++;
                        renderMode = (RenderModes)((int)renderMode % 3);
                        
                        // FIXME: Not cool...```
                        string helpText = LabelShortcuts.Text.Split(':')[0];
                        helpText += ": " + renderMode.ToString();
                        LabelShortcuts.Text = helpText;

                        break;
                }
                hasChanged = xMove != 0 || yMove != 0 || zMove != 0 || zScale != 0 || currentRenderMode != renderMode;
            };

            this.KeyUp += (object s, KeyEventArgs e) => {
                switch(e.KeyCode) {
                    case Keys.Left:
                    case Keys.Right:
                        xMove = 0;
                        break;
                    case Keys.Up:
                    case Keys.Down:
                        yMove = 0;
                        break;
                    case Keys.Q:
                    case Keys.A:
                        zMove = 0;
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                        zScale = 0;
                        break;
                }
                hasChanged = xMove != 0 || yMove != 0 || zMove != 0 || zScale != 0;
            };

            this.MouseWheel += (object s, MouseEventArgs e) => {
                hasChanged = true;
            };

            this.MouseDown += (object s, MouseEventArgs e) => {
                mousePos = e.Location;
                isMouseDown = e.Button == MouseButtons.Left;
            };
            this.MouseUp += (object s, MouseEventArgs e) => {
                isMouseDown = !(e.Button == MouseButtons.Left);
            };
            this.MouseMove += (object s, MouseEventArgs e) => {
                if(isMouseDown) {
                    AngleX -= (e.Location.Y - mousePos.Y) / 500.0;
                    AngleY += (e.Location.X - mousePos.X) / 500.0;
                    AngleZ += (e.Location.X - mousePos.X) / 500.0;
                    mousePos = e.Location;
                    hasChanged = true;
                }
            };

            this.Load += (object s, EventArgs e) => {
                Application.DoEvents();
                LabelShortcuts.Refresh();
                Application.DoEvents();

                Task.Run(async () => {
                    while(true) {
                        await Task.Delay(15);

                        if(hasChanged) {
                            hasChanged = false;

                            lock(lckObj) {
                                pNoiseXOffset += xMove * pNoiseFactorSpeed;
                                pNoiseYOffset += yMove * pNoiseFactorSpeed;
                                pNoiseZOffset += zMove * pNoiseFactorSpeed;
                                pNoiseZScale += zScale * pNoiseFactorSpeed * 4.0;

                                cp = TransformPoint(camera, project: false);
                            }

                            this.Invalidate();
                        }
                    }
                });
            };
        }

        private void CreateGrid() {
            ls.Clear();
            for(int y = 0; y < sh; y += scale) {
                for(int x = 0; x < sw; x += scale) {
                    Point3d p1 = new Point3d(x - sw2, y, 0);
                    Point3d p2 = new Point3d(x - sw2 + scale, y, 0);
                    Point3d p3 = new Point3d(x - sw2 + scale, y + scale, 0);
                    Point3d p4 = new Point3d(x - sw2, y + scale, 0);

                    Point3d tp1 = TransformPoint(p1);
                    Point3d tp2 = TransformPoint(p2);
                    Point3d tp3 = TransformPoint(p3);
                    Point3d tp4 = TransformPoint(p4);

                    if(true || (IsPointValid(tp1) && IsPointValid(tp2) &&
                       IsPointValid(tp3) && IsPointValid(tp4))) {
                        ls.Add(new Line3d(p1, p2));
                        ls.Add(new Line3d(p2, p3));
                        ls.Add(new Line3d(p2, p4));
                    }
                }
            }
        }

        private void CreateZPalette() {
            Color[] baseZPalette = new Color[] {
                                            Color.SlateBlue,
                                            Color.Blue,
                                            Color.Green,
                                            Color.DarkGreen,
                                            Color.Maroon,
                                            Color.SaddleBrown,
                                            Color.LightGray,
                                            Color.White
                                        };
            int numShades = baseZPalette.Length;

            zPalette = new (SolidBrush, Pen)[(baseZPalette.Length - 1) * numShades];

            double f1;
            double f2;
            Color c;

            for(int i = 0; i < baseZPalette.Length - 1; i++) {
                for(int j = 0; j < numShades; j++) {
                    f2 = j / (float)numShades;
                    f1 = 1 - f2;
                    c = Color.FromArgb(
                            (int)(baseZPalette[i].R * f1 + baseZPalette[i + 1].R * f2),
                            (int)(baseZPalette[i].G * f1 + baseZPalette[i + 1].G * f2),
                            (int)(baseZPalette[i].B * f1 + baseZPalette[i + 1].B * f2)
                        );
                    zPalette[i * numShades + j] = (new SolidBrush(c), new Pen(c));
                }
            }
        }

        private Point3d TransformPoint(Point3d p, bool rotate = true, bool project = true) {
            if(project) {
                if(rotate) return (p.RotateX(AngleX).
                                     RotateY(AngleY).
                                     RotateZ(AngleZ) - camera).
                                     Project(w, h, fov, distance);
                else return p.Project(w, h, fov, distance);
            } else {
                if(rotate) return p.RotateX(AngleX).
                                    RotateY(AngleY).
                                    RotateZ(AngleZ) - camera;
                else return p - camera;
            }
        }

        private Point3d GetPerlinZ(Point3d p) {
            p.Z = pNoiseZScale * PerlinNoise.Perlin(
                    w + p.X / pNoiseXFactor + pNoiseXOffset,
                    h + p.Y / pNoiseYFactor + pNoiseYOffset,
                    w + 1.0 / pNoiseZFactor + (pNoiseZOffset / 10.0));
            return p;
        }

        private bool IsPointValid(Point3d p) {
            return (p.X >= 0 && p.X <= w) &&
                   (p.Y >= -h && p.Y <= h) &&
                   (p.Z >= distanceFactor);
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.TranslateTransform(0, ty);           

            lock(lckObj) {
                // Cycle through all lines in reverse z-order (from farthest to closest)
                for(int i = ls.Count - 3; i >= 0; i -= 3) {
                    Point3d p1 = GetPerlinZ(ls[i + 0].Start);
                    Point3d p2 = GetPerlinZ(ls[i + 0].End);
                    Point3d p3 = GetPerlinZ(ls[i + 1].End);
                    Point3d p4 = GetPerlinZ(ls[i + 2].End);

                    Point3d tp1 = TransformPoint(p1);
                    Point3d tp2 = TransformPoint(p2);
                    Point3d tp3 = TransformPoint(p3);
                    Point3d tp4 = TransformPoint(p4);

                    Point3d n = tp1.Cross(tp2) +
                                tp2.Cross(tp3) +
                                tp3.Cross(tp4) +
                                tp4.Cross(tp1); // Normal
                    double a = n.AngleXZ2(cp);
                    if(a >= minAngle && a <= maxAngle && // Don't render faces facing away from the camera
                        IsPointValid(tp1) || IsPointValid(tp2) ||
                        IsPointValid(tp3) || IsPointValid(tp4)) {

                        double az = (p1.Z + p2.Z + p3.Z + p4.Z) / 4.0;
                        int pi = (int)(Math.Abs(zPalette.Length * az / pNoiseZScale)) % zPalette.Length;

                        switch(renderMode) {
                            case RenderModes.Filled:
                                g.FillPolygon(zPalette[pi].Brush, new PointF[] {
                                                                    tp1.ToPointF(),
                                                                    tp2.ToPointF(),
                                                                    tp3.ToPointF(),
                                                                    tp4.ToPointF()
                                                                });
                                break;

                            case RenderModes.BoundingBox:
                                g.DrawPolygon(zPalette[pi].Pen, new PointF[] {
                                                                    tp1.ToPointF(),
                                                                    tp2.ToPointF(),
                                                                    tp3.ToPointF(),
                                                                    tp4.ToPointF()
                                                                });
                                break;

                            case RenderModes.WireFrame:
                                g.DrawLine(Pens.Gainsboro, tp1.ToPointF(), tp2.ToPointF()); // -
                                g.DrawLine(Pens.Gainsboro, tp2.ToPointF(), tp3.ToPointF()); // |
                                g.DrawLine(Pens.Gainsboro, tp2.ToPointF(), tp4.ToPointF()); // /
                                break;
                        }
                    }
                }
            }
        }
    }
}