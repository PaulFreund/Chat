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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using XMPP;
using Tags = XMPP.tags;

namespace Backend.Data
{
    public class ConversationSender
    {
        public ConversationSender(string account, string sender)
        {
            Account = account;
            Sender = sender;
        }

        public string Account { get; set; }
        public string Sender { get; set; }
    }

    public class ConversationMessage
    {
        public ConversationMessage(DateTime timestamp, string body)
        {
            Timestamp = timestamp;
            Body = body;
        }

        public DateTime Timestamp { get; private set; }
        public string Body { get; private set; }
    }

    public class ConversationItem
    {
        public ConversationItem(ConversationSender sender, JID identifier, DateTime timestamp)
        {
            Sender = sender;
            Identifier = identifier;
            Timestamp = timestamp;
            Messages = new ObservableCollection<ConversationMessage>();
        }

        public ConversationSender Sender { get; private set; }
        public JID Identifier { get; private set; }
        public DateTime Timestamp { get; private set; }

        public ObservableCollection<ConversationMessage> Messages { get; private set; }

        public void AddMessage(Tags.jabber.client.message message)
        {
            foreach (var body in message.bodyElements) // Add all body elements 
                Messages.Add(new ConversationMessage(message.Timestamp, body.Value));
        }
    }

    public class Conversation 
    {
        public Conversation(string self, string other)
        {
            Self = self;
            Other = other;

            Items = new ObservableCollection<ConversationItem>(); 
        }

        public string Self { get; private set; }
        public string Other { get; private set; }

        public ObservableCollection<ConversationItem> Items {get; private set; }

        public void AddMessage(Tags.jabber.client.message message)
        {
            JID senderJid = new JID(message.from);
            string sender = null;
            
            // Get the contact the message is from
            if( senderJid.Bare == Self )
                sender = Self;
            else if( senderJid.Bare == Other )
                sender = Other;
            else
                return;

            // The conversationitem we will be adding items to
            ConversationItem current = null;

            // Look if we have to add a new item or reuse an old one
            if (Items.Count > 0)
            {
                ConversationItem last = Items.First();
                if (last != null && last.Identifier == senderJid) // The last item is from the same sender as this item
                {
                    if (last.Messages.Count > 0) // It has messages
                    {
                        ConversationMessage lastMessage = last.Messages.Last();
                        if ((message.Timestamp - lastMessage.Timestamp).Minutes < 1) // The last messag is no older than two minutes
                            current = last;
                    }
                    else // It has no messages, we don't know why but we should add our items to it because an emtpy item looks bad
                    {
                        current = last;
                    }
                }
            }

            if (current == null)
            {
                current = new ConversationItem(new ConversationSender(Self, sender), senderJid, message.Timestamp);
                Items.Insert(0, current);
            }

            current.AddMessage(message);
        }

    }
}
