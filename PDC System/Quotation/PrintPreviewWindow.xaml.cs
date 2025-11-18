using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace PDC_System
{
    public partial class PrintPreviewWindow : Window
    {
        private DrawingVisual _visual;

        public PrintPreviewWindow(DrawingVisual visual)
        {
            InitializeComponent();
            _visual = visual;

            LoadPrinters();
            ShowPreview();
        }

        private void LoadPrinters()
        {
            var server = new LocalPrintServer();
            var printers = server.GetPrintQueues(new[]
            {
                EnumeratedPrintQueueTypes.Local,
                EnumeratedPrintQueueTypes.Connections
            });

            PrinterCombo.ItemsSource = printers.ToList();
            PrinterCombo.DisplayMemberPath = "Name";
            PrinterCombo.SelectedIndex = 0;
        }

        private void ShowPreview()
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(
                900, 600, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(_visual);
            PreviewImage.Source = bmp;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrinterCombo.SelectedItem is PrintQueue printer)
            {
                double width = double.Parse(PaperWidthBox.Text);
                double height = double.Parse(PaperHeightBox.Text);
                string unit = (UnitCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

                double widthPx = ConvertToPx(width, unit);
                double heightPx = ConvertToPx(height, unit);

                PrintDialog pd = new PrintDialog
                {
                    PrintQueue = printer
                };

                pd.PrintTicket.PageMediaSize = new PageMediaSize(widthPx, heightPx);
                pd.PrintVisual(_visual, "Invoice Print");

                MessageBox.Show("✅ Successfully sent to printer");
            }
        }

        private double ConvertToPx(double value, string unit)
        {
            return unit switch
            {
                "mm" => value * 3.78,
                "cm" => value * 37.8,
                "inch" => value * 96,
                _ => value,
            };
        }

        // ======== PRINTER PREFERENCES SUPPORT (Win32 API) ========

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int DocumentProperties(IntPtr hwnd, IntPtr hPrinter,
            string pDeviceName, IntPtr pDevModeOutput, IntPtr pDevModeInput, int fMode);

        const int DM_OUT_BUFFER = 0x2;
        const int DM_IN_BUFFER = 0x8;
        const int DM_IN_PROMPT = 0x4;

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PrinterCombo.SelectedItem is not PrintQueue printer)
            {
                MessageBox.Show("Please select a printer first.");
                return;
            }

            try
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

                // Open printer handle
                if (!OpenPrinter(printer.FullName, out IntPtr hPrinter, IntPtr.Zero))
                {
                    MessageBox.Show("Failed to open printer.");
                    return;
                }

                // Get DEVMODE size
                int size = DocumentProperties(hwnd, hPrinter, printer.FullName,
                    IntPtr.Zero, IntPtr.Zero, 0);

                if (size < 0) { ClosePrinter(hPrinter); return; }

                IntPtr devMode = Marshal.AllocHGlobal(size);

                // Load existing printer settings
                DocumentProperties(hwnd, hPrinter, printer.FullName,
                    devMode, IntPtr.Zero, DM_OUT_BUFFER);

                // Show Printer Preferences dialog
                int result = DocumentProperties(hwnd, hPrinter, printer.FullName,
                    devMode, devMode, DM_IN_BUFFER | DM_OUT_BUFFER | DM_IN_PROMPT);

                if (result == 1) // OK pressed
                {
                    var validation = printer.MergeAndValidatePrintTicket(
                        printer.DefaultPrintTicket,
                        printer.UserPrintTicket);

                    // Apply printer setting changes
                    printer.UserPrintTicket = validation.ValidatedPrintTicket;
                }

                Marshal.FreeHGlobal(devMode);
                ClosePrinter(hPrinter);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Printer Preferences could not be opened.\n\n" + ex.Message);
            }
        }
    }
}
