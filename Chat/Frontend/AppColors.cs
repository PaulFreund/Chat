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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chat
{
    public class AppColors : INotifyPropertyChanged
    {
        private App Frontend { get { return (App)App.Current; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ReadFrom(Colors colors)
        {
            if (colors == null)
                return;

            FrameForeground = colors.FrameForeground;
            FrameBackground = colors.FrameBackground;
            FrameSecondary = colors.FrameSecondary;

            ContentForeground = colors.ContentForeground;
            ContentBackground = colors.ContentBackground;
            ContentSecondary = colors.ContentSecondary;
            ContentPopout = colors.ContentPopout;
            ContentEnabled = colors.ContentEnabled;
            ContentDisabled = colors.ContentDisabled;

            ContactListBackground = colors.ContactListBackground;
            ContactListForeground = colors.ContactListForeground;
            ContactListSelected = colors.ContactListSelected;

            HighlightForeground = colors.HighlightForeground;
            HighlightImportant = colors.HighlightImportant;
            HighlightWarning = colors.HighlightWarning;
            HighlightRequest = colors.HighlightRequest;

            StatusAvailable = colors.StatusAvailable;
            StatusAway = colors.StatusAway;
            StatusDnd = colors.StatusDnd;
            StatusOffline = colors.StatusOffline;
        }

        public void WriteTo(Colors colors)
        {
            colors.FrameForeground = FrameForeground;
            colors.FrameBackground = FrameBackground;
            colors.FrameSecondary = FrameSecondary;

            colors.ContentForeground = ContentForeground;
            colors.ContentBackground = ContentBackground;
            colors.ContentSecondary = ContentSecondary;
            colors.ContentPopout = ContentPopout;
            colors.ContentEnabled = ContentEnabled;
            colors.ContentDisabled = ContentDisabled;

            colors.ContactListBackground = ContactListBackground;
            colors.ContactListForeground = ContactListForeground;
            colors.ContactListSelected = ContactListSelected;

            colors.HighlightForeground = HighlightForeground;
            colors.HighlightImportant = HighlightImportant;
            colors.HighlightWarning = HighlightWarning;
            colors.HighlightRequest = HighlightRequest;

            colors.StatusAvailable = StatusAvailable;
            colors.StatusAway = StatusAway;
            colors.StatusDnd = StatusDnd;
            colors.StatusOffline = StatusOffline;
        }

        protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_lockUpdates)
                return;

            try
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                Frontend.UIError(ex);
            }
        }

        public void SetDefault()
        {
            FrameForeground = DefaultColors.FrameForeground;
            FrameBackground = DefaultColors.FrameBackground;
            FrameSecondary = DefaultColors.FrameSecondary;

            ContentForeground = DefaultColors.ContentForeground;
            ContentBackground = DefaultColors.ContentBackground;
            ContentSecondary = DefaultColors.ContentSecondary;
            ContentPopout = DefaultColors.ContentPopout;
            ContentEnabled = DefaultColors.ContentEnabled;
            ContentDisabled = DefaultColors.ContentDisabled;

            ContactListBackground = DefaultColors.ContactListBackground;
            ContactListForeground = DefaultColors.ContactListForeground;
            ContactListSelected = DefaultColors.ContactListSelected;

            HighlightForeground = DefaultColors.HighlightForeground;
            HighlightImportant = DefaultColors.HighlightImportant;
            HighlightWarning = DefaultColors.HighlightWarning;
            HighlightRequest = DefaultColors.HighlightRequest;

            StatusAvailable = DefaultColors.StatusAvailable;
            StatusAway = DefaultColors.StatusAway;
            StatusDnd = DefaultColors.StatusDnd;
            StatusOffline = DefaultColors.StatusOffline;
        }

        private bool _lockUpdates = false;

        public AppColors()
        {
            SetDefault();
        }

        private string _frameForeground;
        private string _frameBackground;
        private string _frameSecondary;

        private string _contentForeground;
        private string _contentBackground;
        private string _contentSecondary;
        private string _contentPopout;
        private string _contentEnabled;
        private string _contentDisabled;

        private string _contactListBackground;
        private string _contactListForeground;
        private string _contactListSelected;

        private string _highlightForeground;
        private string _highlightImportant;
        private string _highlightWarning;
        private string _highlightRequest;

        private string _statusAvailable;
        private string _statusAway;
        private string _statusDnd;
        private string _statusOffline;


        public string FrameForeground { get { return _frameForeground; } set { _frameForeground = value; EmitPropertyChanged(); } }
        public string FrameBackground { get { return _frameBackground; } set { _frameBackground = value; EmitPropertyChanged(); } }
        public string FrameSecondary { get { return _frameSecondary; } set { _frameSecondary = value; EmitPropertyChanged(); } }

        public string ContentForeground { get { return _contentForeground; } set { _contentForeground = value; EmitPropertyChanged(); } }
        public string ContentBackground { get { return _contentBackground; } set { _contentBackground = value; EmitPropertyChanged(); } }
        public string ContentSecondary { get { return _contentSecondary; } set { _contentSecondary = value; EmitPropertyChanged(); } }
        public string ContentPopout { get { return _contentPopout; } set { _contentPopout = value; EmitPropertyChanged(); } }
        public string ContentEnabled { get { return _contentEnabled; } set { _contentEnabled = value; EmitPropertyChanged(); } }
        public string ContentDisabled { get { return _contentDisabled; } set { _contentDisabled = value; EmitPropertyChanged(); } }

        public string ContactListBackground { get { return _contactListBackground; } set { _contactListBackground = value; EmitPropertyChanged(); } }
        public string ContactListForeground { get { return _contactListForeground; } set { _contactListForeground = value; EmitPropertyChanged(); } }
        public string ContactListSelected { get { return _contactListSelected; } set { _contactListSelected = value; EmitPropertyChanged(); } }

        public string HighlightForeground { get { return _highlightForeground; } set { _highlightForeground = value; EmitPropertyChanged(); } }
        public string HighlightImportant { get { return _highlightImportant; } set { _highlightImportant = value; EmitPropertyChanged(); } }
        public string HighlightWarning { get { return _highlightWarning; } set { _highlightWarning = value; EmitPropertyChanged(); } }
        public string HighlightRequest { get { return _highlightRequest; } set { _highlightRequest = value; EmitPropertyChanged(); } }

        public string StatusAvailable { get { return _statusAvailable; } set { _statusAvailable = value; EmitPropertyChanged(); } }
        public string StatusAway { get { return _statusAway; } set { _statusAway = value; EmitPropertyChanged(); } }
        public string StatusDnd { get { return _statusDnd; } set { _statusDnd = value; EmitPropertyChanged(); } }
        public string StatusOffline { get { return _statusOffline; } set { _statusOffline = value; EmitPropertyChanged(); } }
    }
}
