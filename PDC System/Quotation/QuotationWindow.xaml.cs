using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace PDC_System
{
    public partial class QuotationWindow : System.Windows.Controls.UserControl
    {
        private List<Customer> customers = new List<Customer>();
        private List<Save_pdf> savedPdfs = new List<Save_pdf>();
        public ObservableCollection<Save_pdf> Files { get; set; }

        public QuotationWindow()
        {
            InitializeComponent();
            LoadData();

            // Sort the savedPdfs by Date in descending order
            savedPdfs = savedPdfs.OrderByDescending(pdf => pdf.Date).ToList();

            // Initialize ObservableCollection directly with savedPdfs
            Files = new ObservableCollection<Save_pdf>(savedPdfs);

            // Set Files as the DataGrid's ItemSource
            PDFSaveDataGrid.ItemsSource = Files;
        }

        private void LoadData()
        {
            if (File.Exists("customers.json"))
            {
                customers = JsonConvert.DeserializeObject<List<Customer>>(File.ReadAllText("customers.json"));
                CustomerDataGrid.ItemsSource = customers;
            }

            if (File.Exists("savedpdfs.json"))
            {
                savedPdfs = JsonConvert.DeserializeObject<List<Save_pdf>>(File.ReadAllText("savedpdfs.json"));
            }
        }

        private void CustomerSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (customers == null) return;

            string searchText = CustomerSearchNameTextBox.Text.ToLower();

            var filteredCustomers = customers.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(searchText)) ||
                (c.Address != null && c.Address.ToLower().Contains(searchText))
            ).ToList();

            CustomerDataGrid.ItemsSource = filteredCustomers;
        }


        private void CreateQuotation_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomerDataGrid.SelectedItem as Customer;
            if (selectedCustomer != null)
            {
                var quotationCreateWindow = new QuotationCreateWindow(selectedCustomer);
                if (quotationCreateWindow.ShowDialog() == true)
                {
                    // Create a new Save_pdf object with the quotation details
                    var newPdf = new Save_pdf
                    {
                        CustomerName = selectedCustomer.Name,
                        Date = DateTime.Now,
                        FilePath = quotationCreateWindow.FilePath,
                        gg = selectedCustomer.Address,
                        Gtotal = quotationCreateWindow.gtotal,
                        QuoteNo = quotationCreateWindow.QuoteNumber,
                        qname = quotationCreateWindow.Qname
                    };

                    // Add the new PDF to the list and update savedpdfs.json
                    savedPdfs.Add(newPdf);
                    File.WriteAllText("savedpdfs.json", JsonConvert.SerializeObject(savedPdfs));

                    // Sort the savedPdfs list in descending order by Date
                    savedPdfs = savedPdfs.OrderByDescending(pdf => pdf.Date).ToList();

                    // Clear and add sorted items back to the ObservableCollection
                    Files.Clear();
                    foreach (var pdf in savedPdfs)
                    {
                        Files.Add(pdf);
                    }

                    // Refresh the DataGrid
                    PDFSaveDataGrid.Items.Refresh();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a customer.");
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is Save_pdf selectedItem)
            {
                OpenFile(selectedItem.FilePath);
            }
        }

        private void DeleteSavedpdfs_Click(object sender, RoutedEventArgs e)
        {
            var selectedSave_pdf = PDFSaveDataGrid.SelectedItem as Save_pdf;
            if (selectedSave_pdf != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogQuotation(); // Assuming you've created this dialog class
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    Files.Remove(selectedSave_pdf);
                    PDFSaveDataGrid.Items.Refresh();
                    File.WriteAllText("savedpdfs.json", JsonConvert.SerializeObject(Files));
                }
            }
        }

        private void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else
            {
                System.Windows.MessageBox.Show("File not found!");
            }
        }

        // Save_pdf class definition
        public class Save_pdf
        {
            public DateTime Date { get; set; }
            public string CustomerName { get; set; }
            public decimal Gtotal { get; set; }
            public string FilePath { get; set; }
            public string gg { get; set; }
            public string QuoteNo { get; set; }
            public string qname { get; set; }
        }

        // Search for customer name or date range
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterFiles();
        }

        private void FilterFiles()
        {
            if (savedPdfs == null) return;

            string searchText = CustomerSearchTextBox.Text.Trim().ToLower();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            var filteredFiles = savedPdfs.AsQueryable();

            // Filter by search text (Customer Name or Quote No)
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredFiles = filteredFiles.Where(pdf =>
                    (pdf.CustomerName != null && pdf.CustomerName.ToLower().Contains(searchText)) ||
                    (pdf.QuoteNo != null && pdf.QuoteNo.ToLower().Contains(searchText))
                );
            }

            // Ensure correct date filtering
            if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date; // Start of the day (00:00:00)
                filteredFiles = filteredFiles.Where(pdf => pdf.Date >= start);
            }

            if (endDate.HasValue)
            {
                DateTime end = endDate.Value.Date.AddDays(1).AddTicks(-1); // End of the day (23:59:59)
                filteredFiles = filteredFiles.Where(pdf => pdf.Date <= end);
            }

            filteredFiles = filteredFiles.OrderByDescending(pdf => pdf.Date);

            // Update ObservableCollection instead of directly modifying ItemsSource
            Files.Clear();
            foreach (var file in filteredFiles)
            {
                Files.Add(file);
            }
        }


        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {

            // Apply filter based on search text
            FilterFiles();
        }

        private void SearchBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            // Clear first search box and date filters


            // Apply filter based on `qname`
            ApplyFilter();
        }


        private void ApplyFilter()
        {
            string query = CustomerSearchTextBox2.Text.Trim().ToLower();

            var filteredFiles = savedPdfs
                .Where(pdf => pdf.qname != null && pdf.qname.ToLower().Contains(query))
                .OrderByDescending(pdf => pdf.Date)
                .ToList();

            Files.Clear();
            foreach (var file in filteredFiles)
            {
                Files.Add(file);
            }
        }




        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            // Trigger the calendar to show when the button is clicked
            if (EndDatePicker.IsDropDownOpen)
            {
                EndDatePicker.IsDropDownOpen = false; // Close if already open
            }
            else
            {
                EndDatePicker.IsDropDownOpen = true; // Open calendar
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search fields
            CustomerSearchTextBox.Text = string.Empty;
            CustomerSearchTextBox2.Text = string.Empty;
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            // Reload the original file list
            if (savedPdfs != null)
            {
                Files.Clear();
                foreach (var file in savedPdfs.OrderByDescending(pdf => pdf.Date))
                {
                    Files.Add(file);
                }
            }
        }


    }
}
