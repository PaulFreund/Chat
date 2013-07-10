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
using Chat.Frontend;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;

namespace Chat.UI.Controls
{
    public sealed partial class Conversation : UserControl
    {
        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.RegisterAttached("MessageText", typeof(Block),
            typeof(Conversation), new PropertyMetadata(new Paragraph(), OnMessageTextChanged));

        public static Block GetMessageText(DependencyObject obj)
        {
            return (Block)obj.GetValue(MessageTextProperty);
        }

        public static void SetMessageText(DependencyObject obj, Block value)
        {
            obj.SetValue(MessageTextProperty, value);
        }

        private static void OnMessageTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RichTextBlock;
            if (control != null)
            {
                control.Blocks.Clear();
                var value = e.NewValue as Windows.UI.Xaml.Documents.Block;

                if (value != null)
                {
                    control.Blocks.Add(value);
                }
            }
        }

        private App Frontend { get { return (App)App.Current; } }

        private Backend.Data.ConversationItem CurrentItem { get; set; }

        private Backend.Data.Conversation CurrentConversation 
        { 
            get 
            {
                try
                {
                    var conversation = this.DataContext as Backend.Data.Conversation;
                    if (conversation != null && conversation.Self != null && conversation.Other != null)
                        return conversation;
                }
                catch (Exception uiEx) { Frontend.UIError(uiEx); }

                return null;
            } 
            set 
            {
                try
                {
                    if (value != null && value.Other != null && value.Self != null)
                    {
                        this.DataContext = value;
                    }
                    else
                    {
                        this.DataContext = new Backend.Data.Conversation(null, null);
                    }
                }
                catch (Exception uiEx) { Frontend.UIError(uiEx); }
            } 
        }

