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
using System.Linq;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Tags = XMPP.tags;

namespace Backend.Common
{
    public class Notifier : Tags.TagHandler<bool>
    {
        private readonly BadgeUpdater BadgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
        private readonly Runtime Backend = null;

        private int NotificationCount
        {
            get { return new Status().currentNotificationCount; }
            set { new Status().currentNotificationCount = value; }
        }

        private bool errorState = false;
        private bool windowVisible = false;

        public Notifier(Runtime backend) : base(false)
        {
            if (backend != null)
                Backend = backend;
            else
                throw new Exception("Connections constructor - BACKEND NOT DEFINED");

            UpdateBadge();
        }

        private CoreWindow _coreWindow = null;
        public CoreWindow CoreWindow
        {
            get { return _coreWindow; }
            set
            {
                try
                {
                    if (_coreWindow != null)
                        _coreWindow.VisibilityChanged -= FrontendVisibilityChanged;

                    if (value != null)
                    {
                        _coreWindow = value;
                        windowVisible = _coreWindow.Visible;
                        _coreWindow.VisibilityChanged += FrontendVisibilityChanged;
                    }
                }
                catch { }
            }
        }



        #region publicmethods

        // Return value determins persistancy for Event queue
        public bool Push(BackendEvent event_)
        {
            if (event_ is BackendEventMessage)
            {
                var messageEvent = event_ as BackendEventMessage;
                return Process(messageEvent.Tag);
            }
            else if (event_ is BackendEventState)
            {
                var stateEvent = event_ as BackendEventState;
                UpdateBadge();
            }
            else if (event_ is BackendEventError)
            {
                var errorEvent = event_ as BackendEventError;
                NotifyError(errorEvent.Policy, Helper.Translate("Error") + ": " + Helper.Translate("ErrorType"+errorEvent.Error.ToString()));
            }
            else if (event_ is BackendEventRequest)
            {
                var requestEvent = event_ as BackendEventRequest;
                if (requestEvent.RequestType == RequestType.BackgroundAccess)
                {
                    if (!windowVisible)
                        NotifyError(XMPP.ErrorPolicyType.Severe, Helper.Translate("NotifyBackgroundAccess"));
                }
            }
            else if (event_ is BackendEventWindows)
            {
                var windowsEvent = event_ as BackendEventWindows;
                if (windowsEvent.WindowsType == WindowsType.UserAway || windowsEvent.WindowsType == WindowsType.UserPresent ) // Es hat mit autoaway zutun
                {
                    var settings = new Settings();
                    if (settings.autoAway) // Autoaway ist an 
                    {
                        var status = new Status();

                        // Der user ist nicht am rechner und war bisher online also sind alle voraussetzungen erfuellt
                        if (windowsEvent.WindowsType == WindowsType.UserAway && status.status == StatusType.Available)
                            Helper.PublishState(StatusType.Away, settings.autoAwayMessage);

                        // Der nutzer ist am rechner, egal was vorher war, er will seinen eigentlichen status gebroadcasted haben
                        if (windowsEvent.WindowsType == WindowsType.UserPresent )
                            Helper.PublishState(status.status, status.message );
                    }
                }
            }

            return false;
        }

        #endregion

        #region privatemethods

        private void IncrementNotificationCount()
        {
            if (!windowVisible)
                NotificationCount++;
        }

        private bool Process(Tags.jabber.client.presence presence)
        {
            if (presence.type == Tags.jabber.client.presence.typeEnum.error)
            {
                var errorMessage = Helper.GetErrorMessage(presence);
                NotifyError(XMPP.ErrorPolicyType.Informative, Helper.Translate("ErrorTagPresence") + ": " + errorMessage);
            }
            else if (presence.type == Tags.jabber.client.presence.typeEnum.subscribe)
            {
                var fromJid = new XMPP.JID(presence.from);
                Notify(presence.Account, presence.from, fromJid + " " + Helper.Translate("SubscriptionRequest"));
            }

            return false;
        }

        private bool Process(Tags.jabber.client.message message)
        {
            if (message.type == Tags.jabber.client.message.typeEnum.error)
            {
                var errorMessage = Helper.GetErrorMessage(message);
                NotifyError(XMPP.ErrorPolicyType.Informative, Helper.Translate("ErrorTagMessage") + ": " + errorMessage);
            }
            else if (message.type == Tags.jabber.client.message.typeEnum.chat || message.type == Tags.jabber.client.message.typeEnum.normal)
            {
                if (message.bodyElements.Count() > 0)
                {
                    var from = new XMPP.JID(message.from);
                    var bodytext = string.Join(" ", (from body in message.bodyElements select body.Value));
                    Notify(message.Account, from.Bare, bodytext);
                    return true;
                }
            }

            return false;
        }

