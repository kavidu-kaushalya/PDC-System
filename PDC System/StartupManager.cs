using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System;

namespace PDC_System.Helpers
{
    public static class StartupManager
    {
        // Automatic app name එක set කරනවා
        private static readonly string appName = Path.GetFileNameWithoutExtension(GetExecutablePath());

        private static string GetExecutablePath()
        {
            // Use Process.GetCurrentProcess().MainModule.FileName for reliable executable path
            try
            {
                using (var process = Process.GetCurrentProcess())
                {
                    return process.MainModule?.FileName ?? Assembly.GetExecutingAssembly().Location;
                }
            }
            catch
            {
                // Fallback to assembly location
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        public static void AddToStartup()
        {
            string exePath = GetExecutablePath();

            if (string.IsNullOrEmpty(exePath))
            {
                throw new InvalidOperationException("Could not determine executable path");
            }

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    key.SetValue(appName, $"\"{exePath}\""); // Wrap in quotes to handle spaces in path
                }
            }
        }

        public static void RemoveFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key != null)
                {
                    key.DeleteValue(appName, false);
                }
            }
        }

        public static bool IsInStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key?.GetValue(appName) != null;
            }
        }
    }
}