using System.Windows;

namespace AutoLaunchApp.view
{
    /// <summary>
    /// Logique d'interaction pour Error.xaml
    /// </summary>
    public partial class Error : Window
    {
        public Error(string _errorText)
        {
            InitializeComponent();
            ErrorText.Text = _errorText;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
