using System.Windows;

namespace PDC_System
{
    public partial class ConfirmationDialogCustomer : Window
    {
        public bool IsConfirmed { get; private set; }

        public ConfirmationDialogCustomer()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
