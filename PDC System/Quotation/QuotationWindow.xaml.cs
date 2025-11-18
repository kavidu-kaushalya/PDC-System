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
    public partial class QuotationWindow : UserControl
    {
        public event Action InvoicesUpdated;
        public ObservableCollection<Quotation> SavedQuotations { get; set; } = new();
        public ObservableCollection<InvoiceRecord> SavedInvoices { get; set; } = new();

        public QuotationWindow()
        {
            InitializeComponent();
            UserControl_Loaded();
            LoadInvoices();

        }



        private void AddQuotation_Click(object sender, RoutedEventArgs e)
        {
            // Create AddQuotationWindow and subscribe to its event
            var window = new AddQuotationWindow();
            window.QuotationSaved += UserControl_Loaded;  // ✅ Refresh quotation list after saving
            window.ShowDialog(); // Modal window (use Show() if you prefer non-blocking)
        }

        public void UserControl_Loaded()
        {
            try
            {
                string file = Path.Combine(Directory.GetCurrentDirectory(), "Savers", "SavedQuotations.json");

                if (File.Exists(file))
                {
                    string json = File.ReadAllText(file);
                    var quotations = JsonConvert.DeserializeObject<List<Quotation>>(json);

                    if (quotations != null)
                    {
                        SavedQuotations = new ObservableCollection<Quotation>(quotations);
                        QuotationGrid.ItemsSource = SavedQuotations;
                    }
                }
                else
                {
                    // If file doesn't exist yet, clear grid
                    SavedQuotations.Clear();
                    QuotationGrid.ItemsSource = SavedQuotations;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quotations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void EditQuotation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Quotation selectedQuotation)
            {
                var window = new AddQuotationWindow(selectedQuotation);
                window.QuotationSaved += UserControl_Loaded; // refresh after save
                window.ShowDialog();
            }
        }
        private void DeleteQuotation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Quotation selectedQuotation)
            {
                var result = MessageBox.Show($"Are you sure you want to delete quotation {selectedQuotation.QuotationNumber}?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Remove from ObservableCollection
                    SavedQuotations.Remove(selectedQuotation);

                    // Remove from JSON file
                    string file = Path.Combine(Directory.GetCurrentDirectory(), "Savers", "SavedQuotations.json");
                    if (File.Exists(file))
                    {
                        string json = File.ReadAllText(file);
                        var quotations = JsonConvert.DeserializeObject<List<Quotation>>(json) ?? new List<Quotation>();

                        // Remove the selected quotation from the list
                        quotations.RemoveAll(q => q.QuotationNumber == selectedQuotation.QuotationNumber);

                        File.WriteAllText(file, JsonConvert.SerializeObject(quotations, Formatting.Indented));
                    }
                }
            }
        }


        private void Invoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Quotation selectedQuotation)
            {
                InvoiceWindow invoiceWindow = new InvoiceWindow(selectedQuotation);
                invoiceWindow.ShowDialog();
            }
        }




        #region Billing Invoice Print 

        private void OpenManualInvoice_Click(object sender, RoutedEventArgs e)
        {
            string customerFile = Path.Combine(Directory.GetCurrentDirectory(), "Savers", "customers.json");
            List<Customerinfo> customers = new List<Customerinfo>();

            if (File.Exists(customerFile))
            {
                string json = File.ReadAllText(customerFile);
                customers = JsonConvert.DeserializeObject<List<Customerinfo>>(json) ?? new List<Customerinfo>();
            }

            // Open the manual invoice window
            InvoiceWindow manualInvoiceWindow = new InvoiceWindow(customers);
            manualInvoiceWindow.ShowDialog();
        }




        private void LoadInvoices()
        {
            try
            {
                string file = Path.Combine(Directory.GetCurrentDirectory(), "Savers", "Invoices.json");

                if (File.Exists(file))
                {
                    string json = File.ReadAllText(file);
                    var invoices = JsonConvert.DeserializeObject<List<InvoiceRecord>>(json);

                    if (invoices != null)
                    {
                        SavedInvoices = new ObservableCollection<InvoiceRecord>(invoices);
                        InvoiceGrid.ItemsSource = SavedInvoices;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoices: " + ex.Message);
            }
        }

        private void EditInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceRecord record)
            {
                InvoiceWindow win = new InvoiceWindow(record);   // NEED constructor
                win.ShowDialog();
                LoadInvoices();
            }
        }

        private void DeleteInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceRecord record)
            {
                if (MessageBox.Show("Delete invoice?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SavedInvoices.Remove(record);

                    string file = Path.Combine(Directory.GetCurrentDirectory(), "Savers", "Invoices.json");

                    File.WriteAllText(file, JsonConvert.SerializeObject(SavedInvoices.ToList(), Formatting.Indented));
                }
            }
        }






        #endregion

    }
}
