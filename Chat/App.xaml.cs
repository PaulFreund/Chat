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

using Backend;
using Backend.Common;
using Backend.Data;
using Chat.Frontend;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP;

namespace Chat
{
    public sealed partial class App : Application
    {
        public BackendInterface Backend { get { return Runtime.Interface; } }

        #region DataStores

        public Status Status = new Status();
        public Colors BackendColors = new Colors();
        public Settings Settings = new Settings();
        public Accounts Accounts = new Accounts();
        public Notifications Notifications = null;

        public AppColors AppColors
        {
            get { return Resources["AppColors"] as AppColors; }
        }

        #endregion

        #region EventAndMessageHandling

        public CoreWindow CoreWindow
        {
            get
            {
                try
                {
                    if (Window.Current != null && Window.Current.CoreWindow != null)
                        return Window.Current.CoreWindow;
                }
                catch (Exception uiEx) { UIError(uiEx); }

                return null;
            }
        }

        private class EmptyAsyncAction : IAsyncAction 
        {
            public Exception ErrorCode { get { return null; } }
            public uint Id { get { return 0; } }
            public AsyncStatus Status { get { return AsyncStatus.Completed; } }
            public void Cancel() { }
            public void Close() { }
            public AsyncActionCompletedHandler Completed { get { return null; } set { } }
            public void GetResults() {}
        }

