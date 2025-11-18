using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;

namespace PDC_System
{
    public partial class ScreenCaptureWindow : Window
    {
        private System.Windows.Point startPoint;
        public BitmapSource? CapturedImage { get; private set; }

        public ScreenCaptureWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += ScreenCaptureWindow_MouseLeftButtonDown;
            MouseMove += ScreenCaptureWindow_MouseMove;
            MouseLeftButtonUp += ScreenCaptureWindow_MouseLeftButtonUp;
        }

        private void ScreenCaptureWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            SelectionRectangle.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionRectangle, startPoint.X);
            Canvas.SetTop(SelectionRectangle, startPoint.Y);
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;
        }

        private void ScreenCaptureWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point pos = e.GetPosition(this);
                double x = Math.Min(pos.X, startPoint.X);
                double y = Math.Min(pos.Y, startPoint.Y);
                double w = Math.Abs(pos.X - startPoint.X);
                double h = Math.Abs(pos.Y - startPoint.Y);
                Canvas.SetLeft(SelectionRectangle, x);
                Canvas.SetTop(SelectionRectangle, y);
                SelectionRectangle.Width = w;
                SelectionRectangle.Height = h;
            }
        }

        private void ScreenCaptureWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var topLeft = PointToScreen(new System.Windows.Point(Canvas.GetLeft(SelectionRectangle), Canvas.GetTop(SelectionRectangle)));
            int width = (int)SelectionRectangle.Width;
            int height = (int)SelectionRectangle.Height;

            if (width <= 0 || height <= 0)
            {
                DialogResult = false;
                return;
            }

            using (Drawing.Bitmap bmp = new Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Drawing.Graphics g = Drawing.Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen((int)topLeft.X, (int)topLeft.Y, 0, 0, new Drawing.Size(width, height), Drawing.CopyPixelOperation.SourceCopy);
                }

                CapturedImage = Imaging.CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }

            DialogResult = true;
        }
    }
}
