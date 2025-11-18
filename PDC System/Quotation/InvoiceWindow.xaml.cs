using Microsoft.Win32;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
namespace PDC_System

{
    public partial class InvoiceWindow : Window
    {






        private string dictionaryPath = "Dictionary/dictionary_invoice.txt";
        private List<string> dictionary = new List<string>();



        public ObservableCollection<QuotationItem> Items { get; set; } = new();
        private Quotation _quotation;
        private List<Customerinfo> _customers;
        int invoiceNo = InvoiceNumberManager.GetNextInvoiceNumber();
        // Constructor for quotation-based invoice
        public InvoiceWindow(Quotation quotation)
        {
            InitializeComponent();
            _quotation = quotation;
           
            // Fill customer & invoice info
            CustomerTextBox.Text =
                $"{_quotation.Customer?.Name}\n{_quotation.Customer?.Address}\n{_quotation.Customer?.ContactNo}";
            InvoiceTextBox.Text =
    $"Invoice No: {invoiceNo.ToString("D5")}\nQuotation No: {_quotation.QuotationNumber}\nDate: {DateTime.Now:yyyy-MM-dd}";

            EmailTextBox.Text = _quotation.Customer?.Email;
            EmailTextBox.Visibility = string.IsNullOrEmpty(_quotation.Customer?.Email)
                ? Visibility.Visible
                : Visibility.Visible;  // If auto email available keep visible for editing.

            foreach (var i in _quotation.Items)
                Items.Add(i);

            SetupBindings();
        }

        // Constructor for manual invoice
        public InvoiceWindow(List<Customerinfo> customers)
        {
            InitializeComponent();
            _customers = customers;

            // Hide TextBox, show ComboBox for manual customer selection
            CustomerTextBox.Visibility = Visibility.Visible;
            CustomerComboBox.Visibility = Visibility.Visible;
            CustomerComboBox.ItemsSource = _customers;
            CustomerComboBox.DisplayMemberPath = "Name";
            LoadDictionary();
            EnsureDictionaryFolderExists();

            // ✅ Generate new invoice number
            InvoiceTextBox.Text =
                $"Invoice No: {invoiceNo.ToString("D5")}\nDate: {DateTime.Now:yyyy-MM-dd}";


            SetupBindings();
        }



        public InvoiceWindow(InvoiceRecord record)
        {
            InitializeComponent();

            CustomerTextBox.Text = record.CustomerDetails;
            InvoiceTextBox.Text = record.InvoiceDetails;
            TotalTextBox.Text = record.TotalAmount;

            foreach (var i in record.Items)
                Items.Add(new QuotationItem
                {
                    Description = i.Description,
                    Qty = i.IsTitle ? null : (int?)i.Qty, // <-- explicit cast to int?
                    UnitAmount = i.UnitPrice,
                    TotalAmount = i.IsTitle ? null : i.Qty * i.UnitPrice,
                    IsTitle = i.IsTitle
                });

            SetupBindings();
        }



        private void EnsureDictionaryFolderExists()
        {
            string folder = Path.GetDirectoryName(dictionaryPath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }


        private void LoadDictionary()
        {
            if (File.Exists(dictionaryPath))
                dictionary = File.ReadAllLines(dictionaryPath).ToList();
        }

        private void AddWordToDictionary(string word)
        {
            EnsureDictionaryFolderExists(); // ✅ folder exists check
            word = word.Trim();
            if (!string.IsNullOrEmpty(word) && !dictionary.Contains(word))
            {
                dictionary.Add(word);
                File.AppendAllText(dictionaryPath, word + Environment.NewLine);
            }
        }




        private void ItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            ListBox suggestionList = FindSiblingListBox(tb);

            if (tb == null || suggestionList == null) return;

            string text = tb.Text;
            if (string.IsNullOrEmpty(text))
            {
                suggestionList.Visibility = Visibility.Collapsed;
                return;
            }

            var suggestions = dictionary
                .Where(w => w.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (suggestions.Any())
            {
                suggestionList.ItemsSource = suggestions;
                suggestionList.Visibility = Visibility.Visible;
            }
            else
            {
                suggestionList.Visibility = Visibility.Collapsed;
            }
        }



        private void ItemTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            ListBox suggestionList = FindSiblingListBox(tb);
            if (suggestionList == null || suggestionList.Visibility != Visibility.Visible) return;

            if (e.Key == Key.Down)
            {
                if (suggestionList.SelectedIndex < suggestionList.Items.Count - 1)
                    suggestionList.SelectedIndex++;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (suggestionList.SelectedIndex > 0)
                    suggestionList.SelectedIndex--;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (suggestionList.SelectedItem != null)
                {
                    tb.Text = suggestionList.SelectedItem.ToString();
                }

                // Add typed word to dictionary if not exists
                AddWordToDictionary(tb.Text);

                tb.CaretIndex = tb.Text.Length;
                suggestionList.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }

        }

