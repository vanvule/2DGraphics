using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Paint
{
    class Transformer
    {
        public static Shape Transform(string type, Shape shape, Point pOld, Point pNew)
        {
            Shape newShape = null;

            double X_offset = pNew.X - pOld.X;
            double Y_offset = pNew.Y - pOld.Y;

            Matrix matrix = new Matrix();

            if (type == "Translate")
            {
                matrix.Translate(X_offset, Y_offset);

                newShape = ShapeFactory.CrateShape(
                    shape.Type(),
                    shape.StrokeColor,
                    shape.FillColor
                );

                Point[] points = new Point[shape.ControlPoints.Count];
                shape.ControlPoints.CopyTo(points);
                matrix.Transform(points);
                newShape.ControlPoints.AddRange(points);
            }
            else if (type == "Rotate")
            {
                Point center = shape.Center();
                double a = Shape.Euclid(center, pOld);
                double b = Shape.Euclid(center, pNew);
                double c = Shape.Euclid(pOld, pNew);
                double cos = (a * a + b * b - c * c) / (2 * a * b);
                double angle = Math.Acos(cos);
                angle *= (180.0 / Math.PI);

                if (pNew.X > pOld.X)
                    angle = -angle;

                Debug.WriteLine($"{angle}");

                matrix.RotateAt(angle, center.X, center.Y);

                newShape = ShapeFactory.CrateShape(
                    shape.Type(),
                    shape.StrokeColor,
                    shape.FillColor
                );

                Point[] points = new Point[shape.ControlPoints.Count];
                shape.ControlPoints.CopyTo(points);
                matrix.Transform(points);
                newShape.ControlPoints.AddRange(points);
            }
            else if (type == "Scale")
            {
                Point center = shape.Center();
                double oldDistance = Shape.Euclid(center, pOld);
                double newDistance = Shape.Euclid(center, pNew);
                double scale = newDistance / oldDistance;

                matrix.ScaleAt(scale, scale, center.X, center.Y);

                newShape = ShapeFactory.CrateShape(
                    shape.Type(),
                    shape.StrokeColor,
                    shape.FillColor
                );

                Point[] points = new Point[shape.ControlPoints.Count];
                shape.ControlPoints.CopyTo(points);
                matrix.Transform(points);
                newShape.ControlPoints.AddRange(points);
            }

            return newShape;
        }
    }
}
