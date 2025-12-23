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
    public partial class Jobs : UserControl
    {
        private List<JobCard> jobCards = new List<JobCard>();
        private List<Customerinfo> customers = new List<Customerinfo>();

        private readonly string saversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
        private readonly string jobCardsFile;
        private readonly string customersFile;

        public Jobs()
        {
            InitializeComponent();

            // Ensure Savers folder exists
            if (!Directory.Exists(saversFolder))
                Directory.CreateDirectory(saversFolder);

            jobCardsFile = Path.Combine(saversFolder, "jobcards.json");
            customersFile = Path.Combine(saversFolder, "customers.json");

            LoadData();
            JobCardDataGrid.Items.Refresh();
        }

        private void LoadData()
        {
            if (File.Exists(jobCardsFile))
            {
                jobCards = JsonConvert.DeserializeObject<List<JobCard>>(File.ReadAllText(jobCardsFile));

                // Sort by date descending (latest first)
                jobCards = jobCards.OrderByDescending(j => j.JobCardDate).ToList();

                JobCardDataGrid.ItemsSource = jobCards;
            }

            if (File.Exists(customersFile))
            {
                customers = JsonConvert.DeserializeObject<List<Customerinfo>>(File.ReadAllText(customersFile));
            }
        }

        private void AddJobCard_Click(object sender, RoutedEventArgs e)
        {
            var addJobCardWindow = new AddJobCardWindow(customers);
            if (addJobCardWindow.ShowDialog() == true)
            {
                jobCards.Add(addJobCardWindow.JobCard);
                jobCards.Reverse();
                JobCardDataGrid.Items.Refresh();
                File.WriteAllText(jobCardsFile, JsonConvert.SerializeObject(jobCards));
            }
        }

        private void DeleteJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJob = JobCardDataGrid.SelectedItem as JobCard;
            if (selectedJob != null)
            {
                var confirmationDialog = new ConfirmationDialogJobs();
                confirmationDialog.Owner = Application.Current.MainWindow;
                confirmationDialog.ShowDialog();

                if (confirmationDialog.IsConfirmed)
                {
                    jobCards.Remove(selectedJob);
                    JobCardDataGrid.Items.Refresh();
                    File.WriteAllText(jobCardsFile, JsonConvert.SerializeObject(jobCards));
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                CustomMessageBox.Show("Please select a date range.");
                return;
            }

            ApplyFilter();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            NameAutoCompleteBox1.Text = "";
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            JobCardDataGrid.ItemsSource = null;
            JobCardDataGrid.ItemsSource = jobCards;
        }

        private void OpenJobCardButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = JobCardDataGrid.SelectedItem as JobCard;
            if (selectedRow != null)
            {
                JobCardView detailsWindow = new JobCardView(selectedRow);
                detailsWindow.Show();
            }
            else
            {
                CustomMessageBox.Show("Please select a job card.");
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
                foreach (var job in jobCards)
                {
                    Console.WriteLine($"Job: {job.Customer_Name}, Seen: {job.IsSeen}");
                }

                File.WriteAllText(jobCardsFile, JsonConvert.SerializeObject(jobCards));

                JobCardDataGrid.ItemsSource = null;
                JobCardDataGrid.ItemsSource = jobCards;
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving job cards: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (NameAutoCompleteBox1.Text == null) return;

            string query = NameAutoCompleteBox1.Text.Trim().ToLower();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate?.AddDays(1).AddTicks(-1);

            var filteredFiles = jobCards.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                filteredFiles = filteredFiles
                    .Where(jc => !string.IsNullOrEmpty(jc.Customer_Name) &&
                                 jc.Customer_Name.ToLower().Contains(query));
            }

            if (startDate != null && endDate != null)
            {
                filteredFiles = filteredFiles
                    .Where(jc => jc.JobCardDate >= startDate && jc.JobCardDate <= endDate);
            }

            var finalList = filteredFiles.OrderByDescending(jc => jc.JobCardDate).ToList();

            JobCardDataGrid.ItemsSource = null;
            JobCardDataGrid.ItemsSource = finalList;
        }
    }
}
