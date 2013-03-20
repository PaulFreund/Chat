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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Chat.UI.Converter
{
    public class MessageTextAlignChooser : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var settings = Frontend.Settings;

            try
            {
                if (settings.invertOwnMessages)
                {
                    var sender = (Backend.Data.Contact)value;
                    if (sender != null)
                    {
                        if (sender.jid == sender.account) // This is the account
                            return TextAlignment.Left;
                        else
                            return TextAlignment.Right;
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
            
            return TextAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public class MessageFlowDirectionChooser : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var settings = Frontend.Settings;

            try
            {
                if (settings.invertOwnMessages)
                {
                    var sender = value as Backend.Data.ConversationSender;
                    if (sender != null)
                    {
                        string contact = sender.Sender;
                        string account = sender.Account;
                        if (!string.IsNullOrEmpty(contact) && !string.IsNullOrEmpty(account))
                        {
                            var accountObj = Frontend.Accounts[account];
                            if (account != null)
                            {
                                var contactObj = accountObj.Roster[contact];
                                if (contactObj != null)
                                {
                                    if (contactObj.jid == contactObj.account) // This is the account
                                        return FlowDirection.RightToLeft;
                                    else
                                        return FlowDirection.LeftToRight;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
            
            return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }


    public class JIDToImageConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var sender = value as Backend.Data.ConversationSender;
                if (sender != null)
                {
                    string contact = sender.Sender;
                    string account = sender.Account;
                    if (!string.IsNullOrEmpty(contact) && !string.IsNullOrEmpty(account))
                    {
                        var accountObj = Frontend.Accounts[account];
                        if (account != null)
                        {
                            var contactObj = accountObj.Roster[contact];
                            if (contactObj != null)
                                return contactObj.ImageData;
                        }
                    }
                }

                return Backend.Data.Avatar.GetFileURI("");
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }
    
}
