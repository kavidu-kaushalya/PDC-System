using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace PDC_System.Helpers
{
    public static class StartupManager
    {
        // Automatic app name එක set කරනවා
        private static readonly string appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);

        public static void AddToStartup()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.SetValue(appName, exePath);
            }
        }

        public static void RemoveFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.DeleteValue(appName, false);
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
