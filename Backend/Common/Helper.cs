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
using System.Text;
using Windows.UI;
using Tags = XMPP.tags;

namespace Backend.Common
{
    public class Helper
    {
        public static byte OffsetColorValue(int value, int offset)
        {
            var diff = (255 - (value + offset));
            if (diff < 0 || diff > 255)
                offset *= (-1);

            return (byte)(value + offset);
        }

        public static Color GetColorFromHexString(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue) || hexValue[0] != '#' || hexValue.Length != 9)
                hexValue = "#00000000";

            hexValue = hexValue.Substring(1);

            var a = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var r = Convert.ToByte(hexValue.Substring(2, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(4, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        public static long UnixTimestampFromDateTime(DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            return unixTimestamp;
        }

        public static string EncodeBASE64(string utf8String)
        {
            if (utf8String != null)
            {
                var barebytes = Encoding.UTF8.GetBytes(utf8String);
                return Convert.ToBase64String(barebytes);
            }
            return string.Empty;
        }

        public static string DecodeBASE64(string base64string)
        {
            if (base64string != string.Empty)
            {
                var basebytes = Convert.FromBase64String(base64string);
                return Encoding.UTF8.GetString(basebytes, 0, basebytes.Length);
            }
            return string.Empty;
        }

        public static string GetErrorMessage(Tags.jabber.client.message message)
        {
            var error = message.Element<Tags.jabber.client.error>(Tags.jabber.client.Namespace.error);
            if (error != null)
                return GetErrorMessage(error);

            return Translate("UnknownError");

        }

        public static string GetErrorMessage(Tags.jabber.client.iq iq)
        {
            var error = iq.Element<Tags.jabber.client.error>(Tags.jabber.client.Namespace.error);
            if (error != null)
                return GetErrorMessage(error);

            return Translate("UnknownError");
        }

        public static string GetErrorMessage(Tags.jabber.client.presence presence)
        {
            var error = presence.Element<Tags.jabber.client.error>(Tags.jabber.client.Namespace.error);
            if (error != null)
                return GetErrorMessage(error);

            return Translate("UnknownError");

        }

        public static string GetErrorMessage(Tags.jabber.client.error error)
        {
            if (error != null && error.HasElements)
            {
                var text = error.Element<Tags.xmpp_stanzas.text>(Tags.xmpp_stanzas.Namespace.text);
                if (text != null)
                {
                    return text.Value;
                }
                else if (error.Elements().Count() > 0)
                {
                    var element = error.Elements().First();
                    if (element != null)
                        return element.Name.LocalName;
                }
            }

            return Translate("UnknownError");
        }

        public static string Translate(string id)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            return loader.GetString(id);
        }

        public static void PublishState(StatusType status, string message)
        {
            var accounts = new Accounts();
            foreach (var account in accounts.Enabled)
                PublishState(account.jid, status, message);
        }

        public static void PublishState(Tags.jabber.client.show.valueEnum statusValue, string message)
        {
            var accounts = new Accounts();
            foreach (var account in accounts.Enabled)
                PublishState(account.jid, statusValue, message);
        }

        public static void PublishState(string account, StatusType status, string message)
        {
            Tags.jabber.client.show.valueEnum statusValue = Tags.jabber.client.show.valueEnum.none;
            if (status == StatusType.Available) { statusValue = Tags.jabber.client.show.valueEnum.none; }
            if (status == StatusType.Away)      { statusValue = Tags.jabber.client.show.valueEnum.away; }
            if (status == StatusType.Busy)      { statusValue = Tags.jabber.client.show.valueEnum.dnd; }
            PublishState(account, statusValue, message);
        }

        public static void PublishState(string account, Tags.jabber.client.show.valueEnum statusValue, string message)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("[Frontend] Publishing State for: " + account);
#endif

            var presence = new Tags.jabber.client.presence();

            if (!string.IsNullOrEmpty(message))
            {
                var status = new Tags.jabber.client.status();
                status.Value = message;
                presence.Add(status);
            }

            if (statusValue != Tags.jabber.client.show.valueEnum.none)
            {
                var show = new Tags.jabber.client.show();
                show.Value = statusValue;
                presence.Add(show);
            }

            Runtime.Interface.SendTag(account, presence);
        }

        public static void RequestVCard(Account account)
        {
            var iq = new Tags.jabber.client.iq();

            if (!string.IsNullOrEmpty(account.CurrentJID.Bare))
                iq.from = account.CurrentJID;
            else
                return;

            iq.type = Tags.jabber.client.iq.typeEnum.get;

            var vcard = new Tags.vcard_temp.vCard();
            iq.Add(vcard);

            Runtime.Interface.SendTag(new XMPP.JID(account.jid).Bare, iq);
        }

        public static void RequestRoster(Account account)
        {
            var iq = new Tags.jabber.client.iq();

            if (!string.IsNullOrEmpty(account.CurrentJID.Bare))
                iq.from = account.CurrentJID;
            else
                return;

            iq.type = Tags.jabber.client.iq.typeEnum.get;

            var query = new Tags.jabber.iq.roster.query();
            iq.Add(query);

#if DEBUG
            System.Diagnostics.Debug.WriteLine("[Frontend] Requesting Roster for: " + account.CurrentJID.ToString() + " id " + iq.id);
#endif
            
            Runtime.Interface.SendTag(new XMPP.JID(account.jid).Bare, iq);
        }
    }
}
