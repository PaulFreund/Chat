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

using Backend.Common;
using Backend.Data;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Flyout
{
    public sealed partial class AccountListEdit : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }

        private Account CurrentAccount = null;
        private Flyout flyoutSelf = null;
        private bool loaded = false;

        public AccountListEdit(Flyout self)
        {
            this.InitializeComponent();
            flyoutSelf = self;
            this.DataContext = Frontend.Accounts;

            foreach (var account in Frontend.Accounts)
                account.forceDisabled = false;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
        }

        // Switch toggled
        private async void OnStateChanged(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                await Frontend.RunAsync( () =>
                {
                    Frontend.Events.AccountListChanged();
                    Frontend.Backend.UpdateConnections();
                });
            }
        }

        // Edit Acccount
        private void OnAccountEdit(object sender, RoutedEventArgs e)
        {
            var uielement = sender as Button;
            if (uielement != null)
            {
                CurrentAccount = null;
                CurrentAccount = uielement.Tag as Account;
                if (CurrentAccount != null)
                    new Flyout(FlyoutType.AccountEdit, CurrentAccount, flyoutSelf);
            }
        }

        // Remove Account
        private async void OnAccountRemove(object sender, RoutedEventArgs e)
        {
            var uielement = sender as Button;
            if( uielement != null )
            {
                CurrentAccount = null;
                CurrentAccount = uielement.Tag as Account;
                if (CurrentAccount != null)
                {
                    var dialog = new MessageDialog(Helper.Translate("DeleteAccountMessage"));

                    dialog.Commands.Add(new UICommand(Helper.Translate("MessageBoxYes"), new UICommandInvokedHandler(this.AccountRemoveDialogHandler), "Yes"));
                    dialog.Commands.Add(new UICommand(Helper.Translate("MessageBoxNo"), new UICommandInvokedHandler(this.AccountRemoveDialogHandler), "No"));
                    await dialog.ShowAsync();
                }
            }
        }

        private async void AccountRemoveDialogHandler(IUICommand command)
        {
            if (CurrentAccount != null && (string)command.Id == "Yes")
            {
                await Frontend.RunAsync(() =>
                {
                    CurrentAccount.DeletePassword();
                    Frontend.Accounts.Remove(CurrentAccount);
                    Frontend.Backend.UpdateConnections();
                    CurrentAccount = null;
                });
            }
        }

        private void OnAddXMPP(object sender, RoutedEventArgs e)
        {
            if (!Frontend.Accounts.IsFull)
                new Flyout(FlyoutType.AccountEdit, null, flyoutSelf);
        }

        private void OnAddGTalk(object sender, RoutedEventArgs e)
        {
            if (!Frontend.Accounts.IsFull)
            {
                var template = new AccountTemplate();
                template.Jid = "@gmail.com";
                template.Host = "talk.google.com";
                template.Port = 5223;
                template.SSL = true;
                template.OldSSL = true;
                template.Plain = true;
                template.MD5 = true;
                template.SCRAM = true;
                new Flyout(FlyoutType.AccountEdit, template, flyoutSelf);
            }
        }

        private async void OnAddFacebook(object sender, RoutedEventArgs e)
        {
            if (!Frontend.Accounts.IsFull)
            {
                var dialog = new MessageDialog(Helper.Translate("AddFacebookAccountMessage"));
                dialog.Commands.Add(new UICommand(Helper.Translate("MessageBoxOk"), new UICommandInvokedHandler(this.FacebookAccountDialogHandler), "Ok"));
                await dialog.ShowAsync();
            }
        }

        private void FacebookAccountDialogHandler(IUICommand command)
        {
            var template = new AccountTemplate();
            template.Jid = "@chat.facebook.com";
            template.Host = "chat.facebook.com";
            template.Port = 5222;
            template.SSL = true;
            template.OldSSL = false;
            template.Plain = false;
            template.MD5 = true;
            template.SCRAM = true;
            new Flyout(FlyoutType.AccountEdit, template, flyoutSelf);
        }
    }

}
