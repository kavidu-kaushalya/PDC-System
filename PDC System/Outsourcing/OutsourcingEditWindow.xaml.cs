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
using System.Windows.Shapes;

namespace PDC_System.Outsourcing
{
    /// <summary>
    /// Interaction logic for OutsourcingEditWindow.xaml
    /// </summary>
    public partial class OutsourcingEditWindow : Window
    {
        private Outsourcinginfo editingRow;

        public OutsourcingEditWindow(Outsourcinginfo row)
        {
            InitializeComponent();
            editingRow = row;

            // Load data
            PlateNameTextBox.Text = editingRow.PlateName;
            PlateEmailTextBox.Text = editingRow.PlateEmail;
            PlateContactTextBox.Text = editingRow.PlateContact;

            DigitalNameTextBox.Text = editingRow.DigitalName;
            DigitalEmailTextBox.Text = editingRow.DigitalEmail;
            DigitalContactTextBox.Text = editingRow.DigitalContact;

            AnyNameTextBox.Text = editingRow.AnyName;
            AnyEmailTextBox.Text = editingRow.AnyEmail;
            AnyContactTextBox.Text = editingRow.AnyContact;

            TypeTextBox.Text = editingRow.Type1;

            // Initial visibility
            UpdatePanelVisibility();
        }

        private void TypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePanelVisibility();
        }

        private void UpdatePanelVisibility()
        {
            string type = TypeTextBox.Text?.Trim().ToLower() ?? "";

            // Reset all to collapsed
            PlatePanel.Visibility = Visibility.Collapsed;
            DigitalPanel.Visibility = Visibility.Collapsed;
            AnyPanel.Visibility = Visibility.Collapsed;

            if (type == "digital")
            {
                DigitalPanel.Visibility = Visibility.Visible;
            }
            else if (type == "plate")
            {
                PlatePanel.Visibility = Visibility.Visible;
            }
            else if (type == "any")
            {
                AnyPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // Optionally show all
                PlatePanel.Visibility = Visibility.Visible;
                DigitalPanel.Visibility = Visibility.Visible;
                AnyPanel.Visibility = Visibility.Visible;
            }
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





        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update fields
            editingRow.PlateName = PlateNameTextBox.Text;
            editingRow.PlateEmail = PlateEmailTextBox.Text;
            editingRow.PlateContact = PlateContactTextBox.Text;

            editingRow.DigitalName = DigitalNameTextBox.Text;
            editingRow.DigitalEmail = DigitalEmailTextBox.Text;
            editingRow.DigitalContact = DigitalContactTextBox.Text;

            editingRow.AnyName = AnyNameTextBox.Text;
            editingRow.AnyEmail = AnyEmailTextBox.Text;
            editingRow.AnyContact = AnyContactTextBox.Text;

            editingRow.Type1 = TypeTextBox.Text;

            this.DialogResult = true;
            this.Close();
        }
    }

}