        private Contact GetOther()
        {
            try
            {
                if (CurrentConversation != null && !string.IsNullOrEmpty(CurrentConversation.Self) && !string.IsNullOrEmpty(CurrentConversation.Other))
                {
                    var account = Frontend.Accounts[CurrentConversation.Self];
                    if (account != null)
                        return account.Roster[CurrentConversation.Other];
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return null;
        }

        private Contact GetSelf()
        {
            try
            {
                if (CurrentConversation != null && !string.IsNullOrEmpty(CurrentConversation.Self) && !string.IsNullOrEmpty(CurrentConversation.Other))
                {
                    var account = Frontend.Accounts[CurrentConversation.Self];
                    if (account != null)
                        return account.OwnContact;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return null;
        }

        public Conversation()
        {
            try
            {
                this.InitializeComponent();

                CurrentConversation = null;

                Frontend.Events.OnRosterContactSelected += OnRosterContactSelected;
                Frontend.Events.OnMessageReceived += OnMessageReceived;
                Frontend.Events.OnSettingsChanged += OnSettingsChanged;
                Frontend.Events.OnContactsChanged += OnContactsChanged;
                Frontend.CoreWindow.VisibilityChanged += CoreWindow_VisibilityChanged;

                SizeChanged += Conversation_SizeChanged;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnContactsChanged(object sender, EventArgs e)
        {
            UpdateOfflineWarnings();
        }

        private void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            try
            {
                if (args.Visible)
                    ClearMessageCount();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void Conversation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                OnSettingsChanged(this, new EventArgs());
                ScrollToBottom();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            try
            {
                if (CurrentConversation != null && CurrentConversation.Other != null)
                {
                    var other = GetOther();
                    OnRosterContactSelected(this, new ContactSelectedEventArgs(null));
                    OnRosterContactSelected(this, new ContactSelectedEventArgs(other));
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnMessageReceived(object sender, EventArgs e)
        {
            try
            {
                ClearMessageCount();
                ScrollToBottom();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnRosterContactSelected(object sender, Frontend.ContactSelectedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (e.Contact != null && !string.IsNullOrEmpty(e.Contact.jid))
                    {
                        var account = Frontend.Accounts[new XMPP.JID(e.Contact.account).Bare];
                        if (account == null || account.OwnContact == null)
                            return;

                        if (!account.CurrentConversations.Keys.Contains(e.Contact.jid))
                            account.CurrentConversations[e.Contact.jid] = new Backend.Data.Conversation(account.OwnContact.jid, e.Contact.jid);

                        // Remove old listerners
                        if (CurrentConversation != null)
                            CurrentConversation.Items.CollectionChanged -= OnCoversationItemCollectionChanged;

                        // Change to the new Conversation
                        CurrentConversation = account.CurrentConversations[e.Contact.jid];

                        UpdateOfflineWarnings();

                        // Remove old text
                        SendText.Text = string.Empty;

                        // Add new listener
                        CurrentConversation.Items.CollectionChanged += OnCoversationItemCollectionChanged;

                        ClearMessageCount();

                        ScrollToBottom();
                        
                        // Can be vary annoying
                        //SendText.Focus(FocusState.Programmatic);
                    }
                    else // No contact selected
                    {
                        if (CurrentConversation != null)
                        {
                            if (CurrentConversation.Items.Count > 0)
                            {
                                foreach (var item in CurrentConversation.Items)
                                    item.Messages.CollectionChanged -= OnConversationItemMessageCollectionChanged;
                            }

                            CurrentConversation.Items.CollectionChanged -= OnCoversationItemCollectionChanged;

                            AccountOfflineWarning.Visibility = Visibility.Collapsed;
                            ContactOfflineWarning.Visibility = Visibility.Collapsed;
                        }

                        CurrentConversation = null;
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnCoversationItemCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // Remove old listerners
                if (CurrentItem != null)
                    CurrentItem.Messages.CollectionChanged -= OnConversationItemMessageCollectionChanged;

                // Change to current Item
                CurrentItem = CurrentConversation.Items.Last();

                // Add new listener
                CurrentItem.Messages.CollectionChanged += OnConversationItemMessageCollectionChanged;

                ScrollToBottom();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnConversationItemMessageCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                ScrollToBottom();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }


        private void SendText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                    SendMessage();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void OnSendButton(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMessage();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void UpdateOfflineWarnings()
        {
            try
            {
                var other = GetOther();
                var self = GetSelf();
                if (other != null && self != null)
                {
                    if (!self.IsOnline)
                    {
                        AccountOfflineWarning.Visibility = Visibility.Visible;
                        ContactOfflineWarning.Visibility = Visibility.Collapsed;

                    }
                    else if (!other.IsOnline)
                    {
                        ContactOfflineWarning.Visibility = Visibility.Visible;
                        AccountOfflineWarning.Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        AccountOfflineWarning.Visibility = Visibility.Collapsed;
                        ContactOfflineWarning.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    AccountOfflineWarning.Visibility = Visibility.Collapsed;
                    ContactOfflineWarning.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void ClearMessageCount()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    var visible = false;
                    if (Frontend.CoreWindow != null)
                        visible = Frontend.CoreWindow.Visible;

                    if (CurrentConversation != null && visible)
                    {
                        if (CurrentConversation.Other != null)
                        {
                            var other = GetOther();
                            if (other != null && other.UnreadMessageCount >= 0)
                            {
                                other.LockUpdates();
                                other.UnreadMessageCount = 0;
                                other.UnlockUpdates();
                            }
                        }
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void ScrollToBottom()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (ItemList.Items.Count > 0)
                        ItemList.ScrollIntoView(ItemList.Items.First());
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void SendMessage()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (CurrentConversation != null && !string.IsNullOrEmpty(SendText.Text))
                    {
                        var account = Frontend.Accounts[CurrentConversation.Self];
                        if (account == null)
                            return;

                        if (account.persistantState == AccountState.Enabled)
                        {
                            var self = GetSelf();
                            var other = GetOther();
                            if (self != null && other != null)
                            {
                                if (!string.IsNullOrEmpty(self.jid) && !string.IsNullOrEmpty(other.CurrentJID))
                                {
                                    var message = XMPPHelper.SendMessage(self.jid, other.CurrentJID, SendText.Text);
                                    if (message != null)
                                        CurrentConversation.AddMessage(message);

                                    SendText.Text = string.Empty;
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnImageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    var image = sender as AvatarControl;
                    if (image != null)
                        image.Height = image.ActualWidth;

                    image.UpdateLayout();
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }
    }
}