        private ListBox FindSiblingListBox(TextBox tb)
        {
            var parent = tb.Parent as Grid;
            return parent?.Children.OfType<ListBox>().FirstOrDefault();
        }


        private void ItemSuggestionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox?.SelectedItem == null)
                return;

            // Find the corresponding TextBox
            Grid parent = listBox.Parent as Grid;
            TextBox tb = parent?.Children.OfType<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.Text = listBox.SelectedItem.ToString();
                tb.CaretIndex = tb.Text.Length;
                listBox.Visibility = Visibility.Collapsed;
            }

            // Clear selection so next click triggers again
            listBox.SelectedItem = null;
        }


        private void SaveDictionaryFromItems()
        {
            foreach (var item in Items)
            {
                string word = item.Description?.Trim();
                if (!string.IsNullOrEmpty(word))
                    AddWordToDictionary(word);
            }
        }




        private void SetupBindings()
        {
            // Bind the left side items control
            ItemsControlList.DataContext = this;

            // ✅ ADD THIS LINE - Bind the preview items control
            PreviewItemsList.DataContext = this;

            foreach (var item in Items)
                item.PropertyChanged += Item_PropertyChanged;

            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (QuotationItem newItem in e.NewItems)
                        newItem.PropertyChanged += Item_PropertyChanged;
                }
            };

            UpdateGrandTotal();
            UpdatePreviewDisplay(); // ✅ Add this to update preview on load
        }

        // ✅ ADD THIS NEW METHOD to update preview display
        private void UpdatePreviewDisplay()
        {
            PreviewCustomer.Text = CustomerTextBox.Text;
            PreviewInvoiceDetails.Text = InvoiceTextBox.Text;
            PreviewTotal.Text = TotalTextBox.Text;
        }



        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QuotationItem.TotalAmount))
                UpdateGrandTotal();
        }

        // ✅ UPDATE THIS METHOD to also update preview
        private void UpdateGrandTotal()
        {
            decimal total = Items.Where(i => !i.IsTitle && i.TotalAmount.HasValue)
                                 .Sum(i => i.TotalAmount.Value);
            TotalTextBox.Text = total.ToString("N2");

            // Update preview total
            PreviewTotal.Text = TotalTextBox.Text;
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is QuotationItem item)
                Items.Remove(item);
        }

        private void AddTitle_Click(object sender, RoutedEventArgs e)
        {
            Items.Add(new QuotationItem
            {
                Description = "New Title",
                IsTitle = true,
                Qty = null,
                UnitAmount = null,
                TotalAmount = null
            });
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            Items.Add(new QuotationItem
            {
                Description = "New Item",
                IsTitle = false,
                Qty = 1,
                UnitAmount = 0,
                TotalAmount = 0
            });
        }
        private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem is Customerinfo selectedCustomer)
            {
                // Fill customer text
                CustomerTextBox.Text =
                    $"{selectedCustomer.Name}\n{selectedCustomer.Address}\n{selectedCustomer.ContactNo}";

                // Fill email box
                EmailTextBox.Text = selectedCustomer.Email;

                // Show email textbox always (user can edit)
                EmailTextBox.Visibility = Visibility.Visible;
            }
        }




        // ✅ Print button - Direct print (no dialog)
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            SaveDictionaryFromItems();

            string customerDetails = CustomerTextBox.Visibility == Visibility.Visible
                ? CustomerTextBox.Text
                : (CustomerComboBox.SelectedItem as Customerinfo)?.Name + "\n" +
                  (CustomerComboBox.SelectedItem as Customerinfo)?.Address + "\n" +
                  (CustomerComboBox.SelectedItem as Customerinfo)?.ContactNo;

            var items = Items.Select(i => new InvoiceItem
            {
                Description = i.Description,
                Qty = i.IsTitle ? 0 : (i.Qty ?? 0),
                UnitPrice = i.IsTitle ? 0 : (decimal)(i.UnitAmount ?? 0),
                IsTitle = i.IsTitle
            }).ToList();

            InvoicePrinter printer = new InvoicePrinter
            {
                CustomerDetails = customerDetails,
                InvoiceDetails = InvoiceTextBox.Text,
                TotalAmount = TotalTextBox.Text,
                Items = items
            };

            // Save to JSON
            SaveInvoiceToJson(new InvoiceRecord
            {
                InvoiceNo = invoiceNo,
                CustomerDetails = customerDetails,
                InvoiceDetails = InvoiceTextBox.Text,
                TotalAmount = TotalTextBox.Text,
                Status = "Printed",
                Items = items
            });

            // Save invoice number
            InvoiceNumberManager.SaveInvoiceNumber(invoiceNo);

            // Direct print
            printer.Print();

            CloseWindow();
        }


        private string GenerateTempPdf()
        {
            string tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string filePath = Path.Combine(tempFolder, $"Invoice_{invoiceNo}.pdf");

            string customerDetails = CustomerTextBox.Text;

            var items = Items.Select(i => new InvoiceItem
            {
                Description = i.Description,
                Qty = i.IsTitle ? 0 : (i.Qty ?? 0),
                UnitPrice = i.IsTitle ? 0 : (decimal)(i.UnitAmount ?? 0),
                IsTitle = i.IsTitle
            }).ToList();

            InvoicePdfExporter.SaveInvoiceAsPdf(
                filePath,
                customerDetails,
                InvoiceTextBox.Text,
                TotalTextBox.Text,
                items
            );

            // ✅ Save to JSON - Meka add karanna ona
            SaveInvoiceToJson(new InvoiceRecord
            {
                InvoiceNo = invoiceNo,
                CustomerDetails = customerDetails,
                InvoiceDetails = InvoiceTextBox.Text,
                TotalAmount = TotalTextBox.Text,
                Status = "Email",
                Items = items
            });
            CloseWindow();
            return filePath;

        }


        private async void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter an email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Generate PDF
            string pdfPath = GenerateTempPdf();

            // Create body
            string body = $"Dear Customer,<br><br>Please find attached the invoice #{invoiceNo}.<br><br>Thank you.";

            // Send email
            var mail = new MailServiceFinance();
            bool sent = await mail.SendEmailWithAttachmentAsync(
                email,
                null,
                $"Invoice #{invoiceNo:D5}",
                body,
                pdfPath
            );

            if (sent)
                MessageBox.Show("Email sent successfully!");
            else
                MessageBox.Show("Failed to send email.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }




        private void SavePdf_Click(object sender, RoutedEventArgs e)
        {
            SaveDictionaryFromItems();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PDF Files (*.pdf)|*.pdf";
            dlg.FileName = "Invoice.pdf"; // default name
            dlg.DefaultExt = ".pdf";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;

                string customerDetails = CustomerTextBox.Visibility == Visibility.Visible
                    ? CustomerTextBox.Text
                    : (CustomerComboBox.SelectedItem as Customerinfo)?.Name + "\n" +
                      (CustomerComboBox.SelectedItem as Customerinfo)?.Address + "\n" +
                      (CustomerComboBox.SelectedItem as Customerinfo)?.ContactNo;

                var items = Items.Select(i => new InvoiceItem
                {
                    Description = i.Description,
                    Qty = i.IsTitle ? 0 : (i.Qty ?? 0),
                    UnitPrice = i.IsTitle ? 0 : (decimal)(i.UnitAmount ?? 0),
                    IsTitle = i.IsTitle
                }).ToList();

                // Save PDF
                InvoicePdfExporter.SaveInvoiceAsPdf(
                    filePath,
                    customerDetails,
                    InvoiceTextBox.Text,
                    TotalTextBox.Text,
                    items
                );

                // ✅ Save to JSON - Meka add karanna ona
                SaveInvoiceToJson(new InvoiceRecord
                {
                    InvoiceNo = invoiceNo,
                    CustomerDetails = customerDetails,
                    InvoiceDetails = InvoiceTextBox.Text,
                    TotalAmount = TotalTextBox.Text,
                    Status = "PDF",
                    Items = items
                });

                // Save invoice number
                InvoiceNumberManager.SaveInvoiceNumber(invoiceNo);

                CloseWindow();

                MessageBox.Show("PDF saved successfully!");
            }
        }






        private void SaveInvoiceToJson(InvoiceRecord record)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
            string file = Path.Combine(folder, "Invoices.json");

            // Create folder if missing
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            List<InvoiceRecord> allInvoices = new();

            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                allInvoices = JsonConvert.DeserializeObject<List<InvoiceRecord>>(json) ?? new();
            }

            allInvoices.Add(record);

            File.WriteAllText(file, JsonConvert.SerializeObject(allInvoices, Formatting.Indented));
        }


        private void CloseWindow()
        {
            this.Close();
        }

        // Add this method to your InvoiceWindow class

        // ✅ UPDATE THIS METHOD to refresh preview
        private void UpdatePreview(object sender, TextChangedEventArgs e)
        {
            PreviewCustomer.Text = CustomerTextBox.Text;
            PreviewInvoiceDetails.Text = InvoiceTextBox.Text;
        }


    }



    public class InvoiceRecord
    {
        public int InvoiceNo { get; set; }
        public string CustomerDetails { get; set; }
        public string InvoiceDetails { get; set; }
        public string TotalAmount { get; set; }
        public string Status { get; set; }
        public List<InvoiceItem> Items { get; set; }
    }


}
