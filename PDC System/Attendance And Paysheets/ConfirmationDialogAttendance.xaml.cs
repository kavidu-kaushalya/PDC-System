using System.Windows;

namespace PDC_System
{
    public partial class ConfirmationDialogAttendance : Window
    {
        public bool IsConfirmed { get; private set; }

        public ConfirmationDialogAttendance()
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
