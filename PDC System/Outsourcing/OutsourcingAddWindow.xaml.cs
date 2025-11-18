using System.Windows;

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
