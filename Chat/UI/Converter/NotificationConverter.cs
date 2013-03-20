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
using Chat.Frontend;
using Chat.UI.Flyout;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Chat.UI.Converter
{
    public sealed class NotificationTypeToColor : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var colors = Frontend.AppColors;

            try
            {
                NotificationType type = (NotificationType)value;

                switch (type)
                {
                    case NotificationType.Error:
                        return colors.HighlightImportant;
                    case NotificationType.Informative:
                        return colors.HighlightWarning;
                    case NotificationType.Request:
                        return colors.HighlightRequest;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return colors.HighlightImportant;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class ImportanceToHeaderColorConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var colors = Frontend.AppColors;

            try
            {
                bool notify = (bool)value;

                if (notify)
                    return colors.HighlightImportant;
                else
                    return colors.FrameForeground;

            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return colors.FrameForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    

    public sealed class NotificationTypeToText : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                NotificationType type = (NotificationType)value;
                return Helper.Translate("NotificationType" + type.ToString());
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class NotificationToButtonVisibility : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var notification = value as Notification;
                if (notification != null)
                {
                    if (notification.Action == FlyoutType.AccountListEdit ||
                        notification.Action == FlyoutType.Subscription)
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


    public sealed class NotificationToButtonText : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var notification = value as Notification;
                if (notification != null)
                {
                    if (notification.Action == FlyoutType.AccountListEdit ||
                        notification.Action == FlyoutType.Subscription)
                    {
                        return Helper.Translate("ButtonText" + notification.Action.ToString());
                    }
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }   
}
