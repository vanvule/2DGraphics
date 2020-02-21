using SharpGL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static List<string> ToolTags = new List<string>
            { "Translate", "Rotate", "Scale" };

        private static List<string> ShapeTags = new List<string>
            { "Line", "Circle", "Ellipse", "Triangle", "Rectangle", "Pentagon", "Hexagon" };

        private enum Mode
        {
            DRAW,
            DRAW_BY_CLICK,
            TOOL
        };


        private OpenGL gl;

        //  State of the app
        private Mode mode;
        private string tag;
        bool changed;
        private int selectedShape;


        //  Shape to draw
        private List<Shape> shapes;
        private Shape preview;


        //  Postions
        private Point pStart, pEnd, pCurrent;

        public Point CurrentPosition
        {
            get { return this.pCurrent; }
            set
            {
                this.pCurrent = value;
                this.NotifyPropertyChanged("CurrentPosition");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }



        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            mode = Mode.DRAW;
            tag = "Line";
            changed = false;
            selectedShape = -1;

            shapes = new List<Shape>();
            preview = null;

            StrokeColor.SelectedColor = Colors.Black;
            FillColor.SelectedColor = null;
        }


        private void openGLControl_OpenGLInitialized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            this.gl = openGLControl.OpenGL;
            gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
        }


        private void openGLControl_OpenGLDraw(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Color(0.0f, 0.0f, 0.0f);

            //foreach (Shape shape in shapes)
            //    shape?.Draw(ref gl);

            for (int i = 0; i < shapes.Count - 1; i++)
                shapes[i].Draw(ref gl);

            if (shapes.Count > 0)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                shapes[shapes.Count - 1].Draw(ref gl);
                stopwatch.Stop();
                DrawingTime.Text = $"{stopwatch.Elapsed.TotalMilliseconds} ms";
            }

            preview?.Draw(ref gl);

            //  Draw control points
            if (selectedShape != -1)
            {
                List<Point> controlPoints;
                Point center;

                if (preview != null)
                {
                    controlPoints = preview.ControlPoints;
                    center = preview.Center();
                }
                else
                {
                    controlPoints = shapes[selectedShape].ControlPoints;
                    center = shapes[selectedShape].Center();
                }

                gl.Color(1.0, 0, 1.0, 0);
                gl.PointSize(7.0f);
                gl.Begin(OpenGL.GL_POINTS);

                gl.Vertex(center.X, center.Y);

                foreach (Point point in controlPoints)
                    gl.Vertex(point.X, point.Y);

                gl.End();
            }

            gl.PointSize(1.0f);

            gl.Flush();
        }


        private void openGLControl_Resized(object sender, SharpGL.SceneGraph.OpenGLEventArgs args)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Viewport(0, 0, (int)openGLControl.Width, (int)openGLControl.Height);
            gl.Ortho2D(0, openGLControl.Width, 0, openGLControl.Height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }


        private void openGLControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                pStart = e.GetPosition(openGLControl);
                pStart.Y = openGLControl.ActualHeight + 1 - pStart.Y;
                pEnd = pStart;

                if (mode == Mode.TOOL)
                {
                    selectedShape = -1;

                    if (shapes.Count > 0)
                    {
                        for (int i = shapes.Count - 1; i >= 0; i--)
                        {
                            if (shapes[i].Contains(pCurrent))
                            {
                                selectedShape = i;
                                Debug.WriteLine($"{selectedShape}");
                                break;
                            }
                        }
                    }
                }
                else if (mode == Mode.DRAW_BY_CLICK)
                {
                    preview.ControlPoints.Add(CurrentPosition);
                }
            }
            else if (e.ChangedButton == System.Windows.Input.MouseButton.Right
                && mode == Mode.DRAW_BY_CLICK)
            {
                shapes.Add(preview);
                preview = new Polygon()
                {
                    StrokeColor = StrokeColor.SelectedColor
                };
            }

        }


        private void openGLControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CurrentPosition = e.GetPosition(openGLControl);
            pCurrent.Y = openGLControl.ActualHeight + 1 - pCurrent.Y;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                pEnd = pCurrent;

                if (mode == Mode.DRAW)
                {
                    preview = ShapeFactory.CrateShape(
                    tag, pStart, pCurrent,
                    StrokeColor.SelectedColor,
                    FillColor.SelectedColor);
                }
                else if (mode == Mode.TOOL)
                {
                    if (selectedShape == -1)
                        return;

                    changed = true;
                    shapes[selectedShape].Visible = false;

                    preview = Transformer.Transform(
                        tag,
                        shapes[selectedShape],
                        pStart,
                        pCurrent
                    );
                }
            }
        }


        private void openGLControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            pEnd = e.GetPosition(openGLControl);
            pEnd.Y = openGLControl.ActualHeight + 1 - pEnd.Y;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (mode == Mode.DRAW)
                {
                    shapes.Add(ShapeFactory.CrateShape(
                    tag, pStart, pEnd,
                    StrokeColor.SelectedColor,
                    FillColor.SelectedColor)
                    );

                    preview = null;
                }
                else if (mode == Mode.TOOL)
                {
                    if (selectedShape == -1)
                        return;

                    if (changed)
                        shapes[selectedShape] = preview;
                    else
                        shapes[selectedShape].Visible = true;

                    changed = false;

                    preview = null;
                }
            }

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            tag = button.Tag as string;

            if (ShapeTags.Contains(tag))
            {
                mode = Mode.DRAW;
                selectedShape = -1;
                preview = null;
            }
            else if (tag == "Polygon")
            {
                mode = Mode.DRAW_BY_CLICK;
                selectedShape = -1;
                preview = new Polygon()
                {
                    StrokeColor = StrokeColor.SelectedColor
                };
            }
            else if (ToolTags.Contains(tag))
            {
                mode = Mode.TOOL;
                selectedShape = -1;
                preview = null;
            }

            Debug.WriteLine($"{mode} {tag}");
        }


        private void StrokeColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (selectedShape != -1)
            {
                ColorPicker colorPicker = sender as ColorPicker;
                shapes[selectedShape].StrokeColor = colorPicker.SelectedColor;
            }
        }


        private void FillColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (selectedShape != -1)
            {
                ColorPicker colorPicker = sender as ColorPicker;
                shapes[selectedShape].FillColor = colorPicker.SelectedColor;
            }
        }

    }
}
