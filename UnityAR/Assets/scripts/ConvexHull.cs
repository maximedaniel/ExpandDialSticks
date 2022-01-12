using System;
using System.Collections.Generic;
 
namespace ConvexHull {
    class Point : IComparable<Point> {
        private float x, y;
 
        public Point(float x, float y) {
            this.x = x;
            this.y = y;
        }
 
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
 
        public int CompareTo(Point other) {
            return x.CompareTo(other.x);
        }
 
        public override string ToString() {
            return string.Format("({0}, {1})", x, y);
        }
    }
 
    class Program {
        public static List<Point> ConvexHull(List<Point> p) {
            if (p.Count == 0) return new List<Point>();
            p.Sort();
            List<Point> h = new List<Point>();
 
            // lower hull
            foreach (var pt in p) {
                while (h.Count >= 2 && !Ccw(h[h.Count - 2], h[h.Count - 1], pt)) {
                    h.RemoveAt(h.Count - 1);
                }
                h.Add(pt);
            }
 
            // upper hull
            int t = h.Count + 1;
            for (int i = p.Count - 1; i >= 0; i--) {
                Point pt = p[i];
                while (h.Count >= t && !Ccw(h[h.Count - 2], h[h.Count - 1], pt)) {
                    h.RemoveAt(h.Count - 1);
                }
                h.Add(pt);
            }
 
            h.RemoveAt(h.Count - 1);
            return h;
        }

        public static bool Ccw(Point a, Point b, Point c) {
            return ((b.X - a.X) * (c.Y - a.Y)) > ((b.Y - a.Y) * (c.X - a.X));
        }
    }
}