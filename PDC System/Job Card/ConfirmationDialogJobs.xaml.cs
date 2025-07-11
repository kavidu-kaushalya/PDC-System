using System.Windows;

namespace PDC_System
{
    public partial class ConfirmationDialogJobs : Window
    {
        public bool IsConfirmed { get; private set; }

        public ConfirmationDialogJobs()
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
