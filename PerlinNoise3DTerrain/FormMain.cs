using MorphxLibs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PerlinNoise3DTerrain {
    public partial class FormMain : Form {
        private double AngleX = 0.0;
        private double AngleY = 0.0;
        private double AngleZ = 0.0;

        private double fov = 512.0;
        private double distance = 400.0;

        private Point3d camera = new Point3d(0, 0, -100);

        private int w;
        private int h;

        private int scale = 20;
        private double sw;
        private double sw2;
        private double sh;

        double distanceFactor;

        private double pNoiseXFactor = 200.0;
        private double pNoiseXOffset = 0.0;
        private double pNoiseYFactor = 200.0;
        private double pNoiseYOffset = 0.0;
        private double pNoiseZFactor = 300.0;
        private double pNoiseZOffset = 0.0;
        private double pNoiseFactorSpeed = 0.15;

        private int xMove = 0;
        private int yMove = 0;
        private int zMove = 0;

        private List<Line3d> ls = new List<Line3d>();
        private Brush[] zPalette;

        private bool hasChanged = false;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            w = this.DisplayRectangle.Width;
            h = this.DisplayRectangle.Height;

            sw = w * 4.0;
            sw2 = sw / 2;
            sh = h * 4.0;
            distanceFactor = distance / 100 * camera.Z;

            AngleX = (90.0 + 55.0) * Point3d.ToRad;

            CreateGrid();
            CreateZPalette();
            PerlinNoise.Init();

            Task.Run(() => {
                while(true) {
                    Thread.Sleep(33);
                    if(hasChanged) {
                        this.Invalidate();

                        pNoiseXOffset += xMove * pNoiseFactorSpeed;
                        pNoiseYOffset += yMove * pNoiseFactorSpeed;
                        pNoiseZOffset += zMove * pNoiseFactorSpeed;
                    }
                }
            });

            AddEventHandlers();
        }

        private void AddEventHandlers() {
            this.KeyDown += (object o, KeyEventArgs e) => {
                switch(e.KeyCode) {
                    case Keys.Left:
                        xMove = -1;
                        break;
                    case Keys.Right:
                        xMove = 1;
                        break;
                    case Keys.Up:
                        yMove = 1;
                        break;
                    case Keys.Down:
                        yMove = -1;
                        break;
                    case Keys.Add:
                        zMove = 1;
                        break;
                    case Keys.Subtract:
                        zMove = -1;
                        break;
                }
                hasChanged = xMove != 0 || yMove != 0 || zMove != 0;
            };

            this.KeyUp += (object o, KeyEventArgs e) => {
                switch(e.KeyCode) {
                    case Keys.Left:
                    case Keys.Right:
                        xMove = 0;
                        break;
                    case Keys.Up:
                    case Keys.Down:
                        yMove = 0;
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                        zMove = 0;
                        break;
                }
                hasChanged = xMove != 0 || yMove != 0 || zMove != 0;
            };
        }

        private void CreateGrid() {
            for(int y = 0; y < sh; y += scale) {
                for(int x = 0; x < sw; x += scale) {
                    Point3d p1 = new Point3d(x - sw2, y, 0);
                    Point3d p2 = new Point3d(x - sw2 + scale, y, 0);
                    Point3d p3 = new Point3d(x - sw2 + scale, y + scale, 0);
                    Point3d p4 = new Point3d(x - sw2, y + scale, 0);

                    ls.Add(new Line3d(p1, p2));
                    ls.Add(new Line3d(p2, p3));
                    ls.Add(new Line3d(p2, p4));
                }
            }
        }

        private void CreateZPalette() {
            int numShades = 8;
            Color[] baseZPalette = new Color[] {
                                            Color.SlateBlue,
                                            Color.Blue,
                                            Color.DarkGreen,
                                            Color.Green,
                                            Color.Brown,
                                            Color.LightGray,
                                            Color.White
                                        };

            zPalette = new Brush[(baseZPalette.Length - 1) * numShades];

            double f1;
            double f2;

            for(int i = 0; i < baseZPalette.Length - 1; i++) {
                for(int j = 0; j < numShades; j++) {
                    f2 = j / (float)numShades;
                    f1 = 1 - f2;
                    zPalette[i * numShades + j] = new SolidBrush(Color.FromArgb(
                            (int)(baseZPalette[i].R * f1 + baseZPalette[i + 1].R * f2),
                            (int)(baseZPalette[i].G * f1 + baseZPalette[i + 1].G * f2),
                            (int)(baseZPalette[i].B * f1 + baseZPalette[i + 1].B * f2)
                        ));
                }
            }
        }

        private Point3d TransformPoint(Point3d p, bool rotate = true, bool project = true) {
            if(project)
                if(rotate)
                    return (p.RotateX(AngleX).
                              RotateY(AngleY).
                              RotateZ(AngleZ) - camera).
                              Project(w, h, fov, distance);
                else
                    return p.Project(w, h, fov, distance);
            else
                if(rotate)
                return p.RotateX(AngleX).
                         RotateY(AngleY).
                         RotateZ(AngleZ) - camera;
            else
                return p - camera;

        }

        private Point3d GetPerlinZ(Point3d p) {
            return new Point3d(p.X, p.Y,
                pNoiseZFactor * PerlinNoise.Perlin(
                    w + (p.X + sw2) / pNoiseXFactor + pNoiseXOffset,
                    h + p.Y / pNoiseYFactor + pNoiseYOffset,
                    1.0 / pNoiseZFactor + (pNoiseZOffset / 10)));
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.Clear(Color.SkyBlue);

            g.TranslateTransform(0, h - h / 2 + (float)(pNoiseZFactor * 0.95));

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

                if(tp1.Z >= distanceFactor && tp2.Z >= distanceFactor &&
                   tp3.Z >= distanceFactor && tp4.Z >= distanceFactor) {

                    //Simulate z-buffering by painting black the inside of the two triangles
                    double az = (p1.Z + p2.Z + p3.Z + p4.Z) / 4;
                    int ci = (int)(zPalette.Length * az / pNoiseZFactor);
                    g.FillPolygon(zPalette[ci], new PointF[] {
                                                        tp1.ToPointF(),
                                                        tp2.ToPointF(),
                                                        tp3.ToPointF(),
                                                        tp4.ToPointF()
                                                    });

                    //g.DrawLine(Pens.Gainsboro, tp1.ToPointF(), tp2.ToPointF()); // -
                    //g.DrawLine(Pens.Gainsboro, tp2.ToPointF(), tp3.ToPointF()); //  |
                    //g.DrawLine(Pens.Gainsboro, tp2.ToPointF(), tp4.ToPointF()); // /
                }
            }

            //AngleX += 2.0 * Point3d.ToRad;
        }
    }
}