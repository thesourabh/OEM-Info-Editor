
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OEM_Info_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Singleton class to control access to single registry key
        /// </summary>
        private class OEMKey
        {
            
            private static RegistryKey key;
            private OEMKey() { }
            public static RegistryKey GetKey()
            {
                if (key == null)
                {
                    string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation";
                    RegistryKey baseKey = Registry.LocalMachine;
                    if (Environment.Is64BitOperatingSystem)
                    {
                        // Workaround for 64 bit systems since a 32 bit process will access the 32 bit registry by default
                        baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    }
                    RegistryKey key = baseKey.CreateSubKey(keyPath);
                    return key;
                }
                else
                {
                    return key;
                }
            }
            public static string GetStringFromKey(string name)
            {
                if (key == null)
                    key = GetKey();
                object val = key.GetValue(name);
                if (val == null)
                    return "";
                try
                {
                    string valString = (string)val;
                    return valString;
                }
                catch
                {
                    return "";
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            GetCurrentInformation();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select BMP file";
            ofd.Filter = "BMP|*.bmp";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (ofd.ShowDialog() == true)
            {
                string imageUrl = ofd.FileName;
                SetLogoValues(imageUrl);
            }
        }

        private void btnClearLogo_Click(object sender, RoutedEventArgs e)
        {
            ClearLogo();
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentInformation();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetUpdatedInformation();
        }

        private void ClearLogo()
        {
            SetLogoValues("");
        }

        private void ClearAll()
        {
            ClearLogo();
            txtManufacturer.Text = "";
            txtModel.Text = "";
            txtSupportHours.Text = "";
            txtSupportPhone.Text = "";
            txtSupportUrl.Text = "";
        }

        private void SetLogoValues(string imageUrl)
        {
            try {
                imgLogo.Source = new BitmapImage(new Uri(imageUrl));
                lblLogoName.Content = imageUrl.Substring(imageUrl.LastIndexOf('\\') + 1);
                imgLogo.Tag = imageUrl;
            } catch (Exception) {
                imageUrl = "";
                lblLogoName.Content = "";
                imgLogo.Tag = "";
                imgLogo.Source = null;
            }

        }

        private void GetCurrentInformation()
        {
            RegistryKey key = OEMKey.GetKey();
            txtManufacturer.Text = OEMKey.GetStringFromKey("Manufacturer");
            txtModel.Text = OEMKey.GetStringFromKey("Model");
            txtSupportHours.Text = OEMKey.GetStringFromKey("SupportHours");
            txtSupportPhone.Text = OEMKey.GetStringFromKey("SupportPhone");
            txtSupportUrl.Text = OEMKey.GetStringFromKey("SupportURL");
            SetLogoValues(OEMKey.GetStringFromKey("Logo"));
        }

        private void SetUpdatedInformation()
        {
            string manufacturer, model, supportHours, supportPhone, supportUrl, logo;
            manufacturer = txtManufacturer.Text;
            model = txtModel.Text;
            supportHours = txtSupportHours.Text;
            supportPhone = txtSupportPhone.Text;
            supportUrl = txtSupportUrl.Text;
            logo = (string)imgLogo.Tag;
            RegistryKey key = OEMKey.GetKey();
            key.SetValue("Manufacturer", manufacturer);
            key.SetValue("Model", model);
            key.SetValue("SupportHours", supportHours);
            key.SetValue("SupportPhone", supportPhone);
            key.SetValue("SupportURL", supportUrl);
            if (logo == null)
                key.SetValue("Logo", "");
            else
                key.SetValue("Logo", logo);
            string cplPath = System.IO.Path.Combine(Environment.SystemDirectory, "control.exe");
            System.Diagnostics.Process.Start(cplPath, "/name Microsoft.System");
        }
    }
}
