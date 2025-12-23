using PDC_System.Helpers;
using System.Windows;

namespace PDC_System
{
    public partial class CustomMessageBox : Window
    {
        private MessageBoxResult _result = MessageBoxResult.None;

        private CustomMessageBox(string message, string title,
            MessageBoxButton buttons, MessageBoxImage icon)
        {
            InitializeComponent();

            TxtTitle.Text = title;
            TxtMessage.Text = message;
            ThemeManager.ApplyTheme(this); // Apply initial theme
            SetIcon(icon);
            CreateButtons(buttons);
        }

        // 🔁 CustomMessageBox compatible Show()
        public static MessageBoxResult Show(
            string message,
            string title = "",
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            Window owner = null)
        {
            var box = new CustomMessageBox(message, title, buttons, icon);

            if (owner != null)
                box.Owner = owner;

            box.ShowDialog();
            return box._result;
        }

        private void SetIcon(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Warning:
                    TxtIcon.Text = "⚠";
                    TxtIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    break;

                case MessageBoxImage.Error:
                    TxtIcon.Text = "❌";
                    TxtIcon.Foreground = System.Windows.Media.Brushes.Red;
                    break;

                case MessageBoxImage.Information:
                    TxtIcon.Text = "ℹ";
                    TxtIcon.Foreground = System.Windows.Media.Brushes.DeepSkyBlue;
                    break;

                case MessageBoxImage.Question:
                    TxtIcon.Text = "❓";
                    TxtIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    break;

                default:
                    TxtIcon.Text = "";
                    break;
            }
        }

        private void CreateButtons(MessageBoxButton buttons)
        {
            void Add(string text, MessageBoxResult result)
            {
                var btn = new System.Windows.Controls.Button
                {
                    Content = text,
                    Width = 90,
                    Margin = new Thickness(6)
                };

                // ✅ DynamicResource correctly applied
                btn.SetResourceReference(
                    System.Windows.Controls.Button.StyleProperty,
                    "ActionButton"
                );

                btn.Click += (_, __) =>
                {
                    _result = result;
                    Close();
                };

                ButtonPanel.Children.Add(btn);
            }

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    Add("OK", MessageBoxResult.OK);
                    break;

                case MessageBoxButton.OKCancel:
                    Add("OK", MessageBoxResult.OK);
                    Add("Cancel", MessageBoxResult.Cancel);
                    break;

                case MessageBoxButton.YesNo:
                    Add("Yes", MessageBoxResult.Yes);
                    Add("No", MessageBoxResult.No);
                    break;

                case MessageBoxButton.YesNoCancel:
                    Add("Yes", MessageBoxResult.Yes);
                    Add("No", MessageBoxResult.No);
                    Add("Cancel", MessageBoxResult.Cancel);
                    break;
            }
        }

    }
}
