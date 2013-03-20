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
using Chat.Common;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace Chat.UI.Flyout
{
    public sealed partial class FlyoutControl : LayoutAwarePage
    {
        const int ContentAnimationOffset = 100;
        Flyout flyoutSelf = null;

        public FlyoutControl(Flyout self, FlyoutType type, object data = null)
        {
            this.InitializeComponent();
            flyoutSelf = self;

            // Transition
            Content.Transitions = new TransitionCollection();
            Content.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            // Title
            FlyoutTitle.Text = Helper.Translate("FlyoutType" + type.ToString());
            
            // Type switch
            switch(type)
            {
                case FlyoutType.About:              { FlyoutContent.Child = new About(self); break; }
                case FlyoutType.AccountEdit:        { FlyoutContent.Child = new AccountEdit(self, data); break; }
                case FlyoutType.AccountListEdit:    { FlyoutContent.Child = new AccountListEdit(self); break; } 
                case FlyoutType.AddContact:         { FlyoutContent.Child = new AddContact(self); break; }
                case FlyoutType.EditContact:        { FlyoutContent.Child = new EditContact(self, (Backend.Data.Contact)data); break; }
                case FlyoutType.Notifications:      { FlyoutContent.Child = new Notifications(self); break; }
                case FlyoutType.Subscription:       { FlyoutContent.Child = new Subscription(self, (Backend.Data.Contact)data); break; }
                case FlyoutType.RemoveContact:      { FlyoutContent.Child = new RemoveContact(self, (Backend.Data.Contact)data); break; }
                case FlyoutType.SettingsEdit:       { FlyoutContent.Child = new SettingsEdit(self); break; }
                case FlyoutType.StatusEdit:         { FlyoutContent.Child = new StatusEdit(self); break; }
                case FlyoutType.ThemeEdit:          { FlyoutContent.Child = new ThemeEdit(self); break; }
            }
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            flyoutSelf.ShowParent();
        }
    }
}


 





