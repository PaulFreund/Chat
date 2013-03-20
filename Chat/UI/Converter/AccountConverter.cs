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

namespace Chat.UI.Converter
{

    public sealed class AccountStateToBooleanConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var state = (AccountState)value;
                if (state == AccountState.Enabled)
                    return true;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if ((bool)value == true)
                    return AccountState.Enabled;
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return AccountState.Disabled;
        }
    }

    public sealed class AccountCountToVisibilityConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var accounts = value as Accounts;
                if (accounts != null)
                {
                    if (!accounts.IsFull)
                        return Visibility.Visible;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }


            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class AccountCountToVisibilityInvertedConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var accounts = value as Accounts;
                if (accounts != null)
                {
                    if (accounts.IsFull)
                        return Visibility.Visible;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }


            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class AccountToColorConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value != null && Frontend.Accounts != null)
                {
                    var account = Frontend.Accounts[(string)value];
                    if (account != null)
                        return account.color;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return "DarkGray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class AccountJIDToTitleConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value != null && Frontend.Accounts != null)
                {
                    var account = Frontend.Accounts[(string)value];
                    if (account != null)
                        return account.title;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }
}
