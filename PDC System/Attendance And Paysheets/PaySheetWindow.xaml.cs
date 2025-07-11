using Microsoft.Win32;
using Newtonsoft.Json;
using PDC_System.Attendance_And_Paysheets;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PDC_System
{
    public partial class PaySheetWindow : Window
    {
        // Private fields
        private Attendance _selectedEmployee;
        private List<ExtraItem> extraEarnings;
        private List<ExtraItem> extraDeductions;
        private PaysheetDetails payers; // Make sure this is initialized


        // Constructor
        public PaySheetWindow(Attendance selectedEmployee)
        {
            InitializeComponent();

            // Set window height to full screen height
            this.Height = SystemParameters.FullPrimaryScreenHeight;

            // Initialize private fields
            _selectedEmployee = selectedEmployee;
            extraEarnings = new List<ExtraItem>();
            extraDeductions = new List<ExtraItem>();
            payers = new PaysheetDetails(); // Initialize payers here

            // Load pay sheet data
            LoadPaySheetData();
        }

        // Method to load pay sheet data
        private void LoadPaySheetData()
        {
            // Set UI fields with data from the selected employee
            MonthTextBlock.Text = _selectedEmployee.Month;
            EmployeeNameTextBlock.Text = _selectedEmployee.Employee_Name;
            EmployeeDesignatureTextBlock.Text = _selectedEmployee.jobr;
            WorkingDaysTextBlock.Text = _selectedEmployee.WorkingDays.ToString();
            AbsentDaysTextBlock.Text = _selectedEmployee.AbsentDays.ToString();
            NoPayTextBlock.Text = _selectedEmployee.No_PAY.ToString();

            EarlyTextBlock.Text = $"{_selectedEmployee.eerly} h";
            LateTextBlock.Text = $"{_selectedEmployee.elate} h";
            OTMinTextBlock.Text = $"{_selectedEmployee.AOT:F2} h";
            EmployeeAddressTextBlock.Text = $"{_selectedEmployee.address1},{_selectedEmployee.address2},{_selectedEmployee.city}";
            EmployeeContactTextBlock.Text = $"{_selectedEmployee.contact}";
            SalaryTextBlock.Text = $"{_selectedEmployee.E_Salary} LKR";
            payers.Name = _selectedEmployee.Employee_Name;



            // Calculate overtime and absence values
            decimal actualOvertime = _selectedEmployee.E_OT * ((decimal)_selectedEmployee.AOT);
            int absent = _selectedEmployee.AbsentDays * _selectedEmployee.absentlkr;
            decimal doubleOvertime = Math.Round(_selectedEmployee.E_DOT * ((decimal)_selectedEmployee.edot), 2);
            decimal edoubleOvertime = _selectedEmployee.edot;

            

            // Set text blocks for overtime and absence values
            DOTMinTextBlock.Text = $"{edoubleOvertime:F1} h";
            ActualOvertimeTextBlock.Text = $"{actualOvertime:F2} LKR";
            AbsentTextBlock.Text = $"{absent:F2} LKR";
            DoubleOvertimeTextBlock.Text = $"{doubleOvertime:F2} LKR";

            // Set text blocks for other earnings/deductions
            LoansTextBlock.Text = $"{_selectedEmployee.Loans:F2} LKR";
            CollectedMoneyTextBlock.Text = $"{_selectedEmployee.CollectedMoney:F2} LKR";
            ETFTextBlock.Text = $"{_selectedEmployee.ETF:F2} LKR";

            // Calculate totals
            CalculateTotals();
        }

        // Method to calculate total earnings, deductions, and net salary
        private void CalculateTotals()
        {
            decimal totalEarnings = _selectedEmployee.E_Salary + (((decimal)_selectedEmployee.AOT) * _selectedEmployee.E_OT) + (_selectedEmployee.E_DOT * _selectedEmployee.DOTMin);
            foreach (var earning in extraEarnings)
                totalEarnings += earning.Amount;

            decimal totalDeductions = _selectedEmployee.Loans + _selectedEmployee.CollectedMoney + _selectedEmployee.ETF + (_selectedEmployee.AbsentDays * _selectedEmployee.absentlkr);
            foreach (var deduction in extraDeductions)
                totalDeductions += deduction.Amount;

            // Update UI with totals
            TotalEarningsTextBlock.Text = $"Total Earnings:          {totalEarnings:N2} LKR";
            TotalDeductionsTextBlock.Text = $"Total Deductions:          {totalDeductions:N2} LKR";
            NetSalaryTextBlock.Text = $"{(totalEarnings - totalDeductions):N2} LKR";
            payers.pamount = (totalEarnings - totalDeductions);
            payers.ptotalEarnings = $"{totalEarnings:N2} LKR";
            payers.totalDeductions = $"{totalDeductions:N2} LKR";

            // Show the system generated date and time
            SystemGeneratedTextBlock.Text = $"System Generated | {DateTime.Now}";
        }

        // Event handler to add extra earnings
        private void AddExtraEarnings_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter Earning Name:", "Extra Earnings");

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Earning name cannot be empty.");
                return;
            }

            string amountInput = Microsoft.VisualBasic.Interaction.InputBox("Enter Amount:", "Extra Earnings");

            if (!decimal.TryParse(amountInput, out decimal amount) || amount < 0)
            {
                MessageBox.Show("Invalid amount. Please enter a valid positive number.");
                return;
            }

            // Add new earning item and update the UI
            extraEarnings.Add(new ExtraItem { Name = name, Amount = amount });
            ExtraEarningsListBox.Items.Add($"{amount} LKR");
            ExtraEarningsNameListBox.Items.Add($"{name} LKR");

            // Recalculate totals
            CalculateTotals();
        }

        // Event handler to add extra deductions
        private void AddExtraDeductions_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter Deduction Name:", "Extra Deductions");

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Deduction name cannot be empty.");
                return;
            }

            string amountInput = Microsoft.VisualBasic.Interaction.InputBox("Enter Amount:", "Extra Deductions");

            if (!decimal.TryParse(amountInput, out decimal amount) || amount < 0)
            {
                MessageBox.Show("Invalid amount. Please enter a valid positive number.");
                return;
            }

            // Add new deduction item and update the UI
            extraDeductions.Add(new ExtraItem { Name = name, Amount = amount });
            ExtraDeductionsNameListBox.Items.Add($"{name}");
            ExtraDeductionsListBox.Items.Add($"{amount} LKR");

            // Recalculate totals
            CalculateTotals();
        }

        // Event handler to print the pay sheet
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // Create a PrintDialog to allow the user to select the printer
            PrintDialog printDialog = new PrintDialog();

            // Set the page size to A5 (148.5mm x 210mm)
            printDialog.PrintTicket.PageMediaSize = new PageMediaSize(148.5 * 200 / 30, 210 * 200 / 30); // Convert mm to pixels (96 DPI)

            // Show the PrintDialog
            if (printDialog.ShowDialog() == true)
            {
                double width = StackPanelContent.ActualWidth;
                double height = StackPanelContent.ActualHeight;

                // Verify content size is valid for printing
                if (width <= 0 || height <= 0)
                {
                    MessageBox.Show("Content size is invalid for printing.");
                    return;
                }

                try
                {
                    // Create a RenderTargetBitmap to capture the content of the StackPanel
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(StackPanelContent);

                    // Create a drawing visual for printing
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        // Center the content on the A5 paper
                        double leftMargin = (printDialog.PrintableAreaWidth - renderBitmap.Width) / 2;
                        double topMargin = (printDialog.PrintableAreaHeight - renderBitmap.Height) / 2;

                        // Draw the image at the center of the page
                        drawingContext.DrawImage(renderBitmap, new Rect(leftMargin, topMargin, renderBitmap.Width, renderBitmap.Height));
                    }

                    // Print the visual
                    printDialog.PrintVisual(drawingVisual, "Pay Sheet");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while printing: {ex.Message}");
                }
            }
        }

        private void SaveAsPdf_Click(object sender, RoutedEventArgs e)
        {
            myScrollViewer.ScrollToVerticalOffset(0);
            myScrollViewer.ScrollToHorizontalOffset(0);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{_selectedEmployee.Employee_Name}_{_selectedEmployee.Month}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result != true) return;

            string userSelectedPath = saveFileDialog.FileName;
            string projectFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedPaysheets");

            // Ensure the project folder exists
            if (!Directory.Exists(projectFolderPath))
            {
                Directory.CreateDirectory(projectFolderPath);
            }

            // Get only the file name from the selected path
            string fileName = Path.GetFileName(userSelectedPath);

            // Combine the project folder path with the file name
            string projectFilePath = Path.Combine("SavedPaysheets", fileName);

         
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Pay Sheet";

            double a4Width = 595, a4Height = 842;
            PdfPage page = document.AddPage();
            page.Width = a4Width;
            page.Height = a4Height;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            double dpi = 300, scale = dpi / 96;
            double stackPanelWidth = StackPanelContent.ActualWidth;
            double stackPanelHeight = StackPanelContent.ActualHeight;

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)(stackPanelWidth * scale),
                (int)(stackPanelHeight * scale),
                dpi, dpi, PixelFormats.Pbgra32
            );
            renderBitmap.Render(StackPanelContent);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                encoder.Save(memoryStream);

                XImage img = XImage.FromStream(memoryStream);
                double scaleX = a4Width / img.PixelWidth * 72;
                double scaleY = a4Height / img.PixelHeight * 72;
                double scaleFactor = Math.Min(scaleX, scaleY);
                double width = img.PixelWidth * scaleFactor / 72;
                double height = img.PixelHeight * scaleFactor / 72;
                double leftOffset = (a4Width - width) / 2;

                gfx.DrawImage(img, leftOffset, 0, width, height);
            }

            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PaysheetData.json");
            List<dynamic> payDataList = new List<dynamic>();

            try
            {
                if (File.Exists(jsonFilePath))
                {
                    string existingData = File.ReadAllText(jsonFilePath);
                    payDataList = JsonConvert.DeserializeObject<List<dynamic>>(existingData) ?? new List<dynamic>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading the JSON file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                document.Save(userSelectedPath);
                File.Copy(userSelectedPath, projectFilePath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving the PDF: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newPayData = new
            {
                payers.Name,
                _selectedEmployee.Month,
                payers.pamount,
                payers.ptotalEarnings,
                payers.totalDeductions,
                userFilePath = userSelectedPath,
                pfilepath = projectFilePath,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            payDataList.Add(newPayData);

            try
            {
                File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(payDataList, Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to the JSON file: {ex.Message}", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Pay sheet saved successfully.\nUser Location: {userSelectedPath}\nProject Location: {projectFilePath}", "PDF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }


    // ExtraItem class to hold earnings/deductions
    public class ExtraItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }

}