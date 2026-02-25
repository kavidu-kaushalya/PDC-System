using System.Windows;
using System.Windows.Input;

namespace PDC_System
{
    public partial class MiniWidgetWindow : Window
    {
        private Point _dragStartPoint;
        private bool _isDragging = false;

        public MiniWidgetWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the starting point for drag detection
            _dragStartPoint = e.GetPosition(this);
            _isDragging = false;
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If the mouse hasn't moved much, treat it as a click
            Point currentPoint = e.GetPosition(this);
            double distance = (currentPoint - _dragStartPoint).Length;

            // If distance is small (less than 5 pixels), it's a click, not a drag
            if (distance < 5 && !_isDragging)
            {
                OpenAddJobCardWindow();
            }

            _isDragging = false;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            // Only drag if left mouse button is pressed
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                double distance = (currentPoint - _dragStartPoint).Length;

                // If moved more than 5 pixels, start dragging
                if (distance > 5)
                {
                    _isDragging = true;
                    this.DragMove();
                }
            }
        }

        private void OpenAddJobCardWindow()
        {
            try
            {
                // Load customers from your data source
                var customers = LoadCustomers();

                // Create and show the Add Job Card window
                var addJobCardWindow = new MiniJobCardWindow(customers);
                addJobCardWindow.Owner = this; // Set the mini widget as owner
                bool? result = addJobCardWindow.ShowDialog();

                // After closing, the widget remains visible and active
                this.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Job Card window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Customerinfo> LoadCustomers()
        {
            // Load your customers from JSON or database
            // This is a placeholder - replace with your actual data loading logic
            string customersFile = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(), "Savers", "customers.json");

            if (System.IO.File.Exists(customersFile))
            {
                string json = System.IO.File.ReadAllText(customersFile);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customerinfo>>(json)
                    ?? new List<Customerinfo>();
            }

            return new List<Customerinfo>();
        }
    }
}