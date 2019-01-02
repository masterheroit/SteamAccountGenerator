using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using Newtonsoft.Json;

namespace SteamAccountGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        
        private static readonly BrowserWindow W = new BrowserWindow();
        private static CaptchaViewer C;
        public MainWindow()
        {
            InitializeComponent();
            W.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            W.ShouldClose = true;
            W.Close();
            C?.Close();
        }

        private void SourceClicked(object Sender, RoutedEventArgs E)
        {
            Process.Start("https://github.com/SilentHammerHUN/SteamAccountGenerator");
        }

        private static readonly Random R = new Random();
        private static string GenUsername() => new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 16).Select(S => S[R.Next(S.Length)]).ToArray());
        private static string GenPassword() => new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!%@#<>&,;?.:-_$+=", 32).Select(S => S[R.Next(S.Length)]).ToArray());

        private bool DoneMsgBox = false;

        private async void MsgBox(string Text)
        {
            DoneMsgBox = false;
            await this.ShowChildWindowAsync(
                new ChildWindow
                {
                    Title = Text,
                    EnableDropShadow = true,
                    OverlayBrush = new SolidColorBrush(Colors.Gray) {Opacity = 0.8},
                    GlowBrush = new SolidColorBrush(Colors.Cyan),
                    ShowCloseButton = true
                });
            DoneMsgBox = true;
        }

        private string[] Account = {"", ""};
        private bool Done;
        private async void GenerateClicked(object Sender, RoutedEventArgs E)
        {
            Username.Text = string.Empty;
            Password.Text = string.Empty;
            ((Button)Sender).Visibility = Visibility.Hidden;
            Cog.Visibility = Visibility.Visible;
            Cog.Spin = true;
            await Task.Delay(200);

            //Do actual stuff

            GenerateAccount();

            while (!Done)
                await Task.Delay(10);

            Username.Text = Account[0];
            Password.Text = Account[1];

            //End

            Cog.Spin = false;
            Cog.Visibility = Visibility.Hidden;
            ((Button)Sender).Visibility = Visibility.Visible;
        }

        #pragma warning disable 649
        #pragma warning disable 169
        private struct AjaxResponse
        {
            private readonly string Junk1;
            [JsonProperty("sessionid")]
            public string CreationId;
            [JsonProperty("gid")]
            public string CaptchaId;
            private readonly string Junk2;
        }
        #pragma warning restore 169
        #pragma warning restore 649

        private string CaptchaId;
        private string Solution = string.Empty;
        private async void GetCaptchaId()
        {
            Solution = string.Empty;
            var Client = new HttpClient();
            var Values = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "count", "1" }
            });

            CaptchaId = JsonConvert.DeserializeObject<AjaxResponse>(await Client.PostAsync("https://store.steampowered.com/join/refreshcaptcha/", Values).Result.Content.ReadAsStringAsync()).CaptchaId;
            C = new CaptchaViewer("https://store.steampowered.com/login/rendercaptcha?gid=" + CaptchaId);
            C.Show();
            while (C.Instance.Solution == string.Empty) await Task.Delay(10);
            Solution = C.Instance.Solution;

        }

        private async void GenerateAccount()
        {

            Done = false;
            var Client = new HttpClient();
            try
            {
                await Client.GetAsync("https://google.com");
            } catch { MsgBox("You are not connected to the internet!"); goto end; }

            var EmailAddress = Email.Text;
            if (EmailAddress == string.Empty)
                goto end;

            var ToReturn = new [] { GenUsername(), GenPassword() };

            GetCaptchaId();
            while (Solution == string.Empty) await Task.Delay(10);

            var Values = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "captchagid", CaptchaId },
                { "captcha_text", Solution },
                { "email", EmailAddress },
                { "count", "2" }
            });

            await Client.PostAsync("https://store.steampowered.com/join/verifycaptcha/", Values);
            
            GetCaptchaId();
            while (Solution == string.Empty) await Task.Delay(10);

            Values = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "captchagid", CaptchaId },
                { "captcha_text", Solution },
                { "email", EmailAddress }
            });

            var Response = await Client.PostAsync("https://store.steampowered.com/join/ajaxverifyemail", Values).Result.Content.ReadAsStringAsync();

            var SessionId = JsonConvert.DeserializeObject<AjaxResponse>(Response).CreationId;

            MsgBox("Verify the email, and after you have done that, click on the 'X'");
            while (!DoneMsgBox) await Task.Delay(10);
            //Just to be safe...
            await Task.Delay(3000);

            Values = new FormUrlEncodedContent(new Dictionary<string, string>
            {

                { "accountname", ToReturn[0] },
                { "password", ToReturn[1] },
                { "count", "3" },
                { "creation_sessionid", SessionId }
            });

            await Client.PostAsync("https://store.steampowered.com/join/createaccount/", Values);

            Account = ToReturn;

            end: Done = true;

        }

    }
}
