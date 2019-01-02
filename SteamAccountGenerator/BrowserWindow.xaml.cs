using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using CefSharp.Wpf;

namespace SteamAccountGenerator
{
    /// <summary>
    /// Interaction logic for BrowserWindow.xaml
    /// </summary>
    public partial class BrowserWindow : Window
    {
        public BrowserWindow()
        {
            This = this;
            InitializeComponent();
        }

        public ChromiumWebBrowser Browser;
        private static BrowserWindow This;

        public static BrowserWindow Instance() => This;

        public bool ShouldClose = false;
        protected override void OnClosing(CancelEventArgs E)
        {
            base.OnClosing(E);
            if (!ShouldClose)
                E.Cancel = true;
        }
    }
}
