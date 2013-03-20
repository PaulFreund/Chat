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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Chat.UI.Controls
{
    public sealed partial class ConversationHeader : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }

        private bool _notify = false;
        public bool Notify 
        { 
            get 
            {
                return _notify;
            } 
            set 
            {
                _notify = value;

                var colors = Frontend.AppColors;

                if (_notify)
                    BackButton.Foreground = new SolidColorBrush(Helper.GetColorFromHexString(colors.HighlightImportant));
                else
                    BackButton.Foreground = new SolidColorBrush(Helper.GetColorFromHexString(colors.FrameForeground));
            } 
        }

        public ConversationHeader()
        {
            try
            {
                this.InitializeComponent();
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) { return; }

                this.DataContext = null;

                Frontend.Events.OnRosterContactSelected += Events_OnRosterItemSelected;

                Frontend.AppColors.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == "FrameForeground" || e.PropertyName == "HighlightImportant")
                        Notify = Notify; 
                };
                
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }

        }

        private async void Events_OnRosterItemSelected(object sender, Frontend.ContactSelectedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    if (e.Contact != null)
                        this.DataContext = e.Contact;
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }

        private async void OnBackClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await Frontend.RunAsync(() =>
                {
                    Frontend.Events.DeselectContact();
                });
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }
    }
}
