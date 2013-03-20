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
using Chat.UI.Flyout;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using XMPP;

namespace Chat.Frontend
{
    public enum NotificationType
    {
        Informative = 0,    // Notify about something that happened
        Request = 1,        // Open the mathing flyout
        Error = 2           // Depending on policy: Informative - Just display, Deactivate - Open accounts flyout, Severe - close programm
    }

    public enum NotificationInfoType
    {
        Subscribed,     // Notify that he now sends you updates to his status
        Unsubscribed,   // Notify that he dosn't send you updates to his status anymore
        Unsubscribe     // Notify that he dosn't want to see your updates anymore
    }

    public enum NotificationRequestType
    {
        Subscribe // Open Subscription flyout
    }

    public class Notifications : INotifyPropertyChanged
    {
        private App Frontend { get { return (App)App.Current; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Notification> NotificationList { get; set; }

        public bool HasNotifications { get { return (NotificationCount > 0); } }
        public int NotificationCount { get { return NotificationList.Count; } }

        public NotificationType WorstType
        {
            get
            {
                try
                {
                    var worst = NotificationType.Informative;
                    foreach (var notification in NotificationList)
                    {
                        if (notification.Type > worst)
                            worst = notification.Type;
                    }
                    return worst;
                }
                catch (Exception uiEx) { Frontend.UIError(uiEx); }
                return NotificationType.Informative;
            }
        }

        public Notifications()
        {
            try
            {
                NotificationList = new ObservableCollection<Notification>();
                NotificationList.CollectionChanged += NotificationList_CollectionChanged;

                foreach (var account in Frontend.Accounts)
                {
                    if (account.forceDisabled)
                        CreateError(ErrorPolicyType.Deactivate, account.jid, Helper.Translate("ErrorPreviousDisabled"));
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        protected void EmitPropertyChanged(string propertyName)
        {
            try
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        void NotificationList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                EmitPropertyChanged("HasNotifications");
                EmitPropertyChanged("NotificationCount");
                EmitPropertyChanged("WorstType");
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private void AddNotification(Notification notification)
        {
            foreach (var not in NotificationList)
            {
                if (not.Equals(notification))
                    return;
            }

            NotificationList.Add(notification);
        }

        public void CreateError(BackendEventError error)
        {
            try
            {
                var errorMessage = Helper.Translate("ErrorType" + error.Error.ToString());
                CreateError(error.Policy, error.Id, errorMessage, error.Message);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public void CreateError(ErrorPolicyType errorType, string account, string message, string details = "")
        {
            try
            {
                if (errorType != ErrorPolicyType.Informative || Frontend.Settings.showInformativeErrors)
                {
                    var notification = new Notification();
                    notification.Account = account;
                    notification.Type = NotificationType.Error;
                    notification.Message = message;
                    notification.Details = details;

                    switch (errorType)
                    {
                        case ErrorPolicyType.Deactivate: { notification.Action = FlyoutType.AccountListEdit; break; }
                        case ErrorPolicyType.Informative: { notification.Action = FlyoutType.None; break; }
                        case ErrorPolicyType.Severe: { notification.Action = FlyoutType.None; break; }
                    }

                    notification.Data = null;
                    AddNotification(notification);
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public void CreateRequest(NotificationRequestType requestType, string account, string from)
        {
            try
            {
                var accountObj = Frontend.Accounts[account];
                if (accountObj == null)
                    return;

                var fromJID = new JID(from);
                var contact = accountObj.Roster[fromJID.Bare];
                if (contact == null)
                    return;


                var notification = new Notification();
                notification.Account = account;
                notification.Type = NotificationType.Request;

                if (requestType == NotificationRequestType.Subscribe)
                {
                    notification.Message = fromJID.Bare + " " + Helper.Translate("SubscriptionRequest");
                    notification.Action = FlyoutType.Subscription;
                    notification.Data = contact;
                }

                AddNotification(notification);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public void CreateInformative(NotificationInfoType infoType, string account, string from)
        {
            try
            {
                var accountObj = Frontend.Accounts[account];
                if (accountObj == null)
                    return;

                var fromJID = new JID(from);
                var contact = accountObj.Roster[fromJID.Bare];
                if (contact == null)
                    return;


                var notification = new Notification();
                notification.Account = account;
                notification.Type = NotificationType.Request;

                switch (infoType)
                {
                    case NotificationInfoType.Subscribed:    // Notify that he now sends you updates to his status
                        notification.Message = fromJID.Bare + " " + Helper.Translate("SubscriptionAllowed");
                        break;

                    case NotificationInfoType.Unsubscribed:  // Notify that he dosn't send you updates to his status anymore
                        notification.Message = fromJID.Bare + " " + Helper.Translate("SubscriptionRevoked");
                        break;
                    case NotificationInfoType.Unsubscribe:   // Notify that he dosn't want to see your updates anymore
                        notification.Message = fromJID.Bare + " " + Helper.Translate("SubscriptionUnsubscribed");
                        break;
                }

                notification.Action = FlyoutType.Subscription;
                notification.Data = contact;
                AddNotification(notification);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }
    }

    public class Notification
    {
        public Notification()
        {
            Account = "";
            Type = NotificationType.Informative;
            Message = "";
            Details = "";
            Action = FlyoutType.None;
            Data = null;
        }

        public string Account { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public FlyoutType Action { get; set; }
        public object Data { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Notification))
                return false;

            var other = obj as Notification;

            if (other.Account != this.Account)
                return false;

            if (other.Type != this.Type)
                return false;

            if (other.Message != this.Message)
                return false;

            if (other.Details != this.Details)
                return false;

            // We don't compare those two because theyre irrelevant

            //if (other.Action != this.Action)
            //    return false;

            //if( other.Data != this.Data )
            //    return false;

            return true;
        }
    }

}
