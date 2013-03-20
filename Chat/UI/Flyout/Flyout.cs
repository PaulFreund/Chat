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

using Windows.Foundation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace Chat.UI.Flyout
{
    public enum FlyoutType
    {
        None,
        About,
        AccountEdit,
        AccountListEdit,
        AddContact,
        EditContact,
        Notifications,
        Subscription,
        RemoveContact,
        SettingsEdit,
        StatusEdit,
        ThemeEdit
    }

    public class Flyout
    {
        public Flyout Parent = null;
        private Popup internalPopup = null;
        private bool SettingsPaneParent = false;

        static int SmallFlyoutSize = 346;
        static int BigFlyoutSize = 646;

        public Flyout(FlyoutType type, object data = null, Flyout parent = null, bool settingsPaneParent = false)
        {
            Parent = parent;
            SettingsPaneParent = settingsPaneParent;

            // Get bounds
            Rect WindowBounds = Window.Current.Bounds;
            double settingsWidth = GetFlyoutWidth(type);

            internalPopup = new Popup();
            internalPopup.Opened += OnPopupOpened;
            internalPopup.Closed += OnPopupClosed;


            // Popup settings
            internalPopup.IsLightDismissEnabled = true;
            internalPopup.Width = settingsWidth;
            internalPopup.Height = WindowBounds.Height;

            // Animations
            internalPopup.ChildTransitions = new TransitionCollection();
            internalPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? EdgeTransitionLocation.Right : EdgeTransitionLocation.Left
            });

            // FlyoutControl
            FlyoutControl ctlFlyout = new FlyoutControl(this, type, data);
            ctlFlyout.Width = settingsWidth;
            ctlFlyout.Height = WindowBounds.Height;
            internalPopup.Child = ctlFlyout;

            // Popup position
            internalPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (WindowBounds.Width - settingsWidth) : 0);
            internalPopup.SetValue(Canvas.TopProperty, 0);

            Show();

            if (Parent != null)
                Parent.Hide();
        }

        public void Show() 
        {
            if (internalPopup != null)
                internalPopup.IsOpen = true;
        }

        public void Hide()
        {
            if (internalPopup != null) 
                internalPopup.IsOpen = false;
        }

        public void ShowParent()
        {
            Hide();

            if (Parent != null)
                Parent.Show();
            else if( SettingsPaneParent && ApplicationView.Value != ApplicationViewState.Snapped)
                SettingsPane.Show();
        }

        // --------------------------------------------------

        private void OnPopupOpened(object sender, object e)
        {
            Window.Current.Activated += OnHostWindowActivated;
        }

        private void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnHostWindowActivated;
        }

        private void OnHostWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (internalPopup != null)
                    internalPopup.IsOpen = false;
            }
        }

        // --------------------------------------------------

        private int GetFlyoutWidth(FlyoutType type)
        {
            switch (type)
            {
                case FlyoutType.About: { return SmallFlyoutSize; }
                case FlyoutType.AccountEdit: { return SmallFlyoutSize; }
                case FlyoutType.AccountListEdit: { return SmallFlyoutSize; }
                case FlyoutType.AddContact: { return SmallFlyoutSize; }
                case FlyoutType.EditContact: { return SmallFlyoutSize; }
                case FlyoutType.Notifications: { return BigFlyoutSize; }
                case FlyoutType.Subscription: { return SmallFlyoutSize; }
                case FlyoutType.RemoveContact: { return SmallFlyoutSize; }
                case FlyoutType.SettingsEdit: { return SmallFlyoutSize; }
                case FlyoutType.StatusEdit: { return SmallFlyoutSize; }
                case FlyoutType.ThemeEdit: { return BigFlyoutSize; }
            }

            return SmallFlyoutSize;
        }
    }
}
