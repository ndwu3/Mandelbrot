using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CS480_HW4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Initialize the constants and variables
        const int WIDTH = 800;
        const int HEIGHT = 800;
        //mouse cursor location
        private Point point;
        //selection box for the zoom area
        private Rectangle selection = new Rectangle()
        {
            Stroke = Brushes.Cyan,
            StrokeThickness = 1,
            Visibility = Visibility.Collapsed
        };
        private bool mouseDown = false;

        //area for the mandelbrot set
        private Rect area = new Rect(new Point(0.0, 0.0), new Point(WIDTH, HEIGHT));


        public MainWindow()
        {
            InitializeComponent();
            CreateImage();
        }

        private void CreateImage()
        {
            try
            {
                myCanvas.Children.Remove(selection);
            }
            catch (Exception e)
            {
                //skip
            }

            //variables to use in the mandelbrot set
            double zR, zI;
            double nR, nI;
            double oR, oI;
            int iterations = 200;

            //arrays, variables, and pixels intialized for the bitmap
            byte[] pix1d = new byte[HEIGHT * WIDTH * 4];
            int index = 0;
            Int32Rect rect = new Int32Rect(0, 0, WIDTH, HEIGHT);
            int stride = 4 * WIDTH;
            Image image = new Image();
            image.Stretch = Stretch.None;
            image.Margin = new Thickness(0);

            WriteableBitmap myBitmap = new WriteableBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Bgra32, null);
            byte[,,] pixels = new byte[HEIGHT, WIDTH, 4];

            double xscale = (area.Right - area.Left) / WIDTH;
            double yscale = (area.Top - area.Bottom) / HEIGHT;

            //Clear the bitmap to black
            for (int r = 0; r < HEIGHT; r++)
            {
                for (int c = 0; c < WIDTH; c++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        pixels[r, c, i] = 0;
                    }
                    pixels[r, c, 3] = 255;
                }
            }

            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    //setting the initial pixel location.
                    zR = (area.Left + x * xscale - WIDTH / 2.0) * 4.0  / WIDTH;
                    zI = (area.Top - y * yscale - HEIGHT / 2.0) * 4.0 / HEIGHT;
                    nR = 0;
                    nI = 0;
                    for (int i = 0; i < iterations; i++)
                    {
                        //value of previous iteration
                        oR = nR;
                        oI = nI;
                        //calculations for the mandelbrot set
                        nR = oR * oR - oI * oI + zR;
                        nI = 2 * oR * oI + zI;
                        if ((nR * nR + nI * nI) > 4)
                        {
                            //change value of green as it iterates
                            pixels[y, x, 0] = 89;
                            pixels[y, x, 1] = (byte)((i * 1.2) + 80);
                            pixels[y, x, 2] = 13;
                            break;
                        }
                        else
                        {
                            //color inside a light black
                            pixels[y, x, 0] = 20;
                            pixels[y, x, 1] = 20;
                            pixels[y, x, 2] = 20;
                        }
                    }
                }
            }

            index = 0;
            //copy the data into a 1d array
            for (int r = 0; r < HEIGHT; r++)
            {
                for (int c = 0; c < WIDTH; c++)
                {
                    for (int bgra = 0; bgra < 4; bgra++)
                    {
                        pix1d[index++] = pixels[r, c, bgra];
                    }
                }
            }
            myBitmap.WritePixels(rect, pix1d, stride, 0);
            image.Source = myBitmap;
            myCanvas.Children.Add(image);
            try
            {
                myCanvas.Children.Add(selection);
            }
            catch(Exception e)
            {
                //skip
            }
        }

        private void OnLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //starts the selection box
            point = Mouse.GetPosition(Application.Current.MainWindow);
            mouseDown = true;
            Canvas.SetLeft(selection, point.X);
            Canvas.SetTop(selection, point.Y);
            selection.Width = 0;
            selection.Height = 0;
            selection.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point mousePos = e.GetPosition(myCanvas);
                Vector difference = mousePos - point;
                Point topLeft = point;
                //checks for how user drags the selection box
                if (difference.X < 0)
                {
                    topLeft.X = mousePos.X;
                    difference.X = -difference.X;
                }
                if (difference.Y < 0)
                {
                    topLeft.Y = mousePos.Y;
                    difference.Y = -difference.Y;
                }
                selection.Width = difference.X;
                selection.Height = difference.Y;
                Canvas.SetLeft(selection, topLeft.X);
                Canvas.SetTop(selection, topLeft.Y);
            }
        }

        private void OnLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            selection.Visibility = Visibility.Collapsed;
            double xscale = (area.Right - area.Left) / WIDTH;
            double yscale = (area.Top - area.Bottom) / HEIGHT;
            Point TopLeft = new Point(area.Left + Canvas.GetLeft(selection) * xscale, area.Top - Canvas.GetTop(selection) * yscale);
            Point BottomRight = TopLeft + new Vector(selection.Width * xscale, -selection.Height * yscale);
            area = new Rect(TopLeft, BottomRight);
            CreateImage();
        }

        private void OnRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            area = new Rect(new Point(0.0, 0.0), new Point(WIDTH, HEIGHT));
            CreateImage();
        }
    }
}
