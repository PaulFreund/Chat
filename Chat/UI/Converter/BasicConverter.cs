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
using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Chat.UI.Converter
{

    public sealed class ColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {

                string colorText = string.Empty;
                string parameterText = string.Empty;

                if (value is string)
                    colorText = value as string;
                else if (value is SolidColorBrush)
                    colorText = ((SolidColorBrush)value).Color.ToString();

                if (parameter is string)
                    parameterText = parameter as string;

                if (parameterText.Length == 1)
                {
                    var color = Helper.GetColorFromHexString(colorText);

                    switch (parameterText)
                    {
                        case "A":
                            return color.A;
                        case "R":
                            return color.R;
                        case "G":
                            return color.G;
                        case "B":
                            return color.B;
                    }
                }
            }
            catch {}

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }

    }

    public sealed class OverlayColorPointerOver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            const int offset = 30;
            string colorText = string.Empty;

            if (value is string)
                colorText = value as string;
            else if (value is SolidColorBrush)
                colorText = ((SolidColorBrush)value).Color.ToString();

            var color = Helper.GetColorFromHexString(colorText);
            color.R = Helper.OffsetColorValue(color.R, offset);
            color.G = Helper.OffsetColorValue(color.G, offset);
            color.B = Helper.OffsetColorValue(color.B, offset);
            return color.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }

    }

    public sealed class OverlayColorPressed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            const int offset = -40;
            string colorText = string.Empty;

            if (value is string)
                colorText = value as string;
            else if (value is SolidColorBrush)
                colorText = ((SolidColorBrush)value).Color.ToString();

            var color = Helper.GetColorFromHexString(colorText);
            color.R = Helper.OffsetColorValue(color.R, offset);
            color.G = Helper.OffsetColorValue(color.G, offset);
            color.B = Helper.OffsetColorValue(color.B, offset);
            return color.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }

    }

    public sealed class OverlayColorDisabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            const int offset = -100;
            const int colorOffset = -10;
            string colorText = string.Empty;

            if (value is string)
                colorText = value as string;
            else if (value is SolidColorBrush)
                colorText = ((SolidColorBrush)value).Color.ToString();

            var color = Helper.GetColorFromHexString(colorText);

            color.A = Helper.OffsetColorValue(color.A, offset);
            color.R = Helper.OffsetColorValue(color.R, colorOffset);
            color.G = Helper.OffsetColorValue(color.G, colorOffset);
            color.B = Helper.OffsetColorValue(color.B, colorOffset);

            if (color.A < 100)
                color.A = 100;

            return color.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is string && !string.IsNullOrWhiteSpace((string)value)) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public sealed class CountToNotificationTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if ((int)value != 1)
                    return Helper.Translate("NewNotificationsMulti");
                else
                    return Helper.Translate("NewNotificationSingle");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class ExistanceToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public sealed class DateTimeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var dt = (DateTime)value;
                var culture = CultureInfo.CurrentCulture;
                var pattern = culture.DateTimeFormat.ShortTimePattern;
                return dt.ToString(pattern);
            }

            return string.Empty;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }


    public class BoolToFlowDirectionConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is bool)
                {
                    var boolValue = (bool)value;
                    if (boolValue)
                        return FlowDirection.RightToLeft;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }


            return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    public class BoolToEdgeConverter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is bool)
                {
                    var boolValue = (bool)value;
                    if (boolValue)
                        return EdgeTransitionLocation.Right;
                }
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return EdgeTransitionLocation.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }
}
