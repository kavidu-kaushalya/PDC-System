using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class QuotationCreateWindow : Window
    {
        public ObservableCollection<QuotationItem> QuotationItems { get; set; }
        public DateTime QuoteDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string QuoteNumber { get; set; }
        public string Qname { get; set; }
        public string FilePath { get; internal set; }
        public decimal gtotal { get; set; }
        private Customer _selectedCustomer;

        private static int quoteCounter;
        private static string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "quoteSettings.json");

        public QuotationCreateWindow(Customer selectedCustomer)
        {
            InitializeComponent();

            _selectedCustomer = selectedCustomer;
            QuotationItems = new ObservableCollection<QuotationItem>();
            ItemsDataGrid.ItemsSource = QuotationItems;

            // Load the quote counter from settings and set the default quote number
            quoteCounter = LoadQuoteCounter();
            QuoteNumber = GenerateQuoteNumber();
            QuoteNumberTextBlock.Text = QuoteNumber;

            // Set customer details
            CustomerComboBox.Text = _selectedCustomer.Name;
            CustomerAddressTextBlock.Text = _selectedCustomer.Address;
            CustomerContactTextBlock.Text = _selectedCustomer.ContactNo;
        }

        private string GenerateQuoteNumber()
        {
            quoteCounter++; // Increment each time a new quote is generated
            SaveQuoteCounter(quoteCounter); // Save the updated number
            return quoteCounter.ToString();
        }

        private static void SaveQuoteCounter(int counter)
        {
            var settings = new QuoteSettings { QuoteCounter = counter };
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsFilePath, json);
        }

        private static int LoadQuoteCounter()
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<QuoteSettings>(json);
                return settings?.QuoteCounter ?? 1000; // Default to 1000 if not found
            }
            return 1000; // Default to 1000 if the file doesn't exist
        }

        private void GenerateQuote_Click(object sender, RoutedEventArgs e)
        {
            // Calculate totals
            decimal subtotal = 0m;
            foreach (var item in QuotationItems)
            {
                subtotal += item.Total;
            }

            decimal grandTotal = subtotal;
            gtotal = grandTotal;

            // Update UI
            GrandTotalTextBlock.Text = $"Grand Total: LKR {grandTotal:N2}";
        }

        public class QuotationItem : INotifyPropertyChanged
        {
            private string _description;
            private decimal _unitPrice;
            private int _quantity;

            public string Description
            {
                get => _description;
                set
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }

            public decimal UnitPrice
            {
                get => _unitPrice;
                set
                {
                    _unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(Total));
                }
            }

            public int Quantity
            {
                get => _quantity;
                set
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                }
            }

            public decimal Total => UnitPrice * Quantity;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SaveAsPdf_Click(object sender, RoutedEventArgs e)
        {
            // Calculate totals
            decimal subtotal = 0m;
            foreach (var item in QuotationItems)
            {
                subtotal += item.Total;
            }

            decimal grandTotal = subtotal;
            gtotal = grandTotal;

            // Update UI
            GrandTotalTextBlock.Text = $"Grand Total: LKR {grandTotal:N2}";

            try
                
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Enter Quotation Name:");

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Quotation name cannot be empty.");
                    return;
                }

                Qname=name;


                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"{_selectedCustomer.Name}_{Qname}_{QuoteNumber}_{DateTime.Now:yyyy-MM-dd_HHmmss}.pdf"
                };

                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string userSelectedPath = saveFileDialog.FileName;
                    string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string projectSavePath = Path.Combine(projectDirectory, "SavedQuotation");

                    if (!Directory.Exists(projectSavePath))
                    {
                        Directory.CreateDirectory(projectSavePath);
                    }

                    // Get only the file name from the selected path
                    string fileName = Path.GetFileName(userSelectedPath);

                    // Combine the project save path with the file name
                    string projectFilePath = Path.Combine("SavedQuotation", fileName);

                    // Store main file path as relative
                    FilePath = projectFilePath;


                    // Render content to bitmap
                    double dpi = 192;
                    double scale = dpi / 96;
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)(StackPanelContent.ActualWidth * scale),
                        (int)(StackPanelContent.ActualHeight * scale),
                        dpi, dpi, PixelFormats.Pbgra32
                    );
                    renderBitmap.Render(StackPanelContent);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        encoder.Save(memoryStream);

                        // Save first PDF
                        SavePdf(userSelectedPath, memoryStream);

                        // Reset the memory stream position before creating the second PDF
                        memoryStream.Position = 0;

                        // Save second PDF
                        SavePdf(projectFilePath, memoryStream);
                    }

                    MessageBox.Show($"Quotation saved:\nCustom location: {userSelectedPath}\nProject location: {projectFilePath}",
                                    "PDF Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePdf(string filePath, MemoryStream memoryStream)
        {
            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Quotation";
                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XImage img = XImage.FromStream(memoryStream);

                double scaleX = page.Width / img.PixelWidth;
                double scaleY = page.Height / img.PixelHeight;
                double scaleFactor = Math.Min(scaleX, scaleY);

                double x = (page.Width - (img.PixelWidth * scaleFactor)) / 4;
                double y = (page.Height - (img.PixelHeight * scaleFactor)) / 4;

                gfx.DrawImage(img, x, y, img.PixelWidth * scaleFactor, img.PixelHeight * scaleFactor);
                document.Save(filePath);
            }
        }

        private void ItemsDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            if (dataGrid != null && dataGrid.Items.Count > 0)
            {
                // Ensure that the new row is always added at the bottom
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            }
        }


    }

    public class QuoteSettings
    {
        public int QuoteCounter { get; set; }
    }
}
