using System;
using System.Collections.Generic;

namespace PerlinNoise3DTerrain {
    public class Line3d : IEquatable<Line3d> {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public Point3d[] Vertices { get => new Point3d[] { Start, End }; }

        public Line3d(Point3d start, Point3d end) {
            Start = start;
            End = end;
        }

        public void Reverse() {
            Point3d tmp = Start;
            Start = End;
            End = tmp;
        }

        public static bool operator ==(Line3d l1, Line3d l2) {
            return (l1.Start == l2.Start && l1.End == l2.End) ||
                   (l1.Start == l2.End && l1.End == l2.Start);
        }

        public static bool operator !=(Line3d l1, Line3d l2) {
            return !(l1 == l2);
        }

        public static Line3d operator +(Line3d l1, Line3d l2) {
            return new Line3d(l1.Start + l2.Start, l1.End + l2.End);
        }

        public static Line3d operator +(Line3d l1, Point3d p) {
            return new Line3d(l1.Start + p, l1.End + p);
        }

        public static Line3d operator -(Line3d l1, Line3d l2) {
            return new Line3d(l1.Start - l2.Start, l1.End - l2.End);
        }

        public static Line3d operator -(Line3d l1, Point3d p) {
            return new Line3d(l1.Start - p, l1.End - p);
        }

        public static Line3d operator *(Line3d l1, double scalar) {
            return new Line3d(l1.Start * scalar, l1.End * scalar);
        }

        public static Line3d operator *(double scalar, Line3d l1) {
            return l1 * scalar;
        }

        public override string ToString() {
            return $"{Start} -> {End}";
        }

        public override bool Equals(object obj) {
            return Equals(obj as Line3d);
        }

        public bool Equals(Line3d other) {
            return this == other;
        }

        public override int GetHashCode() {
            int hashCode = 346539639;
            hashCode = hashCode * -1521134295 + EqualityComparer<Point3d>.Default.GetHashCode(Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Point3d>.Default.GetHashCode(End);
            hashCode = hashCode * -1521134295 + EqualityComparer<Point3d[]>.Default.GetHashCode(Vertices);
            return hashCode;
        }
    }
}