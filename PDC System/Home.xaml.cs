using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            
            this.Loaded += Home_Loaded;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            string exeName = "PDC_System_Backups"; // The name of your executable (without the .exe extension)

            // Check if the process is already running
            bool isRunning = Process.GetProcessesByName(exeName).Any();
            MainContent.Content = new HomeUIWindow(); // Remove or comment this line if you don't want it

            if (!isRunning)
            {
                // If it's not running, start the second project
                try
                {
                    Process.Start(@"PDC_System_Backups.exe");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching second project: {ex.Message}");
                }
            }
        }

        private void OpenView1_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Customers(); // Load UserControl1
        }

        private void OpenView2_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Jobs(); // Load UserControl1
        }

        private void OpenView3_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new QuotationWindow(); // Load UserControl1
        }

        private void OpenView4_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new EmployeeWindow(); // Load UserControl1
        }

        private void OpenView5_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AttendanceWindow(); // Load UserControl1
        }

        private void OpenView6_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SalaryDetails(); // Load UserControl1
        }

        private void OpenView8_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HomeUIWindow(); // Load UserControl1
        }

        private void OpenView9_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new Oders(); // Load UserControl1
        }


    }
}
