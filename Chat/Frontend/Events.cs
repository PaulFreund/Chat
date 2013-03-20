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

namespace Chat.Frontend
{
    public class ContactSelectedEventArgs : EventArgs
    {
        public ContactSelectedEventArgs(Contact item) { Contact = item; }
        public readonly Contact Contact;
    }

    public class Events
    {
        public Events() 
        {
            CheckTimer();
        }

        private const int _refreshTime = 5;
        private bool _invokeUpdate = false;
        private DispatcherTimer _updateTimer = new DispatcherTimer();

        public event ContactsChangedHandler OnContactsChanged;  
        public delegate void ContactsChangedHandler(object sender, EventArgs e);

        public void ContactsChanged()
        {
            CheckTimer();
            _invokeUpdate = true;
        }

        private void CheckTimer()
        {
            if (!_updateTimer.IsEnabled)
            {
                _updateTimer.Interval = TimeSpan.FromSeconds(_refreshTime);
                _updateTimer.Tick -= ContactChangedInvoker;
                _updateTimer.Tick += ContactChangedInvoker;
                _updateTimer.Start();
            }
        }

        private void ContactChangedInvoker(object sender, object e)
        {
            if (_invokeUpdate)
            {
                if (OnContactsChanged != null)
                    OnContactsChanged(null, null);

                _invokeUpdate = false;
            }
        }

        public event StatusChangedHandler OnStatusChanged;
        public delegate void StatusChangedHandler(object sender, EventArgs e);
        public void StatusChanged() { if (OnStatusChanged != null) OnStatusChanged(null, null); }

        public event SettingsChangedHandler OnSettingsChanged;
        public delegate void SettingsChangedHandler(object sender, EventArgs e);
        public void SettingsChanged() { if (OnSettingsChanged != null) OnSettingsChanged(null, null); }

        public event MessageReceivedHandler OnMessageReceived;
        public delegate void MessageReceivedHandler(object sender, EventArgs e);
        public void MessageReceived() { if (OnMessageReceived != null) OnMessageReceived(null, null); }

        public event AccountListChangedHandler OnAccountListChanged;
        public delegate void AccountListChangedHandler(object sender, EventArgs e);
        public void AccountListChanged() { if (OnAccountListChanged != null) OnAccountListChanged(null, null); }

        public event DeselectContactHandler OnDeselectContact;
        public delegate void DeselectContactHandler(object sender, EventArgs e);
        public void DeselectContact() { if (OnDeselectContact != null) OnDeselectContact(null, null); }


        public delegate void ContactSelectedHandler(object sender, ContactSelectedEventArgs e);

        public event ContactSelectedHandler OnRosterContactSelected;
        public void RosterContactSelected(object sender, Contact item) { if (OnRosterContactSelected != null) OnRosterContactSelected(sender, new ContactSelectedEventArgs(item)); }

        public event ContactSelectedHandler OnSubscriptionContactSelected;
        public void SubscriptionContactSelected(object sender, Contact item) { if (OnSubscriptionContactSelected != null) OnSubscriptionContactSelected(sender, new ContactSelectedEventArgs(item)); }
    }
}
