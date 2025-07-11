using System.Windows;

namespace PDC_System
{
    public partial class ConfirmationDialogEmployee : Window
    {
        public bool IsConfirmed { get; private set; }

        public ConfirmationDialogEmployee()
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
