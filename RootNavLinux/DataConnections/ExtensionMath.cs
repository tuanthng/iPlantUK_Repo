using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RootNav.Data
{
    /// <summary>
    /// Static class providing some additional maths algorithms.
    /// </summary>
    public static class ExtentionMath
    {
        public static float SquareDistance(Point p1, Point p2)
        {
			int xDiff = p1.X - p2.X;
			int yDiff = p1.Y - p2.Y;

			return (float)Math.Pow((xDiff * xDiff + yDiff*yDiff), 2);
        }
		public static float SquareDistance(PointF p1, PointF p2)
		{
			float xDiff = p1.X - p2.X;
			float yDiff = p1.Y - p2.Y;

			return (float)Math.Pow((xDiff * xDiff + yDiff*yDiff), 2);
		}
		public static float Lenght(Point p1, Point p2)
		{
			int xDiff = p1.X - p2.X;
			int yDiff = p1.Y - p2.Y;

			return (float)Math.Sqrt(xDiff * xDiff + yDiff*yDiff);
		}
		public static float Lenght(PointF p1, PointF p2)
		{
			float xDiff = p1.X - p2.X;
			float yDiff = p1.Y - p2.Y;

			return (float)Math.Sqrt(xDiff * xDiff + yDiff*yDiff);
		}
    }
}
