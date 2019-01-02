using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteamAccountGenerator
{
    /// <summary>
    /// Interaction logic for CaptchaViewer.xaml
    /// </summary>
    public partial class CaptchaViewer : Window
    {
        public CaptchaViewer(string Url)
        {
            InitializeComponent();
            Image.Source = new BitmapImage(new Uri(Url));
            Instance = this;
        }

        public CaptchaViewer Instance;
        public string Solution = string.Empty;

        private void CaptchaSolutionClicked(object Sender, RoutedEventArgs E)
        {

            Solution = Captcha.Text;
            Close();

        }
    }
}
