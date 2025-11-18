using Newtonsoft.Json;
using PDC_System.Customer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;

namespace PDC_System.Outsourcing
{
    /// <summary>
    /// Interaction logic for OutsourcingWindow.xaml
    /// </summary>
    public partial class OutsourcingWindow : UserControl
    {
        private List<Outsourcinginfo> outsourcings = new List<Outsourcinginfo>();
        public List<Outsourcinginfo> PagedData { get; set; } = new List<Outsourcinginfo>();
        private int currentPage = 1;
        private int pageSize = 10; // 10 rows per page
        private readonly string jsonFilePath2;
        private readonly string saversFolder;

        public OutsourcingWindow()
        {
            InitializeComponent();

            saversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
            if (!Directory.Exists(saversFolder))
                Directory.CreateDirectory(saversFolder);

            jsonFilePath2 = Path.Combine(saversFolder, "Outsource.json");

            LoadData();      // Load from JSON
            LoadPage();      // Load first page
        }

        private void LoadData()
        {
            if (File.Exists(jsonFilePath2))
            {
                var json = File.ReadAllText(jsonFilePath2);
                if (!string.IsNullOrWhiteSpace(json))
                    outsourcings = JsonConvert.DeserializeObject<List<Outsourcinginfo>>(json) ?? new List<Outsourcinginfo>();
            }

            var types = outsourcings.Select(o => o.Type).Distinct().ToList();
            types.Insert(0, "All"); // show all types option
            TypeComboBox.ItemsSource = types;
            TypeComboBox.SelectedIndex = 0;

            LoadPage();
        }


        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndLoadPage();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndLoadPage();
        }

        private void FilterAndLoadPage()
        {
            string selectedType = TypeComboBox.SelectedItem?.ToString() ?? "All";
            string searchText = SearchTextBox.Text.ToLower();

            var filtered = outsourcings.Where(o =>
                (selectedType == "All" || o.Type == selectedType) &&
                o.Name.ToLower().Contains(searchText)
            ).ToList();

            // paging
            PagedData = filtered
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            OutsourcingDataGrid.ItemsSource = PagedData;
        }


        private void LoadPage()
        {
            PagedData = outsourcings
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            OutsourcingDataGrid.ItemsSource = PagedData;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < outsourcings.Count)
            {
                currentPage++;
                LoadPage();
            }
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPage();
            }
        }

        private void Outsourcing_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new OutsourcingAddWindow();
            if (addWindow.ShowDialog() == true)
            {
                outsourcings.Add(addWindow.Outsourcing);
                File.WriteAllText(jsonFilePath2, JsonConvert.SerializeObject(outsourcings, Formatting.Indented));
                LoadPage(); // reload page after adding
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                Outsourcinginfo selectedRow = btn.DataContext as Outsourcinginfo;
                if (selectedRow != null)
                {
                    var editWindow = new OutsourcingEditWindow(selectedRow);
                    if (editWindow.ShowDialog() == true)
                    {
                        // Save to JSON after editing
                        File.WriteAllText(jsonFilePath2, JsonConvert.SerializeObject(outsourcings, Formatting.Indented));
                        LoadPage(); // Refresh DataGrid
                    }
                }
            }
        }

        private void OutsourcingDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Find parent ScrollViewer
            ScrollViewer scrollViewer = FindParent<ScrollViewer>((DependencyObject)sender);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        // Helper function to find parent ScrollViewer
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T typedParent) return typedParent;
            return FindParent<T>(parent);
        }



    }
}