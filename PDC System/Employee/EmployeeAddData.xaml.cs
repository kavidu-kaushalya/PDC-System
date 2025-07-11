using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PDC_System
{
    public partial class EmployeeAddData : Window
    {
        public Employee Employee { get; private set; }
        public ImageInfo imageInfo { get; private set; }

        private string customFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImageImports");

        public EmployeeAddData()
        {
            InitializeComponent();
        }

        private void adress_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // You might want to add event logic here or remove this method if not needed.
        }

        private void ImportImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select an image
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string originalFilePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(originalFilePath).ToLower();

                // Ensure the custom folder exists
                Directory.CreateDirectory(customFolderPath);

                // Find the next available file name
                int fileIndex = 1;
                string newFilePath = Path.Combine(customFolderPath, $"{fileIndex}{fileExtension}");

                // Keep incrementing the index until we find an unused file name
                while (File.Exists(newFilePath))
                {
                    fileIndex++;
                    newFilePath = Path.Combine(customFolderPath, $"{fileIndex}{fileExtension}");
                }

                // Save the image to the custom location with the new name
                File.Copy(originalFilePath, newFilePath);

                // Create a new ImageInfo object
                imageInfo = new ImageInfo
                {
                    OriginalPath = originalFilePath,
                    SavedLocation = newFilePath
                };

                // Display the imported image
                ImageDisplay.Source = new BitmapImage(new Uri(newFilePath));
            }
        }



        public class ImageInfo
        {
            public string OriginalPath { get; set; }
            public string SavedLocation { get; set; }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate Name
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Employee Name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate ID
            if (string.IsNullOrWhiteSpace(EmployeeIDTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Employee ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeSalaryTextBox.Text.Trim(), out decimal employeeS) || employeeS < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeBasicSalaryTextBox.Text.Trim(), out decimal employeeBS) || employeeBS < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Basic Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Validate OT
            if (!decimal.TryParse(EmployeeOTTextBox.Text, out decimal employeeOT) || employeeOT < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate DOT
            if (!decimal.TryParse(EmployeeDOTTextBox.Text, out decimal employeeDOT) || employeeDOT < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Double Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Absent Days
            if (!decimal.TryParse(EmployeeAbsentTextBox.Text, out decimal employeeAbsent) || employeeAbsent < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Absent.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create Employee Object
            Employee = new Employee
            {
                Name = EmployeeNameTextBox.Text.Trim(),
                ID = EmployeeIDTextBox.Text.Trim(),
                jobrole = jrole.Text.Trim(),
                Address1 = Address1.Text.Trim(),
                Address2 = Address2.Text.Trim(),
                City = city.Text.Trim(),
                Province = province.Text.Trim(),
                Contactn1 = contact1.Text.Trim(),
                Contactn2 = contact2.Text.Trim(),
                NID = NatId.Text.Trim(),
                Birthday = birthday.SelectedDate,
                Department = Department.Text.Trim(),

                BSalary = employeeBS,
                Salary = employeeS,
                OT = employeeOT,
                DOT = employeeDOT,
                ABSENT = employeeAbsent,
            };

            // Check if imageInfo is null before using it
            if (imageInfo != null)
            {
                Employee.OriginalPath = imageInfo.OriginalPath;
                Employee.SavedLocation = imageInfo.SavedLocation;
            }
            else
            {
                // If no image is provided, leave these properties blank or set them to default values
                Employee.OriginalPath = string.Empty; // Or you can set it to null
                Employee.SavedLocation = string.Empty; // Or you can set it to null
            }

            // Update UI
            EmployeeSalaryTextBox.Text = employeeS.ToString("F2");

            DialogResult = true;
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Check if the input is a number using a regular expression
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input (0-9) and limit to 10 characters
            if (!char.IsDigit(e.Text, 0) || ((sender as TextBox).Text.Length >= 10))
            {
                e.Handled = true;  // Prevent the input
            }
        }

        private bool vEntered = false;
        private void NatId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow "V" only once
            if (e.Text == "V" && !vEntered)
            {
                vEntered = true;
            }
            // Block non-numeric characters and ensure the length doesn't exceed 10 characters
            else if ((!char.IsDigit(e.Text, 0) && e.Text != "V") || (NatId.Text.Length >= 12))
            {
                e.Handled = true;
            }
        }

        private void PreviewfloteTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox?.Text ?? string.Empty;

            // Allow only numeric input (0-9) or a single decimal point
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;  // Prevent the input
            }
            else
            {
                // Allow only one decimal point
                if (e.Text == "." && currentText.Contains("."))
                {
                    e.Handled = true;  // Prevent input if there's already a decimal point
                }

                // Limit the length to 10 characters including the decimal point
                if (currentText.Length >= 10)
                {
                    e.Handled = true;  // Prevent input if length exceeds 10
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;

                // If the text is not empty and is a valid number
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, out decimal result))
                {
                    // Format the number to 2 decimal places (e.g., 500 to 500.00)
                    textBox.Text = result.ToString("F2");
                }
            }
        }
    }
}
