using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDC_System.Paysheets
{
    /// <summary>
    /// Interaction logic for PaysheetWindow.xaml
    /// </summary>
    public partial class PaysheetWindow : UserControl
    {
        public PaysheetWindow()
        {
            InitializeComponent();
            LoadPaysheets();
        }

        private void CreatePaysheet_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddPaysheetWindow();
            window.ShowDialog();
            PaysheetGrid.Items.Refresh();
        }

        private string paysheetFile = "Savers/Paysheets.json";

        private List<Paysheet> paysheets = new List<Paysheet>();

        private void LoadPaysheets()
        {
            if (File.Exists(paysheetFile))
            {
                string json = File.ReadAllText(paysheetFile);
                paysheets = JsonConvert.DeserializeObject<List<Paysheet>>(json) ?? new List<Paysheet>();
            }
            PaysheetGrid.ItemsSource = null;
            PaysheetGrid.ItemsSource = paysheets;
        }


        private void DeletePaysheet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var paysheet = btn.Tag as Paysheet;
            if (paysheet != null)
            {
                paysheets.Remove(paysheet);
                File.WriteAllText(paysheetFile, JsonConvert.SerializeObject(paysheets, Formatting.Indented));
                LoadPaysheets();
                PaysheetGrid.Items.Refresh();
            }
        }

        private void EditPaysheet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var paysheet = btn.Tag as Paysheet;

            if (paysheet != null)
            {
                var window = new AddPaysheetWindow();

                // Load all paysheet data
                window.LoadPaysheetData(paysheet);

                window.ShowDialog();

                // Reload grid after editing
                LoadPaysheets();
                PaysheetGrid.Items.Refresh();
            }
        }


        private void OpenPDF_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string path = btn?.Tag?.ToString();

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("No PDF found for this paysheet.");
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show("PDF file does not exist in saved location!");
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }






    }




}
