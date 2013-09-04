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

using Backend;
using Backend.Common;
using Backend.Data;
using Chat.UI.Flyout;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Views
{
    public sealed partial class Main : Chat.Common.LayoutAwarePage
    {
        private App Frontend { get { return (App)App.Current; } }
        private BackendInterface Backend { get { return Frontend.Backend; } }

        // Main view elements

        private Chat.UI.Controls.Roster Roster { get { return (Chat.UI.Controls.Roster)this.RosterControl; } }
        private Chat.UI.Controls.Conversation Conversation { get { return (Chat.UI.Controls.Conversation)this.ConversationControl; } }

        private Chat.UI.Controls.Status Status { get { return (Chat.UI.Controls.Status)this.StatusControl; } }
        private Chat.UI.Controls.ConversationHeader ConversationHeader { get { return (Chat.UI.Controls.ConversationHeader)this.ConversationHeaderControl; } }

        // Flyout 

        private Flyout.Flyout CurrentFlyout = null;

        private Contact SelectedContact
        {
            get { return this.DataContext as Contact; }
            set { this.DataContext = value; }
        }

        private bool _isCtrlKeyPressed = false;
        private bool _isAltKeyPressed = false;

        private SettingsCommand aboutCommand = null;
        private SettingsCommand privacyCommand = null;
        private SettingsCommand settingsCommand = null;
        private SettingsCommand themeCommand = null;
        private SettingsCommand accountsCommand = null;

        public Main()
        {
            try
            {
                this.InitializeComponent();

                // Assign data to IsLoading ( Progress Indicator )
                IsLoading.DataContext = Frontend.Status;

                // Assign data to Notificationsbutton
                NotificationButton.DataContext = Frontend.Notifications;

                // Assign settings for inverting interface
                MainGrid.DataContext = Frontend.Settings;

                // Add Charms Handler
                SettingsPane.GetForCurrentView().CommandsRequested += CommandsRequest;

                // Size changed and roster
                SizeChanged += WindowSizeChanged;
                Frontend.Events.OnRosterContactSelected += Events_OnRosterItemSelected;
                Frontend.Events.OnSubscriptionContactSelected += Events_OnSubscriptionContactSelected;

                // Background access
                Frontend.OnRequestBackgroundAccess += Frontend_OnRequestBackgroundAccess;

                Frontend.Events.OnMessageReceived += Events_OnMessageReceived;
                Frontend.Events.OnAccountListChanged += Events_OnAccountListChange;
                Frontend.Notifications.NotificationList.CollectionChanged += NotificationList_CollectionChanged;

                RecreateLayout();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

        }

        void Events_OnAccountListChange(object sender, EventArgs e)
        {
            RecreateLayout();
        }

        void NotificationList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnUserRelevantEvent();
        }

        void Events_OnMessageReceived(object sender, EventArgs e)
        {
            OnUserRelevantEvent();
        }

        public async void OnUserRelevantEvent()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    bool notifications = false;
                    foreach (var account in Frontend.Accounts)
                    {
                        if (account.Roster.UnreadNotificationCount > 0)
                            notifications = true;
                    }

                    if (Frontend.Notifications.NotificationList.Count > 0)
                        notifications = true;

                    ConversationHeaderControl.Notify = notifications;
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

        }

        private async void Events_OnSubscriptionContactSelected(object sender, Frontend.ContactSelectedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.Subscription, e.Contact);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void Events_OnRosterItemSelected(object sender, Frontend.ContactSelectedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    SelectedContact = e.Contact;
                    RecreateLayout();
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }


        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecreateLayout();
        }

        public async void RecreateLayout()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (Frontend.Accounts == null)
                        return;

                    OnUserRelevantEvent();

                    // No accounts overlay
                    if (Frontend.Accounts.Count <= 0)
                        NoAccountsOverlay.Visibility = Visibility.Visible;
                    else
                        NoAccountsOverlay.Visibility = Visibility.Collapsed;

                    // Snapping
                    if (ApplicationView.Value == ApplicationViewState.Snapped) // Snapped
                    {
                        if (SelectedContact != null) // Contact Selected
                        {
                            this.LeftGridControl.Visibility = Visibility.Collapsed;
                            this.RightGridControl.Visibility = Visibility.Visible;
                            this.RosterControl.Visibility = Visibility.Visible;
                            this.ConversationControl.Visibility = Visibility.Visible;
                            this.StatusControl.Visibility = Visibility.Visible;
                            this.ConversationHeaderControl.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.LeftGridControl.Visibility = Visibility.Visible;
                            this.RightGridControl.Visibility = Visibility.Collapsed;
                            this.RosterControl.Visibility = Visibility.Visible;
                            this.ConversationControl.Visibility = Visibility.Visible;
                            this.StatusControl.Visibility = Visibility.Visible;
                            this.ConversationHeaderControl.Visibility = Visibility.Visible;
                        }
                    }
                    else // Not snapped
                    {
                        this.LeftGridControl.Visibility = Visibility.Visible;
                        this.RightGridControl.Visibility = Visibility.Visible;
                        this.RosterControl.Visibility = Visibility.Visible;
                        this.ConversationControl.Visibility = Visibility.Visible;
                        this.StatusControl.Visibility = Visibility.Visible;
                        this.ConversationHeaderControl.Visibility = Visibility.Collapsed;
                    }

                    // Appbar
                    if (SelectedContact != null)
                    {
                        RemoveContact.IsEnabled = true;
                        ContactInfo.IsEnabled = true;
                    }
                    else
                    {
                        RemoveContact.IsEnabled = false;
                        ContactInfo.IsEnabled = false;
                    }

                    if (Frontend.Accounts.Enabled.Count() > 0)
                        AddContact.IsEnabled = true;
                    else
                        AddContact.IsEnabled = false;
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void Frontend_OnRequestBackgroundAccess(object sender, EventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (Backend != null)
                        Backend.RequestBackgroundAccess();
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }            
        }

        private void CommandsRequest(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            try
            {
                if (aboutCommand == null)
                    aboutCommand = new SettingsCommand("about", Helper.Translate("FlyoutTypeAbout"), (x) => OnAbout(true));

                if (privacyCommand == null)
                    privacyCommand = new SettingsCommand("privacy", Helper.Translate("PrivacyPolicyTitle"), (x) => OnPrivacy());

                if (settingsCommand == null)
                    settingsCommand = new SettingsCommand("settings", Helper.Translate("FlyoutTypeSettingsEdit"), (x) => OnSettings(true));

                if (themeCommand == null)
                    themeCommand = new SettingsCommand("theme", Helper.Translate("FlyoutTypeThemeEdit"), (x) => OnTheme(true));

                if (accountsCommand == null)
                    accountsCommand = new SettingsCommand("accounts", Helper.Translate("FlyoutTypeAccountListEdit"), (x) => OnAccounts(true));

                args.Request.ApplicationCommands.Clear();
                args.Request.ApplicationCommands.Add(privacyCommand);
                args.Request.ApplicationCommands.Add(aboutCommand);
                args.Request.ApplicationCommands.Add(settingsCommand);
                args.Request.ApplicationCommands.Add(themeCommand);
                args.Request.ApplicationCommands.Add(accountsCommand);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private bool CleanPaneState()
        {
            try
            {
                if (CurrentFlyout != null)
                    CurrentFlyout.Hide();

                AppBar.IsOpen = false;

                if (ApplicationView.Value == ApplicationViewState.Snapped)
                {
                    ApplicationView.TryUnsnap();
                    return false;
                }

                return true;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return false;
        }

        //----------------------------------------------------


        private async void OnAddContact(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.AddContact);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnRemoveContact(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (SelectedContact != null)
                    {
                        if (CleanPaneState())
                            CurrentFlyout = new Flyout.Flyout(FlyoutType.RemoveContact, SelectedContact);
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnContactInfo(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (SelectedContact != null)
                    {
                        if (CleanPaneState())
                            CurrentFlyout = new Flyout.Flyout(FlyoutType.EditContact, SelectedContact);
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnAccountsButton(object sender, RoutedEventArgs e)
        {
            OnAccounts(false);
        }

        private void OnSettingsButton(object sender, RoutedEventArgs e)
        {
            OnSettings(false);
        }

        //----------------------------------------------------

        private async void OnAbout(bool returnCharm)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.About, null, null, returnCharm);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnPrivacy()
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(Helper.Translate("PrivacyLink")));
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnSettings(bool returnCharm)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.SettingsEdit, null, null, returnCharm);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnTheme(bool returnCharm)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.ThemeEdit, null, null, returnCharm);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnAccounts(bool returnCharm)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.AccountListEdit, null, null, returnCharm);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnStatusEdit(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.StatusEdit);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OpenNotifications(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CleanPaneState())
                        CurrentFlyout = new Flyout.Flyout(FlyoutType.Notifications);
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }



        private void MainGrid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:    _isCtrlKeyPressed = true;   break;
                case VirtualKey.Menu:       _isAltKeyPressed = true;    break;

                case VirtualKey.Back:
                    if( _isCtrlKeyPressed && ConversationHeaderControl.Visibility == Visibility.Visible )
                        Frontend.Events.DeselectContact();
                    break;

                default: return;
            }
        }

        private void MainGrid_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch(e.Key)
            {
                case VirtualKey.Control:    _isCtrlKeyPressed = false;   break;
                case VirtualKey.Menu:       _isAltKeyPressed = false;    break;
                default: return;
            }
        }


    }
}
