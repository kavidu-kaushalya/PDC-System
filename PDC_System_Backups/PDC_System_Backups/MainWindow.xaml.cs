using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Forms; // Add reference to System.Windows.Forms
using MessageBox = System.Windows.MessageBox;

namespace PDC_System_Backups
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer backupTimer;
        private readonly string[] filePaths = { "employee.json", "attendance.json", "savedpdfs.json", "customers.json", "jobcards.json", "orders.json", "PaysheetData.json", "quoteSettings.json" }; // Source JSON files
        private readonly string backupDirectory = @"C:\Backups"; // Backup folder path
        private NotifyIcon trayIcon; // System Tray icon


        public MainWindow()
        {
            InitializeComponent();
            InitializeAutoBackup();
            InitializeTrayIcon();
        }

        private void InitializeAutoBackup()
        {
            backupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(60)
            };
            backupTimer.Tick += BackupTimer_Tick;
            backupTimer.Start();
        }

        private void BackupTimer_Tick(object sender, EventArgs e)
        {
            PerformBackup();
        }

        private void PerformBackup()
        {
            try
            {
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        string backupFilePath = Path.Combine(backupDirectory, fileName);

                        File.Copy(filePath, backupFilePath, overwrite: true);
                    }
                    else
                    {
                        MessageBox.Show($"File not found: {filePath}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Backup completed successfully.", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during backup: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManualBackupButton_Click(object sender, RoutedEventArgs e)
        {
            PerformBackup();
        }

        private void RestoreBackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string filePath in filePaths)
                {
                    string fileName = Path.GetFileName(filePath);
                    string backupFilePath = Path.Combine(backupDirectory, fileName);

                    if (File.Exists(backupFilePath))
                    {
                        File.Copy(backupFilePath, filePath, overwrite: true);
                    }
                    else
                    {
                        MessageBox.Show($"Backup file not found: {backupFilePath}", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Restore completed successfully.", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during restore: {ex.Message}", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTrayIcon()
        {
            // Create the system tray icon
            trayIcon = new NotifyIcon
            {
                Icon = new Icon("data-recovery.ico"), // Verbatim string for absolute path
                Visible = true,
                Text = "PDC System Backup"
            };
             

            // Create the context menu for the tray icon
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, OpenWindow);
            contextMenu.Items.Add("Exit", null, ExitApplication);
            trayIcon.ContextMenuStrip = contextMenu;

            // Hide the window on startup
            this.Visibility = Visibility.Hidden;
        }

        private void OpenWindow(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            System.Windows.Application.Current.Shutdown();

        }

        // Override the OnClosing event to prevent window closing, instead hide it to the tray
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Prevent window from closing
            this.Visibility = Visibility.Hidden;
        }


    }
}
