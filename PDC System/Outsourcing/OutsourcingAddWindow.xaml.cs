using System.Windows;
using System.Windows.Input;

namespace PDC_System.Customer
{
    public partial class OutsourcingAddWindow : Window
    {
        public Outsourcinginfo Outsourcing { get; private set; }

        public OutsourcingAddWindow()
        {
            InitializeComponent();
            UpdatePanelVisibility(); // Ensure correct panel is visible on load
        }

        // Unified Checked handler for all radio buttons
        private void OutsourcingType_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePanelVisibility();
        }

        // Show/hide panels based on selected RadioButton
        private void UpdatePanelVisibility()
        {
            if (PlateStackPanel != null) PlateStackPanel.Visibility = Plate_Checked?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (DigitalStackPanel != null) DigitalStackPanel.Visibility = Digital_Checked?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (AnyStackPanel != null) AnyStackPanel.Visibility = Any_Checked?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }


        #region Window Control

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private bool _isMaximized = false;
        private double _previousLeft;
        private double _previousTop;
        private double _previousWidth;
        private double _previousHeight;

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (_isMaximized)
            {
                // Restore to previous size and position
                this.Left = _previousLeft;
                this.Top = _previousTop;
                this.Width = _previousWidth;
                this.Height = _previousHeight;
                _isMaximized = false;
            }
            else
            {
                // get before maximizing
                _previousLeft = this.Left;
                _previousTop = this.Top;
                _previousWidth = this.Width;
                _previousHeight = this.Height;

                // Get the working area (screen minus taskbar)
                var workingArea = SystemParameters.WorkArea;

                // Set window position and size to working area
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                _isMaximized = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        #endregion

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Outsourcing = new Outsourcinginfo
            {
                Type1 = Plate_Checked.IsChecked == true ? "Plate" :
                        Digital_Checked.IsChecked == true ? "Digital" :
                        Any_Checked.IsChecked == true ? "Any" : "",

                // You can also save other text box values here, e.g.:
                PlateName = PlateNameTextBox.Text,
                PlateEmail = PlateCompanyEmailTextBox.Text,
                PlateContact = PlateCompanyContactTextBox.Text,
                PlateCost = PlateCostTextBox.Text,
                DigitalName = DigitalCompanyNameTextBox.Text,
                DigitalEmail = DigitalCompanyEmailTextBox.Text,
                DigitalContact = DigitalCompanyContactTextBox.Text,
                AnyName = AnyCompanyNameTextBox.Text,
                AnyEmail = AnyCompanyEmailTextBox.Text,
                AnyContact = AnyCompanyContactTextBox.Text
            };
            

            DialogResult = true;
        }
    }

    // Example class, you can adjust fields as needed
   
}
