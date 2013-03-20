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
using System.Xml.Linq;
using Tags = XMPP.tags;

namespace Chat.Frontend
{
    public class XMPPHelper
    {
        private static App Frontend { get { return (App)App.Current; } }

        public static void Subscribe(Contact contact)
        {
            try
            {
                // Unsubscribe from the contact
                var subscribe = new Tags.jabber.client.presence();
                subscribe.type = Tags.jabber.client.presence.typeEnum.subscribe;
                subscribe.to = contact.jid;

                Frontend.Backend.SendTag(contact.account, subscribe);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void Subscribed(Contact contact)
        {
            try
            {
                // Unsubscribe to the contact
                var subscribed = new Tags.jabber.client.presence();
                subscribed.type = Tags.jabber.client.presence.typeEnum.subscribed;
                subscribed.to = contact.jid;

                Frontend.Backend.SendTag(contact.account, subscribed);

                contact.subscriptionRequest = Contact.SubscriptionRequestType.None;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void Unsubscribe(Contact contact)
        {
            try
            {
                // Unsubscribe from the contact
                var unsubscribe = new Tags.jabber.client.presence();
                unsubscribe.type = Tags.jabber.client.presence.typeEnum.unsubscribe;
                unsubscribe.to = contact.jid;

                Frontend.Backend.SendTag(contact.account, unsubscribe);

                contact.subscriptionRequest = Contact.SubscriptionRequestType.None;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void Unsubscribed(Contact contact)
        {
            try
            {
                // Unsubscribe to the contact
                var unsubscribed = new Tags.jabber.client.presence();
                unsubscribed.type = Tags.jabber.client.presence.typeEnum.unsubscribed;
                unsubscribed.to = contact.jid;

                Frontend.Backend.SendTag(contact.account, unsubscribed);

                contact.subscriptionRequest = Contact.SubscriptionRequestType.None;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void RemoveContact(Contact contact)
        {
            try
            {
                var account = Frontend.Accounts[contact.account];
                if (account != null)
                {
                    Unsubscribe(contact);
                    Unsubscribed(contact);

                    // Remove hin from the roster
                    var iq = new Tags.jabber.client.iq();
                    iq.from = account.CurrentJID;
                    iq.type = Tags.jabber.client.iq.typeEnum.set;
                    var query = new Tags.jabber.iq.roster.query();
                    var item = new Tags.jabber.iq.roster.item();
                    item.jid = contact.jid;
                    item.subscription = Tags.jabber.iq.roster.item.subscriptionEnum.remove;
                    query.Add(item);
                    iq.Add(query);

                    Frontend.Backend.SendTag(account.jid, iq);
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void EditContact(Account account, XMPP.JID jid, string alias = "")
        {
            try
            {
                if (account != null)
                {
                    var iq = new Tags.jabber.client.iq();
                    iq.from = account.CurrentJID;
                    iq.type = Tags.jabber.client.iq.typeEnum.set;
                    var query = new Tags.jabber.iq.roster.query();
                    var item = new Tags.jabber.iq.roster.item();
                    item.jid = jid.Bare;
                    item.name = alias;
                    query.Add(item);
                    iq.Add(query);

                    Frontend.Backend.SendTag(account.jid, iq);
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static void CreateContact(Account account, XMPP.JID jid, string alias = "")
        {
            try
            {
                if (account != null)
                {
                    var contact = account.Roster[jid.Bare];
                    if (contact == null)
                        contact = account.Roster.CreateContact(account.jid, jid.Bare);

                    if (contact == null)
                        return;

                    var iq = new Tags.jabber.client.iq();
                    iq.from = account.CurrentJID;
                    iq.type = Tags.jabber.client.iq.typeEnum.set;
                    var query = new Tags.jabber.iq.roster.query();
                    var item = new Tags.jabber.iq.roster.item();
                    item.jid = jid.Bare;
                    if (!string.IsNullOrEmpty(alias))
                        item.name = alias;
                    query.Add(item);
                    iq.Add(query);

                    Frontend.Backend.SendTag(account.jid, iq);

                    Subscribe(contact);
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        public static Tags.jabber.client.message SendMessage(string from, string to, string content)
        {
            try
            {
                var account = Frontend.Accounts[from];
                if (account != null)
                {
                    var message = new Tags.jabber.client.message();
                    message.to = to;
                    message.from = account.CurrentJID;
                    message.type = Tags.jabber.client.message.typeEnum.chat;

                    var body = new Tags.jabber.client.body();
                    body.Value = content;
                    message.Add(body);

                    message.Timestamp = DateTime.Now;
                    message.Account = from;

                    Frontend.Backend.SendTag(from, message);
                    return message;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return null;
        }

        public static void PublishAvatar(string filetype, byte[] image)
        {
            try
            {
                foreach (var account in Frontend.Accounts.Enabled)
                {
                    if (account.CurrentVCard != null)
                    {
                        var iq = new Tags.jabber.client.iq();
                        iq.from = account.CurrentJID;
                        iq.type = Tags.jabber.client.iq.typeEnum.set;

                        var vcard = new Tags.vcard_temp.vCard();

                        foreach (var element in account.CurrentVCard.Elements())
                        {
                            if (element.Name != XName.Get("PHOTO", "vcard-temp") &&     // I know how ugly this is, but I have to get this done~!
                                element.Name != XName.Get("TYPE", "vcard-temp") &&
                                element.Name != XName.Get("BINVAL", "vcard-temp"))
                                vcard.Add(element);
                        }

                        var photo = new XElement(XName.Get("PHOTO", "vcard-temp"));
                        photo.Add(new XElement(XName.Get("TYPE", "vcard-temp"), filetype));
                        photo.Add(new XElement(XName.Get("BINVAL", "vcard-temp"), System.Convert.ToBase64String(image)));
                        vcard.Add(photo);

                        iq.Add(vcard);

                        Frontend.Backend.SendTag(account.jid, iq);

                        account.OwnContact.SetAvatar(image);
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

    }
}
