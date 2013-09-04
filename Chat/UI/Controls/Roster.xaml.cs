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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using XMPP.tags.jabber.iq.roster;

namespace Chat.UI.Controls
{
    public sealed partial class Roster : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }

        private List<Contact> _listBuffer = null;
        private Contact _selectedContact = null;

        private const int StickyListSize = 5;
        private LinkedList<Contact> StickyList = new LinkedList<Contact>();

        public Roster()
        {
            this.InitializeComponent();

            _updateCountdownTimer.Interval = TimeSpan.FromMilliseconds(_countdownResolution);
            _updateCountdownTimer.Tick += CountDown;
            _updateCountdownTimer.Start();

            // There is no other way to detect this
            AddHandler(PointerWheelChangedEvent, new PointerEventHandler(OnPointerWheelChanged), true);

            Frontend.Events.OnDeselectContact += (s, e) => DeselectContact();

            // Events upon which the data has to be updated
            Frontend.Events.OnStatusChanged         += (s, e) => UpdateData();
            Frontend.Events.OnContactsChanged       += (s, e) => UpdateData();
            Frontend.Events.OnMessageReceived       += (s, e) => UpdateData();
            Frontend.Events.OnAccountListChanged    += (s, e) => UpdateData();

            Frontend.Events.OnSettingsChanged += (s, e) =>
            {
                _isListScrolled = true;
                UpdateData();
            };

        }

        #region Control functions

        private void OnRosterListTapped(object sender, TappedRoutedEventArgs e) { DelayViewUpdate(); }
        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e) { DelayViewUpdate(); }
        private void OnRosterListPointerCaptureLost(object sender, PointerRoutedEventArgs e) { DelayViewUpdate(); }

        private async void DeselectContact()
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    bool backSwap = false;

                    foreach (Contact contact in RosterList.DataContext as List<Contact>)
                    {
                        if(contact.HasUnreadMessages)
                        {
                            backSwap = true;
                            _selectedContact = contact;
                            RosterList.SelectedItem = contact;
                        }
                    }

                    if(!backSwap)
                    {
                        _selectedContact = null;
                        RosterList.SelectedItem = null;
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_lockContactSelection)
                {
                    _selectedContact = (Contact)RosterList.SelectedItem;
                    Frontend.Events.RosterContactSelected(this, _selectedContact);

                    if (Frontend.Settings.stickyRosterContacts)
                        UpdateData();
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnEditSubscription(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    var image = sender as Image;
                    if (image != null)
                    {
                        var contact = image.Tag as Contact;
                        if (contact != null)
                            Frontend.Events.SubscriptionContactSelected(this, contact);
                    }
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        #endregion

        #region Update timing

        private bool _isDataUpdateRunning   = false; 
        private bool _isDataUpdatePending   = false; 

        private bool _isViewUpdateRunning   = false;
        private bool _isViewUpdatePending   = false;

        private bool _isListScrolled        = false;
        private bool _isListBufferUpdated   = false; 

        private bool _lockContactSelection  = false;
        private ManualResetEvent _lockListBuffer = new ManualResetEvent(true);

        private const int _countdownResolution = 500;
        private const int _refreshDelay = 2500;
        private int _countdownCounter = 0;
        private DispatcherTimer _updateCountdownTimer = new DispatcherTimer();
        private void CountDown(object sender, object e)
        {
            try
            {
                if (_countdownCounter > 0)
                {
                    _countdownCounter -= _countdownResolution;
                }
                else
                {
                    _updateCountdownTimer.Stop();
                    UpdateView();
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void DelayViewUpdate()
        {
            try
            {
                _isListScrolled = true;
                _countdownCounter = _refreshDelay;

                if (!_updateCountdownTimer.IsEnabled)
                    _updateCountdownTimer.Start();
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        #endregion

        #region Update Processing

        private async void UpdateData()
        {
            try
            {
                if (_isDataUpdateRunning)
                {
                    _isDataUpdatePending = true;
                    return;
                }

                _isDataUpdateRunning = true;
                {
                    _isDataUpdatePending = true;
                    await Frontend.RunAsync(() =>
                    {
                        List<Contact> newList = null;

                        while (_isDataUpdatePending)
                        {
                            _isDataUpdatePending = false;
                            newList = GenerateList();
                        }

                        if (newList != null)
                        {
                            _lockListBuffer.WaitOne(10000);
                            _lockListBuffer.Reset();

                            _listBuffer = newList;
                            _isListBufferUpdated = true;

                            _lockListBuffer.Set();

                            UpdateView();
                        }
                    });
                }

                _isDataUpdateRunning = false;

                if (_isDataUpdatePending)
                {
                    _isDataUpdatePending = false;
                    UpdateData();
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void UpdateView()
        {
            try
            {
                if (_isViewUpdateRunning)
                {
                    _isViewUpdatePending = true;
                    return;
                }

                _isViewUpdateRunning = true;

                if (!_updateCountdownTimer.IsEnabled)
                {
                    if (_isListBufferUpdated)
                    {
                        await Frontend.RunAsync(() =>
                        {
                            _lockContactSelection = true;
                            {
                                _lockListBuffer.WaitOne(10000);
                                _lockListBuffer.Reset();
                                {
                                    RosterList.DataContext = _listBuffer;
                                    _isListBufferUpdated = false;
                                }
                                _lockListBuffer.Set();

                                if (_listBuffer.Contains(_selectedContact))
                                    RosterList.SelectedItem = _selectedContact;
                                else
                                    RosterList.SelectedItem = _selectedContact = null;
                            }
                            _lockContactSelection = false;
                        });
                    }

                    if (_isListScrolled)
                    {
                        await Frontend.RunAsync(() =>
                        {
                            if (Frontend.Settings.autoScrollRoster && RosterList.Items.Count > 0)
                            {
                                if (Frontend.Settings.stickyRosterContacts || _selectedContact == null)
                                    RosterList.ScrollIntoView(RosterList.Items.First());
                                else
                                    RosterList.ScrollIntoView(_selectedContact);
                            }

                            _isListScrolled = false;
                        });
                    }
                }

                _isViewUpdateRunning = false;

                if (_isViewUpdatePending)
                {
                    _isViewUpdatePending = false;
                    UpdateView();
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        #endregion

        #region Helper

        private List<Contact> GenerateList()
        {
            try
            {
                var accounts = Frontend.Accounts;
    
                var allContacts = new List<Contact>();
                foreach( var account in accounts)
                    allContacts.AddRange(account.Roster);

                var displayContacts = from contact in allContacts where ShowContact(contact) select contact;

                if (Frontend.Settings.autoSortRoster)
                {
                    displayContacts =   from contact in displayContacts
                                        orderby contact.DisplayName ascending
                                        orderby contact.CurrentStatus descending
                                        orderby contact.UnreadMessageCount descending
                                        select contact;
                }

                List<Contact> returnList = displayContacts.ToList();

                if (Frontend.Settings.stickyRosterContacts)
                {
                    var removeList = (from contact in StickyList where !returnList.Contains(contact) select contact).ToList();
                    foreach (var contact in removeList)
                        StickyList.Remove(contact);

                    if (returnList.Contains(_selectedContact))
                    {
                        if (StickyList.Contains(_selectedContact))
                            StickyList.Remove(_selectedContact);

                        while (StickyList.Count > StickyListSize)
                            StickyList.RemoveLast();

                        StickyList.AddFirst(_selectedContact);
                    }

                    foreach (var contact in StickyList.Reverse())
                    {
                        if (returnList.Contains(contact))
                            returnList.Remove(contact);

                        returnList.Insert(0, contact);
                    }
                }

                return returnList;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return null;
        }

        private bool ShowContact(Contact contact)
        {
            if (_selectedContact != null && contact == _selectedContact)
                return true;

            if (contact.HideContact)
                return false;

            if (Frontend.Settings.showOffline || contact.IsOnline || contact.HasUnreadMessages || contact.subscription == item.subscriptionEnum.from || contact.subscription == item.subscriptionEnum.to || contact.subscriptionRequest == Contact.SubscriptionRequestType.Subscribe)
                return true;

            return false;
        }

        #endregion

    }
}
