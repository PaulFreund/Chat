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
using Windows.UI.Xaml.Media;
using XMPP.tags.jabber.iq.roster;

namespace Chat.UI.Converter
{
    public sealed class ContactStatusToColor : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            try
            {
                StatusType status = (StatusType)value;
                switch (status)
                {
                    case StatusType.Available:
                        return Frontend.Resources["StatusBrushAvailable"] as LinearGradientBrush;
                    case StatusType.Away:
                        return Frontend.Resources["StatusBrushAway"] as LinearGradientBrush;
                    case StatusType.Busy:
                        return Frontend.Resources["StatusBrushBusy"] as LinearGradientBrush;
                    case StatusType.Offline:
                        return Frontend.Resources["StatusBrushOffline"] as LinearGradientBrush;
                }
                
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Frontend.Resources["StatusBrushOffline"] as SolidColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class ContactSubscriptionVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    if ((string)value == "from" || (string)value == "to")
                        return Visibility.Visible;
                }
                else
                {
                    if ((item.subscriptionEnum)value != item.subscriptionEnum.both)
                        return Visibility.Visible;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }


            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class ContactVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    if ((bool)value)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    if (value is bool)
                    {
                        if ((bool)value || Frontend.Settings.showOffline)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }
}
