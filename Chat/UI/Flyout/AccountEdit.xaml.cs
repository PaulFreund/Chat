//###################################################################################################
/*
    Copyright (c) since 2012 - Paul Freund 
    
    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the "Software"), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:
    
    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE.
*/
//###################################################################################################

using Backend.Data;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Flyout
{
    public class AccountTemplate
    {
        public string Jid = "";
        public string Host = "";
        public int Port = 5222;
        public bool SSL = false;
        public bool OldSSL = false;
        public bool Plain = false;
        public bool MD5 = true;
        public bool SCRAM = true;
    }

    public sealed partial class AccountEdit : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }

        private Account CurrentAccount = null;
        private Flyout flyoutSelf = null;

        public AccountEdit(Flyout self, object data)
        {
            this.InitializeComponent();
            flyoutSelf = self;

            if (data is AccountTemplate)
            {
                var template = (AccountTemplate)data;

                this.Jid.Text = template.Jid;
                this.Host.Text = template.Host;
                this.Port.Text = template.Port.ToString();
                this.SSL.IsOn = template.SSL;
                this.OldSSL.IsOn = template.OldSSL;
                this.Plain.IsOn = template.Plain;
                this.MD5.IsOn = template.MD5;
                this.SCRAM.IsOn = template.SCRAM;
            }
            else if (data is Account)
            {
                CurrentAccount = (Account)data;

                if (CurrentAccount != null)
                {
                    foreach (ComboBoxItem color in ColorSelector.Items)
                    {
                        if ((string)color.Tag == CurrentAccount.color)
                            ColorSelector.SelectedItem = color;
                    }

                    this.Title.Text = CurrentAccount.title;
                    this.Jid.Text = CurrentAccount.jid;
                    this.Password.Password = CurrentAccount.password;
                    this.Host.Text = CurrentAccount.host;
                    this.Port.Text = CurrentAccount.port.ToString();
                    this.SSL.IsOn = CurrentAccount.usesssl;
                    this.OldSSL.IsOn = CurrentAccount.oldstylessl;
                    this.Plain.IsOn = CurrentAccount.authplain;
                    this.MD5.IsOn = CurrentAccount.authmd5;
                    this.SCRAM.IsOn = CurrentAccount.authscram;
                    this.ConnectedStandby.IsOn = CurrentAccount.requestConnectedStandby;
                }
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveAccount();
        }

        private async void SaveAccount()
        {
            await Frontend.RunAsync(() =>
            {
                var account = CurrentAccount;

                if (account == null)
                    account = Frontend.Accounts.New();

                if (account != null)
                {
                    if (
                        Title.Text.Length == 0 ||
                        Jid.Text.Length == 0 ||
                        Password.Password.Length == 0 ||
                        Host.Text.Length == 0 ||
                        ColorSelector.SelectedItem == null ||
                        Port.Text.Length == 0)
                    {
                        Warning.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        return;
                    }

                    account.title = this.Title.Text;

                    ComboBoxItem selectedColor = ColorSelector.SelectedItem as ComboBoxItem;
                    if (selectedColor != null)
                        account.color = (string)selectedColor.Tag;
                    else
                        account.color = "Gray";

                    account.jid = this.Jid.Text;
                    account.password = this.Password.Password;

                    account.host = this.Host.Text;
                    account.port = Convert.ToInt32(this.Port.Text);
                    account.usesssl = this.SSL.IsOn;
                    account.oldstylessl = this.OldSSL.IsOn;

                    account.authplain = this.Plain.IsOn;
                    account.authmd5 = this.MD5.IsOn;
                    account.authscram = this.SCRAM.IsOn;
                    account.authoauth2 = false;

                    account.requestConnectedStandby = this.ConnectedStandby.IsOn;
                    account.persistantState = AccountState.Enabled;

                    Frontend.Backend.UpdateConnections();
                }

                flyoutSelf.Hide();
            });
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            flyoutSelf.Hide();
        }

        private void OnExtendedSettings(object sender, RoutedEventArgs e)
        {
            ExtendedSettings.Visibility = Visibility.Visible;
            ExtendedSettingsButton.Visibility = Visibility.Collapsed;
        }
    }
}
