using iText.Layout.Borders;
using Newtonsoft.Json;
using PDC_System.Orders;
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

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Oders.xaml
    /// </summary>
    public partial class Oders : UserControl
    {
        private List<Order> orders;
        public Oders()
        {
            InitializeComponent();
            LoadOrders();
            OrdersDataGrid.ItemsSource = orders;
        }
    

            private void LoadOrders()
        {
            if (File.Exists("orders.json"))
            {
                string json = File.ReadAllText("orders.json");
                orders = JsonConvert.DeserializeObject<List<Order>>(json);
            }
            else
            {
                orders = new List<Order>();
            }
        }

        private void SaveOrders()
        {
            string json = JsonConvert.SerializeObject(orders);
            File.WriteAllText("orders.json", json);
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrderWindow addOrderWindow = new AddOrderWindow();
            if (addOrderWindow.ShowDialog() == true)
            {
                orders.Add(addOrderWindow.NewOrder);
                OrdersDataGrid.Items.Refresh();
                SaveOrders();
            }
        }

        private void MarkAsFinished_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.DataContext as Order;

            if (order != null)
            {
                var result = MessageBox.Show("Are you sure you want to mark this order as finished?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    order.IsFinished = true;
                    OrdersDataGrid.Items.Refresh();
                    SaveOrders();
                }
            }
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.DataContext as Order;

            if (order != null)
            {
                var result = MessageBox.Show("Are you sure you want to revert this order?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    order.IsFinished = false;
                    OrdersDataGrid.Items.Refresh();
                    SaveOrders();
                }
            }
        }

        private void DeleteOrders_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = OrdersDataGrid.SelectedItem as Order; // Ensure correct type
            if (selectedOrder != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogOders(); // Assuming this dialog exists
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    orders.Remove(selectedOrder);
                    OrdersDataGrid.Items.Refresh();
                    File.WriteAllText("orders.json", JsonConvert.SerializeObject(orders)); // Save to correct file
                }
            }
        }


    }
}
