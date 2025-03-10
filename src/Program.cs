using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;


namespace WinStartMenuReplacement
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string Winlogon = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
            if (args.Length == 0)
            {
                Inf.Show = true;
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Choose an image for the Start menu button"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK) {Start_Changing(openFileDialog.FileName); Inf.Show_log();}
                else Console.WriteLine("No file selected.");

            }
            else if (args[0].ToLower() == "makeauto")
            {
                string image = args[1], app = Application.ExecutablePath;
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                using (RegistryKey key = hklm.OpenSubKey(Winlogon, true))
                {
                    key.SetValue("Shell", $"{key.GetValue("Shell")}, \"{app}\" \"{image}\"", RegistryValueKind.String);
                    key.Close();
                }
            }
            else if (args[0].ToLower() == "rmauto")
            {
                string image = args[1], app = Application.ExecutablePath;
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                using (RegistryKey key = hklm.OpenSubKey(Winlogon, true))
                {
                    key.SetValue("Shell", $"{key.GetValue("Shell").ToString().Replace($", \"{app}\" \"{image}\"", "")}", RegistryValueKind.String);
                    key.Close();
                }
            }
            else if (File.Exists(args[0])) Start_Changing(args[0]); 
            else MessageBox.Show("File not found!");
        }
        static void Start_Changing(string path)
        {
            StartButtonModifier modifier = new StartButtonModifier();

            switch (Path.GetExtension(path).ToLower())
            {
                case ".png": break;
                case ".jpg": break;
                case ".jpeg": break;
                case ".bmp": break;
                default: Application.Exit(); break;
            }

            for (int i = 1; i <= 7; i++) { 
                byte[] bitmapBytes = modifier.ConvertToBitmapBytes(new Bitmap(path), 16, 16); 
                modifier.ModifyStyle("TaskbarPearl", i, 0, 0, bitmapBytes);
            }
        }

    }
}
