using System;
using System.IO;

namespace PDC_System.Helpers
{
    public static class StartupManager
    {
        private static readonly string appName =
            Path.GetFileNameWithoutExtension(Environment.ProcessPath!);

        private static string GetExecutablePath()
        {
            return Environment.ProcessPath!;
        }

        public static void AddToStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, appName + ".lnk");

            if (!File.Exists(shortcutPath))
            {
                string exePath = GetExecutablePath();

                Type shellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.Description = appName;
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Save();
            }
        }

        public static void RemoveFromStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, appName + ".lnk");

            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }

        public static bool IsInStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupFolder, appName + ".lnk");

            return File.Exists(shortcutPath);
        }
    }
}