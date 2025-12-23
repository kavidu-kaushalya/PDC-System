using Google.Apis.PeopleService.v1.Data;
using iText.StyledXmlParser.Jsoup.Helper;
using Newtonsoft.Json;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.ObjectModel;
using iTextSharp.text;
using iTextSharp.text.pdf;

using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class AddQuotationWindow : Window
    {
        public ObservableCollection<QuotationItem> Items { get; set; } = new();

        private DateOnly issueDate;

        public event Action QuotationSaved;   // <-- add this

        public DateOnly ValidDate { get; private set; }
        public int ValidDays { get; private set; }

        private readonly string saversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
        private string jsonFile;
        private string currentQuoNumber;

        private readonly string customerFile;
        private List<Customerinfo> customers = new List<Customerinfo>();

        private Quotation editingQuotation; // field to hold existing quotation

        public AddQuotationWindow(Quotation quotationToEdit = null)
        {
            InitializeComponent();



            QuestPDF.Settings.License = LicenseType.Community;

            // Create Savers folder if not exists
            if (!Directory.Exists(saversFolder))
                Directory.CreateDirectory(saversFolder);

            jsonFile = Path.Combine(saversFolder, "QuoData.json");

            GenerateQuoNumber();

            ItemsControlList.ItemsSource = Items;
            TempGrid.ItemsSource = Items;

            // Update total when items or properties change
            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (QuotationItem item in e.NewItems)
                        item.PropertyChanged += Item_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (QuotationItem item in e.OldItems)
                        item.PropertyChanged -= Item_PropertyChanged;
                }
                UpdateTotalSum();
            };

            customerFile = Path.Combine(saversFolder, "customers.json");

            // Load customers
            if (File.Exists(customerFile))
            {
                string json = File.ReadAllText(customerFile);
                customers = JsonConvert.DeserializeObject<List<Customerinfo>>(json) ?? new List<Customerinfo>();
                CustomerComboBox.ItemsSource = customers;
                CustomerComboBox.DisplayMemberPath = "Name";
            }

            // If editing, load the data into the window
            if (quotationToEdit != null)
            {
                editingQuotation = quotationToEdit;
                LoadQuotationData(editingQuotation);
            }
            else
            {
                GenerateQuoNumber();
            }
        }


        private void LoadQuotationData(Quotation quotation)
        {
            currentQuoNumber = quotation.QuotationNumber;
            QuoNumberTextBox.Text = $"Quotation Number: {currentQuoNumber}";

            // Load customer
            CustomerComboBox.SelectedItem = customers.FirstOrDefault(c => c.Name == quotation.Customer.Name);

            // Load items
            Items.Clear();
            foreach (var item in quotation.Items)
                Items.Add(new QuotationItem
                {
                    Description = item.Description,
                    Qty = item.Qty,
                    UnitAmount = item.UnitAmount,
                    IsTitle = item.IsTitle
                });

            // Load dates
            Issue_Date.SelectedDate = quotation.IssueDate.ToDateTime(new TimeOnly());
            Valid_Date.SelectedDate = quotation.ValidDate.ToDateTime(new TimeOnly());
        }


        private void GenerateQuoNumber()
        {
            List<QuoNo> jobs = new List<QuoNo>();

            if (File.Exists(jsonFile))
            {
                string json = File.ReadAllText(jsonFile);
                jobs = JsonConvert.DeserializeObject<List<QuoNo>>(json) ?? new List<QuoNo>();
            }

            if (jobs.Count == 0)
            {
                currentQuoNumber = "0001";
            }
            else
            {
                string lastJob = jobs.Last().QuoNumber;
                int num;

                if (lastJob.Contains("-"))
                    num = int.Parse(lastJob.Split('-')[1]);
                else
                    num = int.Parse(lastJob);

                num++;
                currentQuoNumber = num.ToString("D4");
            }

            QuoNumberTextBox.Text = ($"Quotation Number: {currentQuoNumber}");
        }

        private void Save()
        {
            List<QuoNo> jobs = new List<QuoNo>();

            if (File.Exists(jsonFile))
            {
                string json = File.ReadAllText(jsonFile);
                jobs = JsonConvert.DeserializeObject<List<QuoNo>>(json) ?? new List<QuoNo>();
            }

            if (!jobs.Any(j => j.QuoNumber == currentQuoNumber))
            {
                jobs.Add(new QuoNo { QuoNumber = currentQuoNumber });
                File.WriteAllText(jsonFile, JsonConvert.SerializeObject(jobs, Formatting.Indented));
            }

            SaveQuotationToJson(editingQuotation); // pass the object to update
            QuotationSaved?.Invoke(); // refresh parent window
            GenerateQuoNumber(); // for next new quotation
        }


        private void SaveQuotationToJson(Quotation quotationToUpdate = null)
        {
            string quotationFile = Path.Combine(saversFolder, "SavedQuotations.json");
            List<Quotation> quotations = new List<Quotation>();

            if (File.Exists(quotationFile))
            {
                string json = File.ReadAllText(quotationFile);
                quotations = JsonConvert.DeserializeObject<List<Quotation>>(json) ?? new List<Quotation>();
            }

            if (CustomerComboBox.SelectedItem is not Customerinfo selectedCustomer)
            {
                CustomMessageBox.Show("Please select a customer before saving.");
                return;
            }

            var quotation = new Quotation
            {
                QuotationNumber = currentQuoNumber,
                Customer = selectedCustomer,
                Items = Items.ToList(),
                Total = Items.Where(i => i.TotalAmount.HasValue).Sum(i => i.TotalAmount.Value),
                IssueDate = Issue_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Issue_Date.SelectedDate.Value) : default,
                ValidDate = Valid_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Valid_Date.SelectedDate.Value) : default,
                ValidDays = Issue_Date.SelectedDate.HasValue && Valid_Date.SelectedDate.HasValue
                            ? (Valid_Date.SelectedDate.Value - Issue_Date.SelectedDate.Value).Days
                            : 0
            };

            if (quotationToUpdate != null)
            {
                // Update existing
                int index = quotations.FindIndex(q => q.QuotationNumber == quotationToUpdate.QuotationNumber);
                if (index >= 0)
                    quotations[index] = quotation;
            }
            else
            {
                // Add new
                quotations.Add(quotation);
            }

            File.WriteAllText(quotationFile, JsonConvert.SerializeObject(quotations, Formatting.Indented));
        }



        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QuotationItem.TotalAmount))
                UpdateTotalSum();
        }

        private void UpdateTotalSum()
        {
            decimal sum = Items.Where(i => i.TotalAmount.HasValue).Sum(i => i.TotalAmount.Value);
            TotalAmount.Text = $"Total:  {sum} LKR";
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            Items.Add(new QuotationItem { Qty = 1, UnitAmount = 0 });
        }

        private void AddTitle_Click(object sender, RoutedEventArgs e)
        {
            Items.Add(new QuotationItem { Description = "New Title", IsTitle = true });
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is QuotationItem item)
                Items.Remove(item);
        }

        private void CustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerComboBox.SelectedItem is Customerinfo selectedCustomer)
            {
                NameTextBox.Text = selectedCustomer.Name;
                AddressTextBox.Text = selectedCustomer.Address;
                ContactNoTextBox.Text = selectedCustomer.ContactNo;
                CompanyTextBox.Text = selectedCustomer.companyname;
                EmailTextBox.Text = selectedCustomer.Email;
                TypeTextBox.Text = selectedCustomer.Type;


                CustomerNameText.Text = selectedCustomer.Name;
                CustomerAddress.Text = selectedCustomer.Address;
                CustomerContact.Text = selectedCustomer.ContactNo;
                
            }
        }


        private void FilterChanged(object sender, EventArgs e)
        {
            // Assign IssueDate
            issueDate = Issue_Date.SelectedDate.HasValue
                ? DateOnly.FromDateTime(Issue_Date.SelectedDate.Value)
                : default;

            // Assign ValidDate
            ValidDate = Valid_Date.SelectedDate.HasValue
                ? DateOnly.FromDateTime(Valid_Date.SelectedDate.Value)
                : default;

            // Calculate ValidDays
            ValidDays = Issue_Date.SelectedDate.HasValue && Valid_Date.SelectedDate.HasValue
                ? (Valid_Date.SelectedDate.Value - Issue_Date.SelectedDate.Value).Days
                : 0;


            IssueDateText.Text = Issue_Date.SelectedDate.HasValue
                ? $"Issue Date: {issueDate}"
                : "Issue Date: ";
            ValidDateText.Text = Valid_Date.SelectedDate.HasValue
                ? $"Valid Date: {ValidDate}"
                : "Valid Date: ";



            ValidDaysText.Text = $"The price mentioned here only valid for {ValidDays} days.";

            // Optional: you can add this to update UI or debug output
            // Debug.WriteLine($"ValidDays: {ValidDays}");
        }



        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            Save();

            if (CustomerComboBox.SelectedItem is not Customerinfo selectedCustomer)
            {
                CustomMessageBox.Show("Please select a customer.");
                return;
            }

            var quotation = new Quotation
            {
                QuotationNumber = currentQuoNumber,
                Customer = selectedCustomer,
                Items = Items.ToList(),
                Total = Items.Where(i => i.TotalAmount.HasValue).Sum(i => i.TotalAmount.Value),
                IssueDate = Issue_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Issue_Date.SelectedDate.Value) : default,
                ValidDate = Valid_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Valid_Date.SelectedDate.Value) : default,
                ValidDays = Issue_Date.SelectedDate.HasValue && Valid_Date.SelectedDate.HasValue
                    ? (Valid_Date.SelectedDate.Value - Issue_Date.SelectedDate.Value).Days
                    : 0
            };

            await Task.Run(() =>
            {
                string fileName = $"Quotation_{quotation.QuotationNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    // Replace this line inside Preview_Click:
                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30, 30, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    // Fonts
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLUE);
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLUE);
                    Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    Font regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);

                    // Header
                    document.Add(new Paragraph("Quotation", titleFont));
                    document.Add(new Paragraph($"Quotation No: {quotation.QuotationNumber}", boldFont));
                    document.Add(new Paragraph($"Issue Date: {quotation.IssueDate}     Valid Date: {quotation.ValidDate}", boldFont));
                    document.Add(new Paragraph(" ")); // Space

                    // Customer and Company Information (Two Columns)
                    PdfPTable infoTable = new PdfPTable(2);
                    infoTable.WidthPercentage = 100;
                    infoTable.SpacingBefore = 10f;
                    infoTable.SpacingAfter = 10f;

                    // Customer Info Cell
                    PdfPCell customerCell = new PdfPCell();
                    customerCell.Border = Rectangle.NO_BORDER;
                    customerCell.AddElement(new Paragraph("Customer Information", headerFont));
                    customerCell.AddElement(new Paragraph($"Name: {quotation.Customer.Name}", regularFont));
                    customerCell.AddElement(new Paragraph($"Address: {quotation.Customer.Address}", regularFont));
                    customerCell.AddElement(new Paragraph($"Contact: {quotation.Customer.ContactNo}", regularFont));

                    // Company Info Cell
                    PdfPCell companyCell = new PdfPCell();
                    companyCell.Border = Rectangle.NO_BORDER;
                    companyCell.AddElement(new Paragraph("Our Details", headerFont));
                    companyCell.AddElement(new Paragraph("Company Name: PRIYANTHA DIE CUTTING", regularFont));
                    companyCell.AddElement(new Paragraph("Address: No.07,Waidaya Vidayala Mawatha,Rajagiriya", regularFont));
                    companyCell.AddElement(new Paragraph("Contact: 072 297 8667 | 011 864 267", regularFont));
                    companyCell.AddElement(new Paragraph("Email: priyanthadiecutting@gmail.com", regularFont));

                    infoTable.AddCell(customerCell);
                    infoTable.AddCell(companyCell);
                    document.Add(infoTable);

                    // Items Section
                    document.Add(new Paragraph("Items", headerFont));
                    document.Add(new Paragraph(" ")); // Space

                    // Items Table
                    PdfPTable itemsTable = new PdfPTable(4);
                    itemsTable.WidthPercentage = 100;
                    itemsTable.SetWidths(new float[] { 3f, 1f, 1.5f, 1.5f });
                    itemsTable.SpacingBefore = 5f;

                    // Table Headers
                    PdfPCell headerCell1 = new PdfPCell(new Phrase("Description", boldFont));
                    headerCell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                    headerCell1.Padding = 5;
                    itemsTable.AddCell(headerCell1);

                    PdfPCell headerCell2 = new PdfPCell(new Phrase("Qty", boldFont));
                    headerCell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                    headerCell2.Padding = 5;
                    itemsTable.AddCell(headerCell2);

                    PdfPCell headerCell3 = new PdfPCell(new Phrase("Unit (LKR)", boldFont));
                    headerCell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                    headerCell3.Padding = 5;
                    headerCell3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    itemsTable.AddCell(headerCell3);

                    PdfPCell headerCell4 = new PdfPCell(new Phrase("Total (LKR)", boldFont));
                    headerCell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                    headerCell4.Padding = 5;
                    headerCell4.HorizontalAlignment = Element.ALIGN_RIGHT;
                    itemsTable.AddCell(headerCell4);

                    // Data Rows
                    foreach (var item in quotation.Items)
                    {
                        if (item.IsTitle)
                        {
                            // Title row
                            PdfPCell titleCell = new PdfPCell(new Phrase(item.Description ?? "", boldFont));
                            titleCell.Colspan = 4;
                            titleCell.Padding = 5;
                            titleCell.Border = Rectangle.NO_BORDER;
                            itemsTable.AddCell(titleCell);
                        }
                        else
                        {
                            // Regular item row
                            PdfPCell descCell = new PdfPCell(new Phrase(item.Description ?? "", regularFont));
                            descCell.Padding = 5;
                            descCell.Border = Rectangle.NO_BORDER;
                            itemsTable.AddCell(descCell);

                            PdfPCell qtyCell = new PdfPCell(new Phrase(item.Qty > 0 ? item.Qty.ToString() : "", regularFont));
                            qtyCell.Padding = 5;
                            qtyCell.Border = Rectangle.NO_BORDER;
                            itemsTable.AddCell(qtyCell);

                            PdfPCell unitCell = new PdfPCell(new Phrase(
                                item.UnitAmount.HasValue && item.UnitAmount.Value > 0 ? $"{item.UnitAmount.Value:N2}" : "",
                                regularFont));
                            unitCell.Padding = 5;
                            unitCell.Border = Rectangle.NO_BORDER;
                            unitCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                            itemsTable.AddCell(unitCell);

                            PdfPCell totalCell = new PdfPCell(new Phrase(
                                item.TotalAmount.HasValue && item.TotalAmount.Value > 0 ? $"{item.TotalAmount.Value:N2}" : "",
                                regularFont));
                            totalCell.Padding = 5;
                            totalCell.Border = Rectangle.NO_BORDER;
                            totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                            itemsTable.AddCell(totalCell);
                        }
                    }

                    document.Add(itemsTable);

                    // Total Section
                    PdfPTable totalTable = new PdfPTable(2);
                    totalTable.WidthPercentage = 50;
                    totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalTable.SpacingBefore = 10f;

                    PdfPCell totalLabelCell = new PdfPCell(new Phrase("Total Amount:", boldFont));
                    totalLabelCell.Border = Rectangle.NO_BORDER;
                    totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalTable.AddCell(totalLabelCell);

                    PdfPCell totalValueCell = new PdfPCell(new Phrase($"{quotation.Total:N2} LKR", boldFont));
                    totalValueCell.Border = Rectangle.NO_BORDER;
                    totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalTable.AddCell(totalValueCell);

                    document.Add(totalTable);

                    // Footer Notes
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph($"The price mentioned here only valid for {quotation.ValidDays} days.", boldFont));
                    document.Add(new Paragraph("NOTE: Cheque should be drawn in favor of \"PRIYANTHA DIE CUTTING\".", boldFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph("Prepared By,", regularFont));
                    document.Add(new Paragraph("Priyantha De Costa", regularFont));
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}", smallFont));

                    document.Close();
                }

               

                // Open the PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            });

            CloseWindow();
        }


        private string GenerateQuotationPdf()
        {
            if (CustomerComboBox.SelectedItem is not Customerinfo selectedCustomer)
                throw new Exception("Select customer before generating PDF.");

            var quotation = new Quotation
            {
                QuotationNumber = currentQuoNumber,
                Customer = selectedCustomer,
                Items = Items.ToList(),
                Total = Items.Where(i => i.TotalAmount.HasValue).Sum(i => i.TotalAmount.Value),
                IssueDate = Issue_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Issue_Date.SelectedDate.Value) : default,
                ValidDate = Valid_Date.SelectedDate.HasValue ? DateOnly.FromDateTime(Valid_Date.SelectedDate.Value) : default,
                ValidDays = Issue_Date.SelectedDate.HasValue && Valid_Date.SelectedDate.HasValue
                            ? (Valid_Date.SelectedDate.Value - Issue_Date.SelectedDate.Value).Days
                            : 0
            };

            string tempFolder = Path.Combine(System.IO.Path.GetTempPath(), "Quotations");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            string filePath = Path.Combine(tempFolder, $"Quotation_{quotation.QuotationNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            // Generate PDF (reuse Preview_Click logic)
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLUE);
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLUE);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);

                document.Add(new Paragraph("Quotation", titleFont));
                document.Add(new Paragraph($"Quotation No: {quotation.QuotationNumber}", boldFont));
                document.Add(new Paragraph($"Issue Date: {quotation.IssueDate}     Valid Date: {quotation.ValidDate}", boldFont));
                document.Add(new Paragraph(" "));

                // Customer & Company Info (reuse same as Preview_Click)
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SpacingBefore = 10f;
                infoTable.SpacingAfter = 10f;

                PdfPCell customerCell = new PdfPCell();
                customerCell.Border = Rectangle.NO_BORDER;
                customerCell.AddElement(new Paragraph("Customer Information", headerFont));
                customerCell.AddElement(new Paragraph($"Name: {quotation.Customer.Name}", regularFont));
                customerCell.AddElement(new Paragraph($"Address: {quotation.Customer.Address}", regularFont));
                customerCell.AddElement(new Paragraph($"Contact: {quotation.Customer.ContactNo}", regularFont));

                PdfPCell companyCell = new PdfPCell();
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.AddElement(new Paragraph("Our Details", headerFont));
                companyCell.AddElement(new Paragraph("Company Name: PRIYANTHA DIE CUTTING", regularFont));
                companyCell.AddElement(new Paragraph("Address: No.07,Waidaya Vidayala Mawatha,Rajagiriya", regularFont));
                companyCell.AddElement(new Paragraph("Contact: 072 297 8667 | 011 864 267", regularFont));
                companyCell.AddElement(new Paragraph("Email: priyanthadiecutting@gmail.com", regularFont));

                infoTable.AddCell(customerCell);
                infoTable.AddCell(companyCell);
                document.Add(infoTable);

                // Items Table
                PdfPTable itemsTable = new PdfPTable(4);
                itemsTable.WidthPercentage = 100;
                itemsTable.SetWidths(new float[] { 3f, 1f, 1.5f, 1.5f });
                itemsTable.SpacingBefore = 5f;

                PdfPCell headerCell1 = new PdfPCell(new Phrase("Description", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5 };
                PdfPCell headerCell2 = new PdfPCell(new Phrase("Qty", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5 };
                PdfPCell headerCell3 = new PdfPCell(new Phrase("Unit (LKR)", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT };
                PdfPCell headerCell4 = new PdfPCell(new Phrase("Total (LKR)", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT };

                itemsTable.AddCell(headerCell1);
                itemsTable.AddCell(headerCell2);
                itemsTable.AddCell(headerCell3);
                itemsTable.AddCell(headerCell4);

                foreach (var item in quotation.Items)
                {
                    if (item.IsTitle)
                    {
                        PdfPCell titleCell = new PdfPCell(new Phrase(item.Description ?? "", boldFont)) { Colspan = 4, Padding = 5, Border = Rectangle.NO_BORDER };
                        itemsTable.AddCell(titleCell);
                    }
                    else
                    {
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Description ?? "", regularFont)) { Padding = 5, Border = Rectangle.NO_BORDER });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Qty > 0 ? item.Qty.ToString() : "", regularFont)) { Padding = 5, Border = Rectangle.NO_BORDER });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.UnitAmount.HasValue ? $"{item.UnitAmount.Value:N2}" : "", regularFont)) { Padding = 5, Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.TotalAmount.HasValue ? $"{item.TotalAmount.Value:N2}" : "", regularFont)) { Padding = 5, Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }
                }

                document.Add(itemsTable);

                // Total
                PdfPTable totalTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_RIGHT, SpacingBefore = 10f };
                totalTable.AddCell(new PdfPCell(new Phrase("Total Amount:", boldFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                totalTable.AddCell(new PdfPCell(new Phrase($"{quotation.Total:N2} LKR", boldFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                document.Add(totalTable);

                // Footer
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph($"The price mentioned here only valid for {quotation.ValidDays} days.", boldFont));
                document.Add(new Paragraph("NOTE: Cheque should be drawn in favor of \"PRIYANTHA DIE CUTTING\".", boldFont));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Prepared By,", regularFont));
                document.Add(new Paragraph("Priyantha De Costa", regularFont));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}", smallFont));

                document.Close();
                CloseWindow();


            }

            return filePath;
        }


        private async void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(email))
            {
                CustomMessageBox.Show("Please enter an email address.");
                return;
            }

            string pdfPath;
            try
            {
                pdfPath = GenerateQuotationPdf();

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Failed to generate PDF: {ex.Message}");
                return;
            }

            string body = $"Dear Customer,<br><br>Please find attached the quotation #{currentQuoNumber}.<br><br>Thank you.";

            try
            {
                var mail = new MailServiceFinance();
                bool sent = await mail.SendEmailWithAttachmentAsync(
                    email,
                    null,  // CC emails
                    $"Quotation #{currentQuoNumber}",
                    body,
                    pdfPath
                );

                if (sent)
                    CustomMessageBox.Show("Email sent successfully!");


                else
                    CustomMessageBox.Show("Failed to send email.");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error sending email: {ex.Message}");
            }

        }

        private void CloseWindow()
            {
            this.Close();
        }

    }



    public class QuoNo
    {
        public string QuoNumber { get; set; }
    }

    public class Quotation
    {
        public string QuotationNumber { get; set; }
        public Customerinfo Customer { get; set; }
        public List<QuotationItem> Items { get; set; }
        public decimal Total { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly ValidDate { get; set; }
        public int ValidDays { get; internal set; }
    }
}
