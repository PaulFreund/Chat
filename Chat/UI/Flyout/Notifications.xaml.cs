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

using Chat.Frontend;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Flyout
{
    public sealed partial class Notifications : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }
        private Flyout flyoutSelf = null;

        public Notifications(Flyout self)
        {
            this.InitializeComponent();
            flyoutSelf = self;
            this.DataContext = Frontend.Notifications.NotificationList;
        }

        private void OnHide(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var notification = button.Tag as Notification;
                Frontend.Notifications.NotificationList.Remove(notification);
            }

            if (Frontend.Notifications.NotificationCount <= 0)
                flyoutSelf.Hide();
        }

        private void OnAction(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var notification = button.Tag as Notification;
                Frontend.Notifications.NotificationList.Remove(notification);
                
                if (notification.Action != FlyoutType.None)
                    new Flyout(notification.Action, notification.Data, flyoutSelf);
            }
        }
    }
}
