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
using Chat.Frontend;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Flyout
{
    public sealed partial class RemoveContact : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }
        private Flyout flyoutSelf = null;
        private Contact CurrentContact = null;
        public RemoveContact(Flyout self, Backend.Data.Contact contact)
        {
            this.InitializeComponent();
            this.flyoutSelf = self;
            this.DataContext = contact;
            this.CurrentContact = contact;
        }

        private async void OnYes(object sender, RoutedEventArgs e)
        {
            if (CurrentContact != null)
            {
                await Frontend.RunAsync(() =>
                {
                    XMPPHelper.RemoveContact(CurrentContact);
                });
            }
            flyoutSelf.Hide();     
        }

        private void OnNo(object sender, RoutedEventArgs e)
        {
            flyoutSelf.Hide();            
        }
    }
}
