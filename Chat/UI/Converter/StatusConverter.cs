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
using Windows.UI.Xaml.Data;

namespace Chat.UI.Converter
{
    public sealed class StatusMessageAdapter : IValueConverter
    {
        private App Frontend { get { return (App)App.Current; } }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                string message = value as string;
                if (!string.IsNullOrEmpty(message))
                    return message;

                if (Frontend.Status != null)
                    return StatusToMessage(Frontend.Status.status);
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }

        private string StatusToMessage(StatusType type)
        {
            var statusString = Helper.Translate("StatusType" + type.ToString());

            if (!string.IsNullOrEmpty(statusString))
                return statusString;

            return type.ToString();

        }
    }
}
