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
using System.Linq;
using System.Xml.Linq;
using XMPP;
using Tags = XMPP.tags;

namespace Chat.Frontend
{
    public class Interpreter : Tags.TagHandler<bool>
    {
        private App Frontend { get { return (App)App.Current; } }

        private bool Process(Tags.jabber.client.presence presence)
        {
            var settings = Frontend.Settings;

            try
            {
                if (presence.type == Tags.jabber.client.presence.typeEnum.error)
                {
                    var errorMessage = Helper.Translate("ErrorTagPresence") + ": " + Helper.GetErrorMessage(presence);
                    Frontend.Notifications.CreateError(ErrorPolicyType.Informative, presence.Account, errorMessage);
                    return false;
                }

                if (presence.from == null)
                    presence.from = presence.Account;

                var from = new XMPP.JID(presence.from);
                // The rest of the application only accepts to = jid
                var to = new XMPP.JID(presence.Account);


                // Get matching Account
                Account account = Frontend.Accounts[presence.Account];
                if (account == null)
                    return false;

                Backend.Data.Contact contact = account.Roster[from.Bare];
                if (contact == null)
                    contact = account.Roster.CreateContact(account.jid, from.Bare);

                if (contact == null)
                    return false;

                contact.LockUpdates();

                // Get nick if any
                var nick = presence.Element<Tags.jabber.protocol.nick.nick>(Tags.jabber.protocol.nick.Namespace.nick);
                if (nick != null)
                    contact.nick = nick.Value;

                // Get avatar hash if any
                if (settings.autoDownloadAvatars)
                {
                    var x = presence.Element<Tags.vcard_temp.x.update.x>(Tags.vcard_temp.x.update.Namespace.x);
                    if (x != null)
                    {
                        var photo = x.Element<Tags.vcard_temp.x.update.photo>(Tags.vcard_temp.x.update.Namespace.photo);
                        if (photo != null)
                        {
                            if (!string.IsNullOrEmpty(photo.Value))
                            {
                                // Request new photo if available
                                if (contact.photohash != photo.Value)
                                {
                                    if (!contact.vCardRequested)
                                    {
                                        contact.vCardRequested = true;
                                        var iq = new Tags.jabber.client.iq();

                                        iq.from = account.CurrentJID;
                                        iq.to = new XMPP.JID(presence.from).Bare;
                                        iq.type = Tags.jabber.client.iq.typeEnum.get;
                                        var vcard = new Tags.vcard_temp.vCard();
                                        iq.Add(vcard);

                                        iq.Timestamp = DateTime.Now;
                                        iq.Account = presence.Account;

                                        Frontend.Backend.SendTag(iq.Account, iq);
                                    }
                                }
                            }
                        }
                    }
                }

                // No resource, fix it
                if (string.IsNullOrEmpty(from.Resource))
                    from.Resource = from.Bare;

                // Set resource and status
                switch (presence.type)
                {
                    case Tags.jabber.client.presence.typeEnum.none:
                        {
                            // Get Show
                            var showElement = presence.showElements.FirstOrDefault();
                            var status = showElement != null ? showElement.Value : Tags.jabber.client.show.valueEnum.none;

                            // Get status message
                            var statusElement = presence.statusElements.FirstOrDefault();
                            var statusMessage = statusElement != null ? statusElement.Value : string.Empty;

                            // Get priority
                            var priorityElement = presence.priorityElements.FirstOrDefault();
                            var priority = priorityElement != null ? priorityElement.Value : 0;

                            contact.SetResource(from.Resource, priority, status, statusMessage);
                            break;
                        }

                    case Tags.jabber.client.presence.typeEnum.unavailable:
                        {
                            contact.RemoveResource(from.Resource);
                            break;
                        }

                    case Tags.jabber.client.presence.typeEnum.subscribe:
                        {
                            contact.subscriptionRequest = Backend.Data.Contact.SubscriptionRequestType.Subscribe;
                            Frontend.Notifications.CreateRequest(NotificationRequestType.Subscribe, presence.Account, presence.from);
                            break;
                        }

                    case Tags.jabber.client.presence.typeEnum.subscribed:
                        {
                            Frontend.Notifications.CreateInformative(NotificationInfoType.Subscribed, presence.Account, presence.from);
                            break;
                        }
                    case Tags.jabber.client.presence.typeEnum.unsubscribe:
                        {
                            contact.subscriptionRequest = Backend.Data.Contact.SubscriptionRequestType.Unsubscribe;
                            Frontend.Notifications.CreateInformative(NotificationInfoType.Unsubscribe, presence.Account, presence.from);
                            break;
                        }
                    case Tags.jabber.client.presence.typeEnum.unsubscribed:
                        {
                            Frontend.Notifications.CreateInformative(NotificationInfoType.Unsubscribed, presence.Account, presence.from);
                            break;
                        }
                }

                contact.UnlockUpdates();
                Frontend.Events.ContactsChanged();

                return true;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return false;
        }

        private bool Process(Tags.jabber.client.message message)
        {
            var settings = Frontend.Settings;

            try
            {
                if (message.type == Tags.jabber.client.message.typeEnum.error)
                {
                    var errorMessage = Helper.Translate("ErrorTagMessage") + ": " + Helper.GetErrorMessage(message);
                    Frontend.Notifications.CreateError(ErrorPolicyType.Informative, message.Account, errorMessage);
                    return false;
                }

                if (message.type == Tags.jabber.client.message.typeEnum.chat || message.type == Tags.jabber.client.message.typeEnum.normal)
                {
                    if (message.bodyElements.Count() > 0)
                    {
                        JID from = message.from;

                        // The rest of the application only accepts to = jid
                        JID to = message.Account;

                        Account account = Frontend.Accounts[message.Account];
                        if (account != null)
                        {
                            Backend.Data.Contact contact = account.Roster[from.Bare];

                            if (contact == null && settings.allowUnknownSenders)
                                contact = account.Roster.CreateContact(account.jid, from.Bare);

                            if (contact == null)
                                return false;

                            contact.SetLastSender(from.Resource);

                            if (account.OwnContact != null && contact != null)
                            {
                                if (!account.CurrentConversations.Keys.Contains(from.Bare))
                                    account.CurrentConversations[from.Bare] = new Conversation(account.OwnContact.jid, contact.jid);

                                account.CurrentConversations[from.Bare].AddMessage(message);
                            }

                            contact.LockUpdates();
                            contact.UnreadMessageCount++;
                            contact.UnlockUpdates();

                            Frontend.Events.MessageReceived();
                            Frontend.Events.ContactsChanged();

                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return false;
        }

        private bool Process(Tags.jabber.client.iq iq)
        {
            try
            {
                if (iq.type == Tags.jabber.client.iq.typeEnum.error)
                {
                    var errorMessage = Helper.Translate("ErrorTagIq") + ": " + Helper.GetErrorMessage(iq);
                    Frontend.Notifications.CreateError(ErrorPolicyType.Informative, iq.Account, errorMessage);
                    return false;
                }

                Account account = Frontend.Accounts[iq.Account];
                if (account == null)
                    return false;

                if (iq.type == Tags.jabber.client.iq.typeEnum.result ||
                    iq.type == Tags.jabber.client.iq.typeEnum.set)
                {
                    #region query

                    var query = iq.Element<Tags.jabber.iq.roster.query>(Tags.jabber.iq.roster.Namespace.query);
                    if (query != null)
                    {
                        if (iq.type == Tags.jabber.client.iq.typeEnum.result) // This is a roster
                        {
                            account.Roster.ClearOffline();
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("[Frontend] Received Roster: " + iq.id);
#endif
                        }

                        foreach (var item in query.itemElements)
                        {
                            Backend.Data.Contact contact = account.Roster[item.jid];
                            if (contact == null)
                                contact = account.Roster.CreateContact(account.jid, item.jid);

                            if (contact == null)
                                continue;

                            contact.LockUpdates();

                            // Remove
                            if (item.subscription == Tags.jabber.iq.roster.item.subscriptionEnum.remove)
                            {
                                account.Roster.Remove(contact);
                            }
                            // Add or update
                            else
                            {
                                contact.name = item.name;
                                contact.subscription = item.subscription;

                                contact.ask = item.ask;

                                contact.Groups.Clear();
                                foreach (var group in item.groupElements)
                                    contact.Groups.Add(group.Value);
                            }

                            contact.UnlockUpdates();

                        }

                        Frontend.Events.ContactsChanged();
                    }

                    #endregion

                    var bind = iq.Element<Tags.xmpp_bind.bind>(Tags.xmpp_bind.Namespace.bind);
                    if (bind != null)
                    {
                        var jid = new JID(bind.jid.Value);
                        if (!string.IsNullOrEmpty(jid))
                        {
                            account.serverJID = jid;
                            account.OwnResource = jid.Resource;
                            account.OwnContact.SetResource(jid.Resource);
                        }
                    }

                    #region vcard

                    var vcard = iq.Element<Tags.vcard_temp.vCard>(Tags.vcard_temp.Namespace.vCard);
                    if (vcard != null)
                    {
                        if (string.IsNullOrEmpty(iq.from) || new JID(iq.from).Bare == iq.Account)
                        {
                            account.CurrentVCard = vcard;
                        }

                        var photo = vcard.Element(XName.Get("PHOTO", "vcard-temp"));
                        if (photo != null)
                        {
                            var type = photo.Element(XName.Get("TYPE", "vcard-temp"));
                            var binval = photo.Element(XName.Get("BINVAL", "vcard-temp"));

                            if (iq.from == null)
                                iq.from = iq.Account;

                            var jid = new JID(iq.from).Bare;

                            if (type != null && binval != null)
                            {
                                Backend.Data.Contact contact = account.Roster[jid];
                                if (contact == null)
                                    contact = account.Roster.CreateContact(account.jid, jid);

                                if (contact == null)
                                    return false;

                                contact.LockUpdates();

                                contact.SetAvatar(binval.Bytes);
                                contact.vCardRequested = false;

                                contact.UnlockUpdates();
                            }
                        }
                    }

                    #endregion
                }

                return true;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return false;
        }
    }
}

