using SharpGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Paint
{
    public abstract class Shape
    {
        public Color? StrokeColor { get; set; }
        public Color? FillColor { get; set; }
        public bool Visible { get; set; } = true;
        public List<Point> ControlPoints { get; set; } = new List<Point>();

        public virtual Point Center()
        {
            Point center = new Point();

            double sum_X = 0.0;
            double sum_Y = 0.0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                sum_X += ControlPoints[i].X;
                sum_Y += ControlPoints[i].Y;
            }

            center.X = sum_X / ControlPoints.Count;
            center.Y = sum_Y / ControlPoints.Count;

            return center;
        }

        public virtual bool Contains(Point point)
        {
            return false;
        }

        public abstract void Draw(ref OpenGL gl);

        public abstract string Type();

        

        static int INF = 10000;

        protected static bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) &&
                q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) &&
                q.Y >= Math.Min(p.Y, r.Y))
            {
                return true;
            }
            return false;
        }

        protected static int orientation(Point p, Point q, Point r)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0)
            {
                return 0; // colinear 
            }
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        protected static bool doIntersect(Point p1, Point q1,
                        Point p2, Point q2)
        {
            // Find the four orientations needed for  
            // general and special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // Special Cases 
            // p1, q1 and p2 are colinear and 
            // p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1))
            {
                return true;
            }

            // p1, q1 and p2 are colinear and 
            // q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1))
            {
                return true;
            }

            // p2, q2 and p1 are colinear and 
            // p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2))
            {
                return true;
            }

            // p2, q2 and q1 are colinear and 
            // q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2))
            {
                return true;
            }

            // Doesn't fall in any of the above cases 
            return false;
        }

        // Returns true if the point p lies  
        // inside the polygon[] with n vertices 
        protected static bool isInside(Point[] polygon, Point p)
        {
            int n = polygon.Length;
            // There must be at least 3 vertices in polygon[] 
            if (n < 3)
            {
                return false;
            }

            // Create a point for line segment from p to infinite 
            Point extreme = new Point(INF, p.Y);

            // Count intersections of the above line  
            // with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to  
                // 'extreme' intersects with the line  
                // segment from 'polygon[i]' to 'polygon[next]' 
                if (doIntersect(polygon[i],
                                polygon[next], p, extreme))
                {
                    // If the point 'p' is colinear with line  
                    // segment 'i-next', then check if it lies  
                    // on segment. If it lies, return true, otherwise false 
                    if (orientation(polygon[i], p, polygon[next]) == 0)
                    {
                        return onSegment(polygon[i], p,
                                         polygon[next]);
                    }
                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise 
            return (count % 2 == 1); // Same as (count%2 == 1) 
        }

        public static double Euclid(Point A, Point B)
        {
            return Math.Sqrt((A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y));
        }


        public static List<Point> Bresenham(Point A, Point B)
        {
            List<Point> points = new List<Point>();

            Point temp = new Point();

            double a_x = A.X;
            double a_y = A.Y;
            double b_x = B.X;
            double b_y = B.Y;

            //  Vẽ điểm bắt đầu
            temp.X = (int)a_x;
            temp.Y = (int)a_y;
            points.Add(temp);

            if (a_x > b_x || a_y > b_y)
            {
                double tmp = a_x;
                a_x = b_x;
                b_x = tmp;
                tmp = a_y;
                a_y = b_y;
                b_y = tmp;
            }

            int x_unit = 1, Dx = (int)(b_x - a_x), Dy = (int)(b_y - a_y); // Init first value
            int x = (int)a_x;
            int y = (int)a_y;
            int y_unit = 1;

            if (Dx < 0)
                x_unit = -1;
            if (Dy < 0)
                y_unit = -1;


            if (Math.Abs(b_y - a_y) < Math.Abs(b_x - a_x))
            {
                float p = 2 * Dy - Dx;
                for (int i = 0; i < Math.Abs(Dx); i++)
                {
                    if (p >= 0)
                    {
                        x += x_unit;
                        y += y_unit;
                        p += 2 * Math.Abs(Dy) - 2 * Math.Abs(Dx);
                        temp.X = x;
                        temp.Y = y;
                        points.Add(temp);
                    }
                    else
                    {
                        x += x_unit;
                        p += 2 * Math.Abs(Dy);
                        temp.X = x;
                        temp.Y = y;
                        points.Add(temp);
                    }
                }
            }
            else
            {
                int kt = (int)((b_x - a_x) * (b_y - a_y));
                int x1 = kt >= 0 ? (int)b_y : (int)(-b_y);
                double y1 = kt >= 0 ? b_x : -b_x;
                x = kt >= 0 ? (int)a_y : (int)(-a_y);
                y = kt >= 0 ? (int)a_x : (int)(-a_x);
                Dx = x1 - x;
                Dy = (int)(y1 - y);
                float p = 2 * Dy - Dx;
                for (int i = 0; i < Math.Abs(Dx); i++)
                {
                    if (p >= 0)
                    {
                        x += x_unit;
                        y += y_unit;
                        p += 2 * Math.Abs(Dy) - 2 * Math.Abs(Dx);
                        temp.X = kt >= 0 ? y : -y;
                        temp.Y = kt >= 0 ? x : -x;
                        points.Add(temp);
                    }
                    else
                    {
                        x += x_unit;
                        p += 2 * Math.Abs(Dy);
                        temp.X = kt >= 0 ? y : -y;
                        temp.Y = kt >= 0 ? x : -x;
                        points.Add(temp);
                    }
                }
            }

            return points;
        }


        public static void Bresenham(ref OpenGL gl, double A_X, double A_Y, double B_X, double B_Y)
        {
            Point A = new Point() { X = A_X, Y = A_Y };
            Point B = new Point() { X = B_X, Y = B_Y };

            List<Point> points = Shape.Bresenham(A, B);

            gl.Begin(OpenGL.GL_POINTS);
            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();
        }


        public void ScanFill(List<Point> point, Color color_fill, ref OpenGL gl)
        {
            gl.Color(color_fill.R / 255.0, color_fill.G / 255.0, color_fill.B / 255.0, 0);
            gl.PointSize(1.0f);

            point.Sort((left, right) => Convert.ToInt32(left.X - right.X));

            int i = 0, size_list = point.Count;

            List<int> x_Each_y = new List<int>();   //Danh sách dùng để lưu lại Y các phần tử có X giống nhau
            while (i < size_list - 1)                 //Xử lý tô màu
            {
                if (point[i].X == point[i + 1].X)
                {
                    int j = i, min_x, max_x;

                    while (j < size_list)           //Nạp Y các phần tử có X giống nhau
                    {
                        if (point[i].X == point[j].X)
                            x_Each_y.Add((int)point[j].Y);
                        else
                            break;
                        j++;
                    }

                    for (int u = 0; u < x_Each_y.Count - 1; u++)  //Sắp xếp tăng dần
                    {
                        for (int v = u + 1; v < x_Each_y.Count; v++)
                        {
                            if (x_Each_y[u] > x_Each_y[v])
                            {
                                int t = x_Each_y[u];
                                x_Each_y[u] = x_Each_y[v];
                                x_Each_y[v] = t;
                            }
                        }
                    }
                    if (x_Each_y.Count > 1)   //Chọn cặp điểm để tô
                    {
                        for (int t = 0; t < x_Each_y.Count - 1; t++)
                        {
                            if (x_Each_y[t + 1] - x_Each_y[t] > 1)
                            {
                                min_x = x_Each_y[t];
                                max_x = x_Each_y[t + 1];
                                Bresenham(ref gl, point[i].X, min_x + 1, point[i].X, max_x - 1);
                                break;
                            }
                        }
                    }

                    x_Each_y.Clear();
                    i = j;
                }
                else
                    i++;
            }
        }
    }


    public class ShapeFactory
    {
        public static Shape CrateShape(string type, Point pStart, Point pEnd, Color? strokeColor, Color? fillColor)
        {
            Shape shape = null;

            if (type == "Line")
            {
                shape = new Line(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Circle")
            {
                shape = new Circle(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Ellipse")
            {
                shape = new Ellipse(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Triangle")
            {
                shape = new Triangle(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Rectangle")
            {
                shape = new Rectangle(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Pentagon")
            {
                shape = new Pentagon(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Hexagon")
            {
                shape = new Hexagon(pStart, pEnd)
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }

            return shape;
        }

        public static Shape CrateShape(string type, Color? strokeColor, Color? fillColor)
        {
            Shape shape = null;

            if (type == "Line")
            {
                shape = new Line()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Circle")
            {
                shape = new Circle()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Ellipse")
            {
                shape = new Ellipse()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Triangle")
            {
                shape = new Triangle()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Rectangle")
            {
                shape = new Rectangle()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Pentagon")
            {
                shape = new Pentagon()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Hexagon")
            {
                shape = new Hexagon()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }
            else if (type == "Polygon")
            {
                shape = new Polygon()
                {
                    StrokeColor = strokeColor,
                    FillColor = fillColor
                };
            }

            return shape;
        }

    }


    public class Line : Shape
    {
        public Line() {  }

        public Line(Point a, Point b)
        {
            ControlPoints.Add(a);
            ControlPoints.Add(b);
        }

        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = Shape.Bresenham(
                ControlPoints[0], 
                ControlPoints[1]
            );

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();
        }

        public override bool Contains(Point point)
        {
            Point A = ControlPoints[0];
            Point B = ControlPoints[1];

            double m = (B.Y - A.Y) / (B.X - A.X);

            Point p = new Point((int) point.X, (int) point.Y);
            return Bresenham(
                ControlPoints[0],
                ControlPoints[1]
            ).Contains(p);
        }

        public override string Type()
        {
            return "Line";
        }
    }


    public class Circle : Shape
    {
        public Circle() { }

        public Circle(Point a, Point b)
        {
            int r = (int)Math.Min(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)) / 2;
            int tamI_X = (int)Math.Min(a.X, b.X) + r;
            int tamI_Y = (int)Math.Min(a.Y, b.Y) + r;

            ControlPoints.Add(new Point(tamI_X, tamI_Y));
            ControlPoints.Add(new Point(tamI_X + r, tamI_Y));
            ControlPoints.Add(new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)));
            ControlPoints.Add(new Point(tamI_X - r, tamI_Y));
            ControlPoints.Add(new Point(tamI_X, tamI_Y + r));
            ControlPoints.Add(new Point(tamI_X, tamI_Y - r));
        }


        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();
            Point temp = new Point();

            int r = (int) ControlPoints[1].X - (int) ControlPoints[0].X;

            int xi = 0,
                yi = r;
            int tamI_X = (int) ControlPoints[0].X;
            int tamI_Y = (int) ControlPoints[0].Y;
            int p = 1 - r;

            temp.X = ControlPoints[2].X;
            temp.Y = ControlPoints[2].Y;
            temp.X += 2 * r;
            temp.Y += 2 * r;
            temp.X -= 2 * r;


            points.Add(new Point(xi + tamI_X, yi + tamI_Y));
            temp.X = xi + tamI_X;
            temp.Y = yi + tamI_Y;
            points.Add(new Point(yi + tamI_X, xi + tamI_Y));
            temp.X = yi + tamI_X;
            temp.Y = xi + tamI_Y;
            points.Add(new Point(yi + tamI_X, -xi + tamI_Y));
            temp.X = yi + tamI_X;
            temp.Y = -xi + tamI_Y;
            points.Add(new Point(xi + tamI_X, -yi + tamI_Y));
            temp.X = xi + tamI_X;
            temp.Y = -yi + tamI_Y;
            points.Add(new Point(-xi + tamI_X, -yi + tamI_Y));
            temp.X = -xi + tamI_X;
            temp.Y = -yi + tamI_Y;
            points.Add(new Point(-yi + tamI_X, -xi + tamI_Y));
            temp.X = -yi + tamI_X;
            temp.Y = -xi + tamI_Y;
            points.Add(new Point(-yi + tamI_X, xi + tamI_Y));
            temp.X = -yi + tamI_X;
            temp.Y = xi + tamI_Y;
            points.Add(new Point(-xi + tamI_X, yi + tamI_Y));
            temp.X = -xi + tamI_X;
            temp.Y = yi + tamI_Y;


            while (xi < yi)
            {
                if (p < 0)
                {
                    p += 2 * xi + 3;
                }
                else
                {
                    p += 2 * xi - 2 * yi + 5;
                    yi--;
                }
                xi++;
                points.Add(new Point(xi + tamI_X, yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = yi + tamI_Y;
                points.Add(new Point(yi + tamI_X, xi + tamI_Y));
                temp.X = yi + tamI_X;
                temp.Y = xi + tamI_Y;
                points.Add(new Point(yi + tamI_X, -xi + tamI_Y));
                temp.X = yi + tamI_X;
                temp.Y = -xi + tamI_Y;
                points.Add(new Point(xi + tamI_X, -yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, -yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-yi + tamI_X, -xi + tamI_Y));
                temp.X = -yi + tamI_X;
                temp.Y = -xi + tamI_Y;
                points.Add(new Point(-yi + tamI_X, xi + tamI_Y));
                temp.X = -yi + tamI_X;
                temp.Y = xi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = yi + tamI_Y;
            }

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            int r = (int)ControlPoints[1].X - (int)ControlPoints[0].X;
            double d = Shape.Euclid(ControlPoints[0], point);
            return d <= r;
        }

        public override Point Center()
        {
            return ControlPoints[0];
        }

        public override string Type()
        {
            return "Circle";
        }
    }


    public class Ellipse : Shape
    {
        public Ellipse() { }

        public Ellipse(Point a, Point b)
        {
            int rx = (int)Math.Abs(a.X - b.X) / 2;
            int ry = (int)Math.Abs(a.Y - b.Y) / 2;

            int tamI_X = (int)Math.Min(a.X, b.X) + rx;
            int tamI_Y = (int)Math.Min(a.Y, b.Y) + ry;

            ControlPoints.Add(new Point(tamI_X, tamI_Y));
            ControlPoints.Add(new Point(tamI_X + rx, tamI_Y));
            ControlPoints.Add(new Point(tamI_X, tamI_Y + ry));
            ControlPoints.Add(new Point(tamI_X - rx, tamI_Y));
            ControlPoints.Add(new Point(tamI_X, tamI_Y - ry));
        }

        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();
            Point temp = new Point();

            int rx = (int) ControlPoints[1].X - (int) ControlPoints[0].X;
            int ry = (int) ControlPoints[2].Y - (int)ControlPoints[0].Y;
            int xi = 0,
                yi = ry;
            int tamI_X = (int) ControlPoints[0].X;
            int tamI_Y = (int) ControlPoints[0].Y;

            points.Add(new Point(xi + tamI_X, yi + tamI_Y));
            temp.X = xi + tamI_X;
            temp.Y = yi + tamI_Y;
            points.Add(new Point(rx + tamI_X, xi + tamI_Y));
            temp.X = rx + tamI_X;
            temp.Y = xi + tamI_Y;
            points.Add(new Point(xi + tamI_X, -yi + tamI_Y));
            temp.X = xi + tamI_X;
            temp.Y = -yi + tamI_Y;
            points.Add(new Point(-rx + tamI_X, xi + tamI_Y));
            temp.X = -rx + tamI_X;
            temp.Y = xi + tamI_Y;

            int p, ry2_x_2, rx2_y_2;
            ry2_x_2 = 2 * ry * ry * xi;
            rx2_y_2 = 2 * rx * rx * yi;

            p = ry * ry - rx * rx * ry + rx * rx;

            //ve nhanh thu 1(tu tren xuong )
            while (rx2_y_2 > ry2_x_2)
            {
                if (p < 0)
                {
                    ry2_x_2 += 2 * ry * ry;
                    p += ry2_x_2 + ry * ry;
                }
                else
                {
                    ry2_x_2 += 2 * ry * ry;
                    rx2_y_2 -= 2 * rx * rx;
                    p += ry2_x_2 - rx2_y_2 + ry * ry;
                    yi--;
                }
                xi++;
                points.Add(new Point(xi + tamI_X, yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = yi + tamI_Y;
                points.Add(new Point(xi + tamI_X, -yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, -yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = yi + tamI_Y;
            }

            //ve nhanh thu 2
            p = ry * ry * (xi + 1 / 2) * (xi + 1 / 2) + rx * rx * (yi - 1) * (yi - 1) - rx * rx * ry * ry;
            while (yi > 0)
            {
                if (p >= 0)
                {
                    rx2_y_2 -= 2 * rx * rx;
                    p -= rx2_y_2 + rx * rx;
                }
                else
                {
                    ry2_x_2 += 2 * ry * ry;
                    rx2_y_2 -= 2 * rx * rx;
                    p += ry2_x_2 - rx2_y_2 + rx * rx;
                    xi++;
                }

                yi--;
                points.Add(new Point(xi + tamI_X, yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = yi + tamI_Y;
                points.Add(new Point(xi + tamI_X, -yi + tamI_Y));
                temp.X = xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, -yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = -yi + tamI_Y;
                points.Add(new Point(-xi + tamI_X, yi + tamI_Y));
                temp.X = -xi + tamI_X;
                temp.Y = yi + tamI_Y;
            }

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            int rx = (int)ControlPoints[1].X - (int)ControlPoints[0].X;
            int ry = (int)ControlPoints[2].Y - (int)ControlPoints[0].Y;
            int tamI_X = (int)ControlPoints[0].X;
            int tamI_Y = (int)ControlPoints[0].Y;

            double x_diff = (point.X - tamI_X);
            double y_diff = (point.Y - tamI_Y);

            double d = (x_diff * x_diff) / (rx * rx) +
                (y_diff * y_diff) / (ry * ry);

            return d <= 1.0;
        }

        public override Point Center()
        {
            return ControlPoints[0];
        }

        public override string Type()
        {
            return "Ellipse";
        }

    }


    public class Triangle : Shape
    {
        public Triangle() { }

        public Triangle(Point a, Point b)
        {
            Point[] vertex = new Point[3];
            Point temp = new Point();
            double canh = Math.Min(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

            temp.X = a.X;
            temp.Y = a.Y;

            vertex[0].X = temp.X;
            vertex[0].Y = temp.Y;

            temp.X = (int)(a.X > b.X ? (a.X - canh) : (a.X + canh));

            vertex[1].X = temp.X;
            vertex[1].Y = temp.Y;

            temp.Y = (int)(a.Y > b.Y ? a.Y - canh * Math.Sqrt(3) / 2 : a.Y + canh * Math.Sqrt(3) / 2);
            temp.X = (int)(a.X > b.X ? (a.X - canh / 2) : (a.X + canh / 2));

            vertex[2].X = temp.X;
            vertex[2].Y = temp.Y;

            ControlPoints.AddRange(vertex);
        }

        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();

            for (int i = 0; i < ControlPoints.Count - 1; i++)
                points.AddRange(Bresenham(ControlPoints[i], ControlPoints[i + 1]));
            points.AddRange(Bresenham(ControlPoints[ControlPoints.Count - 1], ControlPoints[0]));

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            return isInside(ControlPoints.ToArray(), point);
        }

        public override string Type()
        {
            return "Triangle";
        }
    }


    public class Rectangle : Shape
    {
        public Rectangle() { }

        public Rectangle(Point a, Point b)
        {
            ControlPoints.Add(new Point(a.X, a.Y));
            ControlPoints.Add(new Point(b.X, a.Y));
            ControlPoints.Add(new Point(b.X, b.Y));
            ControlPoints.Add(new Point(a.X, b.Y));
        }

        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();

            for (int i = 0; i < ControlPoints.Count - 1; i++)
                points.AddRange(Bresenham(ControlPoints[i], ControlPoints[i + 1]));
            points.AddRange(Bresenham(ControlPoints[ControlPoints.Count - 1], ControlPoints[0]));

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            return isInside(ControlPoints.ToArray(), point);
        }

        public override string Type()
        {
            return "Rectangle";
        }
    }


    public class Pentagon : Shape
    {
        public Pentagon() { }

        public Pentagon(Point a, Point b)
        {
            double d_x = Math.Min(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)),
                canh = d_x / 2 / Math.Sin(Math.PI * 54.0 / 180.0),
                d1 = canh * Math.Cos(Math.PI * 54.0 / 180.0),
                d2 = canh * Math.Cos(Math.PI * 18.0 / 180.0),
                d3 = canh * Math.Sin(Math.PI * 18.0 / 180.0);
            int d = (int)(d1 + d2);

            Point[] vertex = new Point[5];

            vertex[0].X = a.X > b.X ? a.X - d3 : a.X + d3;
            vertex[0].Y = a.Y;
            vertex[1].X = a.X > b.X ? a.X - d_x + d3 : a.X + d_x - d3;
            vertex[1].Y = a.Y;
            vertex[2].X = a.X > b.X ? a.X - d_x : a.X + d_x;
            vertex[2].Y = a.Y > b.Y ? a.Y - d2 : a.Y + d2;
            vertex[3].X = a.X > b.X ? a.X - d_x / 2 : a.X + d_x / 2;
            vertex[3].Y = a.Y > b.Y ? (a.Y - d) : (a.Y + d);
            vertex[4].X = a.X;
            vertex[4].Y = a.Y > b.Y ? a.Y - d2 : a.Y + d2;

            ControlPoints.AddRange(vertex);
        }


        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();

            for (int i = 0; i < ControlPoints.Count - 1; i++)
                points.AddRange(Bresenham(ControlPoints[i], ControlPoints[i + 1]));
            points.AddRange(Bresenham(ControlPoints[ControlPoints.Count - 1], ControlPoints[0]));

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            return isInside(ControlPoints.ToArray(), point);
        }

        public override string Type()
        {
            return "Pentagon";
        }
    }


    public class Hexagon : Shape
    {
        public Hexagon() { }

        public Hexagon(Point a, Point b)
        {
            double canh = Math.Min(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
            double d1 = canh / 2 / Math.Sqrt(3);
            int d = 4 * (int)(d1);

            Point[] vertex = new Point[6];

            vertex[0].X = a.X > b.X ? a.X - canh / 2 : a.X + canh / 2;
            vertex[0].Y = a.Y;
            vertex[1].X = a.X;
            vertex[1].Y = a.Y > b.Y ? a.Y - d1 : a.Y + d1;
            vertex[2].X = a.X;
            vertex[2].Y = a.Y > b.Y ? a.Y - 3 * d1 : a.Y + 3 * d1;
            vertex[3].X = (a.X > b.X ? a.X - canh / 2 : a.X + canh / 2);
            vertex[3].Y = a.Y > b.Y ? a.Y - d : a.Y + d;
            vertex[4].X = a.X > b.X ? a.X - canh : a.X + canh;
            vertex[4].Y = a.Y > b.Y ? a.Y - 3 * d1 : a.Y + 3 * d1;
            vertex[5].X = a.X > b.X ? a.X - canh : a.X + canh;
            vertex[5].Y = a.Y > b.Y ? a.Y - d1 : a.Y + d1;

            ControlPoints.AddRange(vertex);
        }

        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();

            for (int i = 0; i < ControlPoints.Count - 1; i++)
                points.AddRange(Bresenham(ControlPoints[i], ControlPoints[i + 1]));
            points.AddRange(Bresenham(ControlPoints[ControlPoints.Count - 1], ControlPoints[0]));

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            return isInside(ControlPoints.ToArray(), point);
        }

        public override string Type()
        {
            return "Hexagon";
        }
    }


    public class Polygon : Shape
    {
        public override void Draw(ref OpenGL gl)
        {
            if (!Visible)
                return;

            List<Point> points = new List<Point>();

            if (ControlPoints.Count <= 1)
                return;

            for (int i = 0; i < ControlPoints.Count - 1; i++)
                points.AddRange(Bresenham(ControlPoints[i], ControlPoints[i + 1]));
            points.AddRange(Bresenham(ControlPoints[ControlPoints.Count - 1], ControlPoints[0]));

            Color strokeColor = StrokeColor.Value;
            gl.Color(strokeColor.R, strokeColor.G, strokeColor.B);

            gl.Begin(OpenGL.GL_POINTS);
            gl.PointSize(1.0f);

            foreach (Point point in points)
                gl.Vertex(point.X, point.Y);
            gl.End();

            if (FillColor.HasValue)
                ScanFill(points, FillColor.Value, ref gl);
        }

        public override bool Contains(Point point)
        {
            return isInside(ControlPoints.ToArray(), point);
        }

        public override string Type()
        {
            return "Polygon";
        }

    }
}
