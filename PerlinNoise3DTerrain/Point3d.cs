using System;
using System.Drawing;

namespace PerlinNoise3DTerrain {
    public class Point3d : IEquatable<Point3d> {
        public const double Epsilon = 0.01;
        public const double ToRad = Math.PI / 180.0;
        public const double ToDeg = 180.0 / Math.PI;
        public const double PI2 = 2.0 * Math.PI;

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3d() { }

        public Point3d(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3d(Point3d p) : this(p.X, p.Y, p.Z) { }

        public double Distance(Point3d p) {
            double dx = X - p.X;
            double dy = Y - p.Y;
            double dz = Z - p.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double Length { get => Math.Sqrt(X * X + Y * Y + Z * Z); }

        public double Dot(Point3d p) {
            return X * p.X + Y * p.Y + Z * p.Z;
        }

        public Point3d Cross(Point3d p) {
            return new Point3d((Y * p.Z) - (p.Y * Z),
                               (Z * p.X) - (p.Z * X),
                               (X * p.Y) - (p.X * Y));
        }

        public void Normalize() {
            double len = this.Length;
            if(len > 0) {
                X /= len;
                Y /= len;
                Z /= len;
            } else {
                X = 1.0;
                Y = 0.0;
                Z = 0.0;
            }
        }

        public Point3d Normalized() {
            Point3d p = new Point3d(this);
            Normalize();
            return p;
        }

        public Point3d RotateX(double a) {
            double cos = Math.Cos(a);
            double sin = Math.Sin(a);
            return new Point3d(X, Y * cos - Z * sin, Y * sin + Z * cos);
        }

        public Point3d RotateY(double a) {
            double cos = Math.Cos(a);
            double sin = Math.Sin(a);
            return new Point3d(Z * sin + X * cos, Y, Z * cos - X * sin);
        }

        public Point3d RotateZ(double a) {
            double cos = Math.Cos(a);
            double sin = Math.Sin(a);
            return new Point3d(X * cos - Y * sin, X * sin + Y * cos, Z);
        }

        public Point3d Project(int viewWidth, int viewHeight, double fov, double viewDistance) {
            double factor = viewDistance == -Z ? 999 : fov / (viewDistance + Z);
            return new Point3d(X * factor + viewWidth / 2, Y * factor + viewHeight / 2, Z);
        }

        public Point3d UnProject(int viewWidth, int viewHeight, double fov, double viewDistance) {
            double factor = viewDistance == -Z ? 999 : fov / (viewDistance + Z);
            return new Point3d((X - viewWidth / 2) / factor, (Y - viewHeight / 2) / factor, Z);
        }

        public PointF ToPointF() {
            return new PointF((float)X, (float)Y);
        }

        public static bool operator ==(Point3d p1, Point3d p2) {
            return (p1.X == p2.X) && (p1.Y == p2.Y);
        }

        public static bool operator !=(Point3d p1, Point3d p2) {
            return !(p1 == p2);
        }

        public static Point3d operator +(Point3d p1, Point3d p2) {
            return new Point3d(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Point3d operator -(Point3d p1, Point3d p2) {
            return new Point3d(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Point3d operator -(Point3d p1) {
            return new Point3d(p1.X, p1.Y, -p1.Z);
        }

        public static Point3d operator *(Point3d p1, double scalar) {
            return new Point3d(p1.X * scalar, p1.Y * scalar, p1.Z * scalar);
        }

        public static Point3d operator *(double scalar, Point3d p1) {
            return p1 * scalar;
        }

        public static Point3d operator /(Point3d p1, double scalar) {
            return p1 * (1 / scalar);
        }

        public double[] ToArray() {
            return new double[] { X, Y, Z };
        }

        public override string ToString() {
            return $"({X:F2}, {Y:F2}, {Z:F2})";
        }

        public Point3d AsInt(int padding = 0) {
            int X1 = (int)X;
            int Y1 = (int)Y;
            int Z1 = (int)Z;

            if(padding > 1) {
                X1 -= X1 % padding;
                Y1 -= Y1 % padding;
                Z1 -= Z1 % padding;
            }

            return new Point3d(X1, Y1, Z1);
        }

        public int Compare(Point3d p) {
            return Length.CompareTo(p.Length);
        }

        public override bool Equals(object obj) {
            return Equals(obj as Point3d);
        }

        public bool IsSimilar(Point3d p) {
            return Math.Abs(X - p.X) <= Epsilon &&
                   Math.Abs(Y - p.Y) <= Epsilon &&
                   Math.Abs(Z - p.Z) <= Epsilon;
        }

        public double AngleXY(Point3d p) {
            double a = Math.Atan2(p.Y - Y, p.X - X);
            if(a < 0) a += PI2;
            return a;
        }

        public double AngleXZ(Point3d p) {
            double a = Math.Atan2(p.Z - Z, p.X - X);
            if(a < 0) a += PI2;
            return a;
        }

        public double AngleYZ(Point3d p) {
            double a = Math.Atan2(p.Z - Z, p.Y - Y);
            if(a < 0) a += PI2;
            return a;
        }

        public bool Equals(Point3d other) {
            return this == other;
        }

        public override int GetHashCode() {
            int hashCode = 612420109;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }
    }
}