        public IAsyncAction RunAsync(DispatchedHandler handler)
        {
            try
            {
                if (CoreWindow != null && CoreWindow.Dispatcher != null)
                {
                    return CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
                }
                else
                {
                    handler.Invoke();
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }

            return new EmptyAsyncAction();
        }

        public event EventHandler OnRequestBackgroundAccess;
        public Frontend.Events Events = new Frontend.Events();
        public Interpreter Interpreter = new Interpreter();

        #endregion

        #region Internal

        private Dictionary<string, DispatcherTimer> _loadingTimers = new Dictionary<string, DispatcherTimer>();

        #endregion

        public App()
        {
            try
            {
                this.InitializeComponent();
                this.UnhandledException += OnUnhandledException;

                Settings.PropertyChanged    += (sender, e) => Events.SettingsChanged();
                Accounts.CollectionChanged  += (sender, e) => Events.AccountListChanged();

                // Initialize our notifications
                Notifications = new Frontend.Notifications();

                // Delete old avatars
                Avatar.RemoveOld();

                // Delete invalid contacts
                foreach (var account in Accounts)
                {
                    account.Roster.CleanInvalid();
                    ResetAccountState(account.jid);
                }
                // Restore all old objects
                Backend.RestoreEvents();

                UpdateBackendConnection();
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        public void UIError(Exception e, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string name = null)
        {
            string displaypath = path.Substring(path.IndexOf("Chat"));

            string message = "Error from " + displaypath + " in function " + name + " on line " + lineNumber + ": " + e.Message + ". Please send to http://feedback.lvl3.org !";

            if( Notifications != null )
                Notifications.CreateError(ErrorPolicyType.Informative, "All", message);
        }

        #region AppStates

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                if (Window.Current != null)
                {
                    // Colors
                    AppColors.ReadFrom(BackendColors);
                    AppColors.PropertyChanged += (sender, e) => AppColors.WriteTo(BackendColors);

                    // Create new Frame and load the Mainview
                    Frame uiFrame = new Frame();
                    if (!uiFrame.Navigate(typeof(UI.Views.Main)))
                        Exit();

                    // Assign it to the current window
                    Window.Current.Content = uiFrame;

                    // (Re)Add
                    Window.Current.VisibilityChanged -= WindowVisibilityChanged;
                    Window.Current.VisibilityChanged += WindowVisibilityChanged;
                    Window.Current.Activated -= WindowSelected;
                    Window.Current.Activated += WindowSelected;

                    // Open The window
                    Window.Current.Activate();
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        void WindowSelected(object sender, WindowActivatedEventArgs e)
        {
            UpdateBackendConnection();
        }

        private void WindowVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            UpdateBackendConnection();
        }

        private void UpdateBackendConnection()
        {
            try
            {
                if (CoreWindow != null)
                {
                    // Pass UI to backend
                    Backend.SetCoreWindow(CoreWindow);

                    // (Re)Register at the backend
                    Backend.OnNewEvent -= OnBackendEvent;
                    Backend.OnNewEvent += OnBackendEvent;
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region BackendEventHandling

        private void OnBackendEvent(object sender, EventArgs e)
        {
            OnProcessBackendEvent();
        }

        private bool _blocked = false;
        private bool _newRequest = false;

        private void OnProcessBackendEvent()
        {
            try
            {
                if (_blocked)
                {
                    _newRequest = true;
                }
                else
                {
                    _blocked = true;
                    {
                        BackendEvent newEvent = null;

                        do
                        {
                            newEvent = GetBackendEvent(); 

                            if (newEvent == null)
                                break;

                            if (newEvent is BackendEventWindows)    OnBackendEventWindows   (newEvent as BackendEventWindows);
                            if (newEvent is BackendEventMessage)    OnBackendEventMessage   (newEvent as BackendEventMessage);
                            if (newEvent is BackendEventRequest)    OnBackendEventRequest   (newEvent as BackendEventRequest);
                            if (newEvent is BackendEventState)      OnBackendEventState     (newEvent as BackendEventState);
                            if (newEvent is BackendEventLog)        OnBackendEventLog       (newEvent as BackendEventLog);
                            if (newEvent is BackendEventError)      OnBackendEventError     (newEvent as BackendEventError);

                        }
                        while (newEvent != null);
                    }
                    _blocked = false;

                    if (_newRequest)
                    {
                        _newRequest = false;
                        OnProcessBackendEvent();
                    }
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private BackendEvent GetBackendEvent()
        {
            try
            {
                if (Backend != null)
                    return Backend.GetNextEvent();
                else
                    return null;
            }
            catch (Exception uiEx) { UIError(uiEx); }

            return null;
        }

        #endregion

        #region BackendEventHandler

        private void OnBackendEventWindows(BackendEventWindows windowsEvent)
        {
            try
            {
                if (Settings.autoAway)
                {
                    if (windowsEvent.WindowsType == WindowsType.UserPresent)
                        Status.autoAwayActive = false;

                    if (windowsEvent.WindowsType == WindowsType.UserAway && Status.status == StatusType.Available)
                        Status.autoAwayActive = true;
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private async void OnBackendEventMessage(BackendEventMessage message)
        {
            try
            {
                await RunAsync(() =>
                {
                    Interpreter.Process(message.Tag);
                });
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private void OnBackendEventRequest(BackendEventRequest request)
        {
            try
            {
                if (request.RequestType == RequestType.BackgroundAccess)
                    OnRequestBackgroundAccess(this, new EventArgs());

                if (request.RequestType == RequestType.CoreWindow)
                    UpdateBackendConnection();
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private async void OnBackendEventState(BackendEventState state)
        {
            try
            {
                await RunAsync(() =>
                {
                    var account = Accounts[state.Id];
                    if (account != null)
                    {
                        account.CurrentConnectionState = state.StateType;

                        CheckInvalidAccountStates();

                        // Request the Roster
                        switch (state.StateType)
                        {
                            case StateType.Connecting:
                                StartLoading(account.jid);
                                ResetAccountState(account.jid);
                                break;
                            case StateType.ResourceBound:
                                break;
                            case StateType.Running:
                                StartLoading(account.jid);
                                Helper.RequestRoster(account.CurrentJID);
                                Helper.RequestVCard(account.CurrentJID);
                                break;
                            case StateType.Disconnecting:
                                StopLoading(account.jid);
                                break;
                            case StateType.Disconnected:
                                StopLoading(account.jid);
                                ResetAccountState(account.jid);
                                break;
                        }
                    }
                });
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private void OnBackendEventLog(BackendEventLog log)
        {
            try
            {
#if DEBUG
                if (log.LogType != LogType.Debug)
                    System.Diagnostics.Debug.WriteLine(log.ToString());
#endif
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private void OnBackendEventError(BackendEventError error)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(error.ToString());
                Notifications.CreateError(error);
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        #endregion

        #region Helper

        private void StartLoading(string account)
        {
            try
            {
                var accountObj = Accounts[account];
                if (accountObj != null)
                {
                    // Create Timer
                    var timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(5);
                    timer.Tick += (o, e) =>
                    {
                        Status.SetLoading(account, false);
                        timer.Stop();
                    };

                    // Add it to list
                    if (!_loadingTimers.ContainsKey(account))
                        _loadingTimers.Add(account, timer);
                    else
                    {
                        _loadingTimers[account].Stop();
                        _loadingTimers[account] = timer;
                    }

                    _loadingTimers[account].Start();
                    Status.SetLoading(account, true);
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private void StopLoading(string account)
        {
            try
            {
                var accountObj = Accounts[account];
                if (accountObj != null)
                {
                    // Add it to list
                    if (_loadingTimers.ContainsKey(account))
                        _loadingTimers[account].Stop();

                    Status.SetLoading(account, false);
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        private void CheckInvalidAccountStates()
        {
            if (Status.status != StatusType.Offline)
            {
                foreach (var acc in Accounts)
                {
                    if (acc.forceDisabled)
                    {
                        Status.HasInvalidAccounts = true;
                        return;
                    }

                    if (acc.persistantState == AccountState.Enabled && acc.CurrentConnectionState != StateType.Running)
                    {
                        Status.HasInvalidAccounts = true;
                        return;
                    }
                }
            }

            Status.HasInvalidAccounts = false;
        }

        private void ResetAccountState(string account)
        {
            try
            {
                var accountObj = Accounts[account];
                if (accountObj != null)
                {
                    accountObj.OwnResource = string.Empty;

                    var roster = accountObj.Roster;
                    if (roster != null)
                    {
                        foreach (var contact in roster)
                        {
                            contact.LockUpdates();
                            contact.ClearResources();
                            contact.UnlockUpdates();
                        }
                    }
                }
            }
            catch (Exception uiEx) { UIError(uiEx); }
        }

        #endregion
    }
}
