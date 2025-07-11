using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace PDC_System
{
    public partial class EDetailsWindow : System.Windows.Controls.UserControl
    {
        public EDetailsWindow(Employee data)
        {
            InitializeComponent();

            // Display the details in the TextBlock
            NameTextBlock.Text = $"{data.Name}";
            Attendanceid.Text = $"{data.ID}";
            Birthday.Text = $"{data.Birthday.Value.Year}/{data.Birthday.Value.Month}/{data.Birthday.Value.Day}";
            address1.Text = $"{data.Address1}";
            address2.Text = $"{data.Address2}";
            city.Text = $"{data.City}";
            province.Text = $"{data.Province}";
            nid.Text = $"{data.NID}";
            con1.Text = $"{data.Contactn1}";
            con2.Text = $"{data.Contactn2}";

            Jobrole.Text = $"{data.jobrole}";
            dep.Text = $"{data.Department}";

            Basicsalary.Text = $"{data.BSalary} LKR";
            salary.Text = $"{data.Salary} LKR";
            ot.Text = $"{data.OT} LKR";
            DOT.Text = $"{data.DOT} LKR";








            // Calculate and display age

            // Load the image from the SavedLocation or default if not found
            LoadEmployeeImage(data.SavedLocation);



            int age = 0;

            if (data.Birthday.HasValue)
            {
                age = DateTime.Now.Year - data.Birthday.Value.Year;
            }
            else
            {
                // Handle the case when Birthday is null, if needed
                // For example, you can set a default age or handle this case differently
                age = 0;  // or some default value
            }
            AgeTextBlock.Text = $"{age} years old";  // Display the age in the TextBlock


        }

        private void LoadEmployeeImage(string savedLocation)
        {
            // Get the directory of the currently running project (or executable)
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the project directory with the relative default image path
            string defaultImagePath = Path.Combine(projectDirectory, @"Assets\3626507.png");

            if (!string.IsNullOrEmpty(savedLocation) && File.Exists(savedLocation))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(savedLocation));
                    ImagePreview.Source = bitmap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image from {savedLocation}: {ex.Message}");
                    SetDefaultImage(defaultImagePath);
                }
            }
            else
            {
                Console.WriteLine("Invalid or missing employee image file.");
                SetDefaultImage(defaultImagePath);
            }
        }

        private void SetDefaultImage(string defaultImagePath)
        {
            try
            {
                // Make sure the default image exists and set it as the source
                if (File.Exists(defaultImagePath))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(defaultImagePath));
                    ImagePreview.Source = bitmap;
                }
                else
                {
                    Console.WriteLine("Default image is missing.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default image: {ex.Message}");
            }
        }

    }
}
