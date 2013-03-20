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
using Windows.UI.Xaml.Data;
using XMPP.tags.jabber.iq.roster;

/*
none:   the user does not have a subscription to the contact's presence, and the contact does not have a subscription to the user's presence; this is the default value, so if the subscription attribute is not included then the state is to be understood as "none" <=> Show that there is no accociation, 
to:     the user has a subscription to the contact's presence, but the contact does not have a subscription to the user's presence ( i can see him , but he cant see me ( he has no authorization from me ))                                                           <=> Indicate that he cant see me
from:   the contact has a subscription to the user's presence, but the user does not have a subscription to the contact's presence ( he can see me , but i cant see hin ( im not authorized ))                                                                         <=> Indicate that I'm not authorized and can't see his status
both:   the user and the contact have subscriptions to each other's presence (also called a "mutual subscription")
*/

namespace Chat.UI.Converter
{
    public sealed class SubscribeAskVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var contact = value as Contact;
                if (contact != null)
                {
                    if (contact.ask == item.askEnum.subscribe || contact.subscription == item.subscriptionEnum.from)
                        return Visibility.Visible;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class SubscribeRequestVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if ((Contact.SubscriptionRequestType)value == Contact.SubscriptionRequestType.Subscribe)
                    return Visibility.Visible;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class SubscribeFromVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var contact = value as Contact;
                if (contact != null)
                {
                    if (contact.subscription != item.subscriptionEnum.both &&
                        contact.subscription != item.subscriptionEnum.from &&
                        contact.subscriptionRequest != Contact.SubscriptionRequestType.Subscribe)
                    {
                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class SubscribeToVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var contact = value as Contact;
                if (contact != null)
                {
                    if (contact.subscription != item.subscriptionEnum.both && contact.subscription != item.subscriptionEnum.to)
                        return Visibility.Visible;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }
}