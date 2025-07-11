using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using PDC_System.Job_Card;
using static PDC_System.QuotationWindow;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Jobs.xaml
    /// </summary>
    public partial class Jobs : System.Windows.Controls.UserControl
    {
        private List<JobCard> jobCards = new List<JobCard>();
        private List<Customer> customers = new List<Customer>();

        public Jobs()
        {
            InitializeComponent();
            LoadData();
            JobCardDataGrid.Items.Refresh();
        }

        private void LoadData()
        {
            if (File.Exists("jobcards.json"))
            {
                jobCards = JsonConvert.DeserializeObject<List<JobCard>>(File.ReadAllText("jobcards.json"));

                // Sort by date descending (latest first)
                jobCards = jobCards.OrderByDescending(j => j.JobCardDate).ToList();

                JobCardDataGrid.ItemsSource = jobCards;
            }

            if (File.Exists("customers.json"))
            {
                customers = JsonConvert.DeserializeObject<List<Customer>>(File.ReadAllText("customers.json"));
            }
        }


        private void AddJobCard_Click(object sender, RoutedEventArgs e)
        {
            var addJobCardWindow = new AddJobCardWindow(customers);
            if (addJobCardWindow.ShowDialog() == true)
            {
                jobCards.Add(addJobCardWindow.JobCard);
                // Reverse the list to show the latest job card first
                jobCards.Reverse();
                JobCardDataGrid.Items.Refresh();
                File.WriteAllText("jobcards.json", JsonConvert.SerializeObject(jobCards));
            }
        }

        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJob = JobCardDataGrid.SelectedItem as JobCard;
            if (selectedJob != null)
            {
                // Create and show the custom dialog
                var confirmationDialog = new ConfirmationDialogJobs();
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as owner
                confirmationDialog.ShowDialog();

                // Check if the user clicked Yes
                if (confirmationDialog.IsConfirmed)
                {
                    jobCards.Remove(selectedJob);
                    JobCardDataGrid.Items.Refresh();
                    File.WriteAllText("jobcards.json", JsonConvert.SerializeObject(jobCards));
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date range.");
                return;
            }

            // Ensure full day inclusion
            DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            // Keep the existing customer name search filter
            ApplyFilter();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Clear customer name search box
            NameAutoCompleteBox1.Text = "";

            // Clear date selection
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            // Restore the original job card list
            JobCardDataGrid.ItemsSource = null; // Ensure UI refresh
            JobCardDataGrid.ItemsSource = jobCards;
        }


        // Define the Button Click event handler
        private void OpenJobCardButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = JobCardDataGrid.SelectedItem as JobCard;
            if (selectedRow != null)
            {
                JobCardView detailsWindow = new JobCardView(selectedRow); // Pass selected job card
                detailsWindow.Show(); // Show the window modally
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a job card.");
            }
        }

        private void CheckBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveJobCardsToJson();
        }

        private void SaveJobCardsToJson()
        {
            try
            {
                // Debug: Print the current state of jobCards
                foreach (var job in jobCards)
                {
                    Console.WriteLine($"Job: {job.Customer_Name}, Seen: {job.IsSeen}");
                }

                // Save the updated jobCards list to the JSON file
                File.WriteAllText("jobcards.json", JsonConvert.SerializeObject(jobCards));

                // Refresh the DataGrid
                JobCardDataGrid.ItemsSource = null;
                JobCardDataGrid.ItemsSource = jobCards;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving job cards: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            // Clear first search box and date filters


            // Apply filter based on `qname`
            ApplyFilter();
        }
        private void ApplyFilter()
        {
            if (NameAutoCompleteBox1.Text == null) return;

            string query = NameAutoCompleteBox1.Text.Trim().ToLower();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate?.AddDays(1).AddTicks(-1); // Include full day

            var filteredFiles = jobCards.AsEnumerable(); // Start with the full list

            // Apply customer name filter if a name is entered
            if (!string.IsNullOrEmpty(query))
            {
                filteredFiles = filteredFiles
                    .Where(jc => !string.IsNullOrEmpty(jc.Customer_Name) &&
                                 jc.Customer_Name.ToLower().Contains(query));
            }

            // Apply date range filter if both dates are selected
            if (startDate != null && endDate != null)
            {
                filteredFiles = filteredFiles
                    .Where(jc => jc.JobCardDate >= startDate && jc.JobCardDate <= endDate);
            }

            // Apply sorting
            var finalList = filteredFiles.OrderByDescending(jc => jc.JobCardDate).ToList();

            // Update UI without resetting search box
            JobCardDataGrid.ItemsSource = null; // Force UI refresh
            JobCardDataGrid.ItemsSource = finalList;
        }



    }
}
