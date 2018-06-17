using System;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace FileSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
            folderDialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
            WinForms.DialogResult result = folderDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                SenderModel.FolderPath = folderDialog.SelectedPath;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SenderModel.IntervalInHour = Convert.ToInt32(timer.Text);
            SenderModel.Email = email.Text;
            JobScheduler.Start(SenderModel.FolderPath, SenderModel.Email, SenderModel.IntervalInHour).Wait();
            jobStatus.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            JobScheduler.Stop().Wait();
            jobStatus.Visibility = Visibility.Hidden;
        }
    }
}
