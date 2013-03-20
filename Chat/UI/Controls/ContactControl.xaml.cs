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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Chat.UI.Controls
{
    public sealed partial class ContactControl : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }

        public ContactControl()
        {
            this.InitializeComponent();
        }
        public static DependencyProperty ContactBackgroundProperty = DependencyProperty.Register("ContactBackground", typeof(Brush), typeof(ContactControl), null);
        public static DependencyProperty ContactForegroundProperty = DependencyProperty.Register("ContactForeground", typeof(Brush), typeof(ContactControl), null);
        public static DependencyProperty ContactImageProperty = DependencyProperty.Register("ContactImage", typeof(ImageSource), typeof(ContactControl), null);
        public static DependencyProperty ShowAccountProperty = DependencyProperty.Register("ShowAccount", typeof(bool), typeof(ContactControl), null);
        public static DependencyProperty AccountColorProperty = DependencyProperty.Register("AccountColor", typeof(Color), typeof(ContactControl), null);
        public static DependencyProperty ContactNameProperty = DependencyProperty.Register("ContactName", typeof(string), typeof(ContactControl), null);
        public static DependencyProperty ContactMessageProperty = DependencyProperty.Register("ContactMessage", typeof(string), typeof(ContactControl), null);
        public static DependencyProperty ContactStatusProperty = DependencyProperty.Register("ContactStatus", typeof(StatusType), typeof(ContactControl), null);

        public Brush ContactBackground { get { return (Brush)GetValue(ContactBackgroundProperty); } set { SetValue(ContactBackgroundProperty, value); } }
        public Brush ContactForeground { get { return (Brush)GetValue(ContactForegroundProperty); } set { SetValue(ContactForegroundProperty, value); } }
        public ImageSource ContactImage { get { return (ImageSource)GetValue(ContactImageProperty); } set { SetValue(ContactImageProperty, value); } }
        public bool ShowAccount { get { return (bool)GetValue(ShowAccountProperty); } set { SetValue(ShowAccountProperty, value); } }
        public Color AccountColor { get { return (Color)GetValue(AccountColorProperty); } set { SetValue(AccountColorProperty, value); } }
        public string ContactName { get { return (string)GetValue(ContactNameProperty); } set { SetValue(ContactNameProperty, value); } }
        public string ContactMessage { get { return (string)GetValue(ContactMessageProperty); } set { SetValue(ContactMessageProperty, value); } }
        public StatusType ContactStatus { get { return (StatusType)GetValue(ContactStatusProperty); } set { SetValue(ContactStatusProperty, value); } }


    }
}
