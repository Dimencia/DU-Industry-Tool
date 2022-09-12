using System;
using System.Windows.Forms;
using Bluegrams.Application;
using Path = System.IO.Path;

namespace DU_Industry_Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            PortableJsonSettingsProvider.SettingsFileName = "DU-Industry-Tool.usersettings.json";
            PortableJsonSettingsProvider.SettingsDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            PortableJsonSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(new IndustryManager()));
        }
    }
}