        private bool Process(Tags.jabber.client.iq iq)
        {
            if (iq.type == Tags.jabber.client.iq.typeEnum.error)
            {
                var errorMessage = Helper.GetErrorMessage(iq);
                NotifyError(XMPP.ErrorPolicyType.Informative, Helper.Translate("ErrorTagIq") + ": " + errorMessage);
                return false;
            }

            return true;
        }

        public void UpdateBadge()
        {
            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var badgeAttributes = badgeXml.GetElementsByTagName("badge");

            bool clearValue = false;

            if (errorState)
            {
                badgeAttributes[0].Attributes.GetNamedItem("value").NodeValue = "error";
            }
            else if (NotificationCount > 0)
            {
                badgeAttributes[0].Attributes.GetNamedItem("value").NodeValue = NotificationCount.ToString();
            }
            else if (new Accounts().Enabled.Count() <= 0)
            {
                clearValue = true;
            }
            else
            {
                var status = new Status();
                string value = "";
                switch (status.status)
                {
                    case StatusType.Available:
                        value = "available";
                        break;
                    case StatusType.Away:
                        value = "away";
                        break;
                    case StatusType.Busy:
                        value = "busy";
                        break;
                    case StatusType.Offline:
                        value = "unavailable";
                        break;
                }

                badgeAttributes[0].Attributes.GetNamedItem("value").NodeValue = value;
            }

            if (clearValue)
                BadgeUpdater.Clear();
            else
                BadgeUpdater.Update(new BadgeNotification(badgeXml));
        }


        private void NotifyError(XMPP.ErrorPolicyType type, string message)
        {
            var settings = new Settings();
            if (type != XMPP.ErrorPolicyType.Informative || settings.showInformativeErrors)
            {
                WriteToastSystem(message);
                errorState = true;
                UpdateBadge();
            }
        }

        private void Notify(string account, string jid, string message)
        {
            WriteToast(account, jid, message);
            IncrementNotificationCount();
            UpdateBadge();
        }

        private void WriteToast(string account, string jid, string message)
        {

            var settings = new Settings();
            if (settings.notificationToast && ( !windowVisible || settings.notificationVisible ) )
            {
                var contactname = jid;
                var imageURI = "";

                var Accounts = new Accounts();
                var accountObj = Accounts[account];
                if (accountObj == null)
                    return;

                var contact = accountObj.Roster[jid];
                if (contact != null)
                {
                    contactname = contact.DisplayName;

                    if (contact.ImageURI != null)
                        imageURI = contact.ImageURI.ToString();
                }

                XDocument toast = new XDocument();

                XElement toastElement = new XElement("toast",
                    new XElement("visual",
                        new XElement("binding",
                            new XAttribute("template", "ToastImageAndText02"),
                            new XElement("image",
                                new XAttribute("id", "1"),
                                new XAttribute("src", imageURI)
                            ),
                            new XElement("text",
                                new XAttribute("id", "1"),
                                new XText(contactname)
                            ),
                            new XElement("text",
                                new XAttribute("id", "2"),
                                new XText(message)
                            )
                        )
                    )
                );

                if (settings.notificationSound)
                {
                    var audio = new XElement("audio");
                    audio.Add(new XAttribute("src", "ms-winsoundevent:Notification.IM"));
                    audio.Add(new XAttribute("silent", "false"));
                    toastElement.Add(audio);
                }

                toast.Add(toastElement);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(toast.ToString());

                // Send it 
                ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc));
            }
        }

        private void WriteToastSystem(string message)
        {
            var settings = new Settings();

            XDocument toast = new XDocument();

            XElement toastElement = new XElement("toast",
                new XElement("visual",
                    new XElement("binding",
                        new XAttribute("template", "ToastImageAndText01"),
                            new XElement("image",
                                new XAttribute("id", "1"),
                                new XAttribute("src", "ms-appx:///Assets/Error.png")
                            ),
                            new XElement("text",
                                new XAttribute("id", "1"),
                                new XText(message)
                            )
                    )
                )
            );

            if (settings.notificationSound)
            {
                var audio = new XElement("audio");
                audio.Add(new XAttribute("src", "ms-winsoundevent:Notification.IM"));
                audio.Add(new XAttribute("silent", "false"));
                toastElement.Add(audio);
            }

            toast.Add(toastElement);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(toast.ToString());

            // Send it 
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(doc));
        }

        #endregion

        #region eventhandler

        private void FrontendVisibilityChanged(Windows.UI.Core.CoreWindow sender, VisibilityChangedEventArgs args)
        {
            windowVisible = args.Visible;
            NotificationCount = 0;
            errorState = false;
            UpdateBadge();
        }

        #endregion
    }
}
