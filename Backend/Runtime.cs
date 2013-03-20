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
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using XMPP;
using Tags = XMPP.tags;

namespace Backend
{
    public class BackendInterface
    {
        // Own Object
        private readonly Runtime _backendRuntime;
        public BackendInterface(Runtime backendRuntime) { _backendRuntime = backendRuntime; }

        // Background Access
        public void RequestBackgroundAccess() { _backendRuntime.RequestBackgroundAccess(); }

        // Connections
        public void SendTag(string id, Tags.Tag tag) { if (_backendRuntime._connections != null) { _backendRuntime._connections.Send(id, tag); } }
        public void UpdateConnections() { if (_backendRuntime._connections != null) { _backendRuntime._connections.Update(); _backendRuntime._notifier.UpdateBadge(); } }

        // UI Connection
        public void SetCoreWindow(CoreWindow window) { if (_backendRuntime != null) { _backendRuntime.SetCoreWindow(window); } }

        // Events
        public void ClearEvents() { if(_backendRuntime._events != null ) {_backendRuntime._events.Clear(); } }
        public void RestoreEvents() { if (_backendRuntime._events != null) { _backendRuntime._events.Restore(); } }

        public BackendEvent GetNextEvent() { if (_backendRuntime._events != null) { return _backendRuntime._events.Dequeue(); } else { return null; } }

        public event Events.EventNotification OnNewEvent { add { if (_backendRuntime._events != null) { _backendRuntime._events.OnEvent += value; } } remove { if (_backendRuntime._events != null) { _backendRuntime._events.OnEvent -= value; } } }
    }

    public class Runtime : IDisposable
    {
        public const int _eventDelayMS = 3000;

        public Runtime()
        {
            _interface = new BackendInterface(this);
            _events = new Events();

            CoreApplication.Exiting += OnApplicationExiting;
            CoreApplication.Resuming += OnApplicationResuming;
            CoreApplication.Suspending += OnApplicationSuspending;

            if (_hasBackgroundAccess)
                Init();
            else
                PushEvent(RequestType.BackgroundAccess);
        }

        #region publicproperties

        public static BackendInterface Interface  { get { return Instance._interface; } }

        // Get singleton Runtime instance
        private static object instanceLock = new object();
        public static Runtime Instance
        {
            get
            {
                lock (instanceLock)
                {
                    CoreApplication.IncrementApplicationUseCount();

                    if (CoreApplication.Properties.ContainsKey("Instance"))
                    {
                        return (Runtime)CoreApplication.Properties["Instance"];
                    }
                    else
                    {
                        CoreApplication.Properties.Add("Instance", new Runtime());
                        return (Runtime)CoreApplication.Properties["Instance"];
                    }
                }
            }
        }

        #endregion

        #region privateproperties

        internal Events _events = null;
        internal Connections _connections = null;
        internal Notifier _notifier = null;
        internal BackendInterface _interface = null;

        private bool _isInitialized = false;
        private ManualResetEvent _settingMutex = new ManualResetEvent(true);
        private bool _hasBackgroundAccess { get { return (BackgroundExecutionManager.GetAccessStatus() == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity); } }
        private bool _hasRegisteredSystemEvents  { get { return (BackgroundTaskRegistration.AllTasks.Count >= 9); } } // 9 is the current count of background tasks we use apart from ControlChannels


        #endregion

        #region internalmethods

        internal void SetCoreWindow(CoreWindow window)
        {
            if (_notifier != null && window != null)
                _notifier.CoreWindow = window;

            if (_events != null && window != null && window.Dispatcher != null)
                _events.UIDispatcher = window.Dispatcher;
        }

        #endregion

        #region privatemethods

        internal void RequestBackgroundAccess()
        {
            if (_hasBackgroundAccess)
                return;

            try
            {
                BackgroundExecutionManager.RequestAccessAsync().Completed += CompletedBackgroundRequest;
            }
            catch
            {
                PushEvent(ErrorType.RequestBackgroundAccess, ErrorPolicyType.Severe);
                return;
            }
        }

        private bool Init()
        {
            if (_isInitialized)
                return true;

            if( _hasBackgroundAccess )
            {
                if (!_hasRegisteredSystemEvents)
                {
                    var success = RegisterSystemEvents();
                    if(!success)
                        return false;
                }

                _notifier = new Notifier(this);
                PushEvent(RequestType.CoreWindow);
                _connections = new Connections(this);
                _isInitialized = true;
                return true;
            }
            else
            {
#if DEBUG
                PushEvent(LogType.Error, "Backend init missing backgroundAccess");
#endif
                PushEvent(RequestType.BackgroundAccess);
                return false;
            }  
        }

        private bool RegisterSystemEvents()
        {
            if (_hasBackgroundAccess)
            {
                if (!UnregisterSystemEvents())
                    return false;

                try
                {
                    #region SystemEvents

                    // ControlChannel reset
                    BackgroundTaskBuilder controlChannelReset = new BackgroundTaskBuilder();
                    controlChannelReset.Name = "ControlChannelReset";
                    controlChannelReset.TaskEntryPoint = "BackgroundTasks.ControlChannelReset";
                    controlChannelReset.SetTrigger(new SystemTrigger(SystemTriggerType.ControlChannelReset, false));
                    controlChannelReset.Register();

                    // Internet available
                    BackgroundTaskBuilder internetAvailable = new BackgroundTaskBuilder();
                    internetAvailable.Name = "InternetAvailable";
                    internetAvailable.TaskEntryPoint = "BackgroundTasks.InternetAvailable";
                    internetAvailable.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                    internetAvailable.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    internetAvailable.Register();

                    // Internet not available
                    BackgroundTaskBuilder internetNotAvailable = new BackgroundTaskBuilder();
                    internetNotAvailable.Name = "InternetNotAvailable";
                    internetNotAvailable.TaskEntryPoint = "BackgroundTasks.InternetNotAvailable";
                    internetNotAvailable.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                    internetNotAvailable.AddCondition(new SystemCondition(SystemConditionType.InternetNotAvailable));
                    internetNotAvailable.Register();

                    // Session connected
                    BackgroundTaskBuilder sessionConnected = new BackgroundTaskBuilder();
                    sessionConnected.Name = "SessionConnected";
                    sessionConnected.TaskEntryPoint = "BackgroundTasks.SessionConnected";
                    sessionConnected.SetTrigger(new SystemTrigger(SystemTriggerType.SessionConnected, false));
                    sessionConnected.Register();

                    // User Away
                    BackgroundTaskBuilder userAway = new BackgroundTaskBuilder();
                    userAway.Name = "UserAway";
                    userAway.TaskEntryPoint = "BackgroundTasks.UserAway";
                    userAway.SetTrigger(new SystemTrigger(SystemTriggerType.UserAway, false));
                    userAway.Register();

                    // User present
                    BackgroundTaskBuilder userPresent = new BackgroundTaskBuilder();
                    userPresent.Name = "UserPresent";
                    userPresent.TaskEntryPoint = "BackgroundTasks.UserPresent";
                    userPresent.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
                    userPresent.Register();

                    // LockScreen application added
                    BackgroundTaskBuilder lockScreenApplicationAdded = new BackgroundTaskBuilder();
                    lockScreenApplicationAdded.Name = "LockScreenApplicationAdded";
                    lockScreenApplicationAdded.TaskEntryPoint = "BackgroundTasks.LockScreenApplicationAdded";
                    lockScreenApplicationAdded.SetTrigger(new SystemTrigger(SystemTriggerType.LockScreenApplicationAdded, false));
                    lockScreenApplicationAdded.Register();

                    // LockScreen application removed
                    BackgroundTaskBuilder lockScreenApplicationRemoved = new BackgroundTaskBuilder();
                    lockScreenApplicationRemoved.Name = "LockScreenApplicationRemoved";
                    lockScreenApplicationRemoved.TaskEntryPoint = "BackgroundTasks.LockScreenApplicationRemoved";
                    lockScreenApplicationRemoved.SetTrigger(new SystemTrigger(SystemTriggerType.LockScreenApplicationRemoved, false));
                    lockScreenApplicationRemoved.Register();

                    // TimeZone change
                    BackgroundTaskBuilder timeZoneChange = new BackgroundTaskBuilder();
                    timeZoneChange.Name = "TimeZoneChange";
                    timeZoneChange.TaskEntryPoint = "BackgroundTasks.TimeZoneChange";
                    timeZoneChange.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
                    timeZoneChange.Register();

                    #endregion
                }
                catch
                {
                    PushEvent(ErrorType.RegisterSystemEvents, ErrorPolicyType.Severe);
                    return false;
                }
#if DEBUG
                PushEvent(LogType.Info, "Systemevents registered");
#endif
                return true;
            }
            else
            {
#if DEBUG
                PushEvent(LogType.Error, "RegisterSystemEvents missing backgroundaccess");
#endif
                PushEvent(RequestType.BackgroundAccess);
                return false;
            }
        }

        private bool UnregisterSystemEvents()
        {
            try
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (!task.Value.Name.StartsWith("PN") && !task.Value.Name.StartsWith("KA"))
                        task.Value.Unregister(true);                    
                }
            }
            catch
            {
                PushEvent(ErrorType.UnregisterSystemEvents, ErrorPolicyType.Severe);
                return false;
            }
#if DEBUG
            PushEvent(LogType.Info, "Unregistered system events");
#endif
            return true;
        }

        #endregion

        #region eventhandler

        private void OnEvent(BackendEvent newEvent) 
        {
            if (newEvent is BackendEventError)
            {
                var error = newEvent as BackendEventError;

                if (error.Policy == ErrorPolicyType.Deactivate)
                {
                    try
                    {
                        var accounts = new Accounts();
                        var account = accounts[error.Id];
                        if (account != null)
                        {
                            account.persistantState = AccountState.Disabled;
                            account.forceDisabled = true;
                        }
                    }
                    catch { }
                }
            }

            if (_notifier != null)
            {
                // Returns true if user relevant
                if (_notifier.Push(newEvent))
                    newEvent.Persist = true;
            }

            _events.Enqueue(newEvent); 
        }


        private void CompletedBackgroundRequest(Windows.Foundation.IAsyncOperation<BackgroundAccessStatus> asyncInfo, Windows.Foundation.AsyncStatus asyncStatus)
        {
            OnBackgroundStatusChanged();
        }

        private void OnBackgroundStatusChanged()
        {
            BackgroundAccessStatus status = BackgroundExecutionManager.GetAccessStatus();
            switch (status)
            {
                case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
                    Init();
                    break;
                case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
                    Init();
                    break;
                case BackgroundAccessStatus.Unspecified:
                    RequestBackgroundAccess();
                    break;
                case BackgroundAccessStatus.Denied:
                    PushEvent(ErrorType.RequestBackgroundAccess, ErrorPolicyType.Severe);
                    break;
            }
        }

        internal void OnConnectionEvent(object sender, ConnectionEvent e)
        {
            OnEvent(e.Event);
        }

        private void OnApplicationSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            PushEvent(WindowsType.ApplicationSuspending);
        }

        private void OnApplicationResuming(object sender, object e)
        {
            PushEvent(WindowsType.ApplicationResuming);
        }

        private void OnApplicationExiting(object sender, object e)
        {
            PushEvent(WindowsType.ApplicationExiting);
        }

        public async void OnBackgroundTaskRunning(IBackgroundTaskInstance instance)
        {
            var defferal = instance.GetDeferral();
            if (instance.Task.Name.StartsWith("PN"))
            {
                var serverId = instance.Task.Name.Substring(2);

                if( _connections != null )
                    _connections.WaitProcessing(serverId);
                return;
            }

            if (instance.Task.Name.StartsWith("KA"))
                return;

            switch (instance.Task.Name)
            {
                case "ControlChannelReset":
                    await Task.Delay(_eventDelayMS);
                    PushEvent(WindowsType.ControlChannelReset);
                    _connections.Update();
                    break;

                case "InternetAvailable":
                    await Task.Delay(_eventDelayMS);
                    PushEvent(WindowsType.InternetAvailable);
                    _connections.Update();
                    break;

                case "InternetNotAvailable":
                    await Task.Delay(_eventDelayMS);
                    PushEvent(WindowsType.InternetNotAvailable);
                    _connections.Update();
                    break;

                case "ServicingComplete":
                    PushEvent(WindowsType.ServicingComplete);
                    break;

                case "SessionConnected":
                    await Task.Delay(_eventDelayMS);
                    PushEvent(WindowsType.SessionConnected);
                    _connections.Update();
                    break;

                case "UserAway":
                    PushEvent(WindowsType.UserAway);
                    break;

                case "UserPresent":
                    PushEvent(WindowsType.UserPresent);
                    break;

                case "LockScreenApplicationAdded":
                    PushEvent(WindowsType.LockScreenApplicationAdded);
                    OnBackgroundStatusChanged();
                    break;

                case "LockScreenApplicationRemoved":
                    PushEvent(WindowsType.LockScreenApplicationRemoved);
                    OnBackgroundStatusChanged();
                    break;

                case "TimeZoneChange":
                    PushEvent(WindowsType.TimeZoneChange);
                    break;
            }

            defferal.Complete();

        }

        public void OnBackgroundTaskCanceled(IBackgroundTaskInstance instance, string name, BackgroundTaskCancellationReason reason)
        {
            var canceled = true;
            var strreason = reason.ToString();
            switch (name)
            {
                case "ControlChannelReset": PushEvent(WindowsType.ControlChannelReset, canceled, strreason); break;
                case "InternetAvailable": PushEvent(WindowsType.InternetAvailable, canceled, strreason); break;
                case "InternetNotAvailable": PushEvent(WindowsType.InternetNotAvailable, canceled, strreason); break;
                case "ServicingComplete": PushEvent(WindowsType.ServicingComplete, canceled, strreason); break;
                case "SessionConnected": PushEvent(WindowsType.SessionConnected, canceled, strreason); break;
                case "UserAway": PushEvent(WindowsType.UserAway, canceled, strreason); break;
                case "UserPresent": PushEvent(WindowsType.UserPresent, canceled, strreason); break;
                case "LockScreenApplicationAdded": PushEvent(WindowsType.LockScreenApplicationAdded, canceled, strreason); break;
                case "LockScreenApplicationRemoved": PushEvent(WindowsType.LockScreenApplicationRemoved, canceled, strreason); break;
                case "TimeZoneChange": PushEvent(WindowsType.TimeZoneChange, canceled, strreason); break;
            }
        }

        #endregion

        #region helper

        private void PushEvent(Tags.Tag tag) { OnEvent(new BackendEventMessage("", tag)); }
        private void PushEvent(WindowsType windowsType, bool canceled = false, string reason = "") { OnEvent(new BackendEventWindows("", windowsType, canceled, reason)); }
        private void PushEvent(RequestType requestType) { OnEvent(new BackendEventRequest("", requestType)); }
        private void PushEvent(StateType stateType) { OnEvent(new BackendEventState("", stateType)); }
        private void PushEvent(LogType logType, string logMessage) { OnEvent(new BackendEventLog("", logType, logMessage)); }
        private void PushEvent(ErrorType errorType, ErrorPolicyType policy) { OnEvent(new BackendEventError("", errorType, policy)); }
        
        #endregion

        public void Dispose() { Dispose(true); }

        virtual protected void Dispose(bool managed)
        {
            _settingMutex.Dispose();
            _connections.Dispose();
            CoreApplication.Exiting -= OnApplicationExiting;
            CoreApplication.Resuming -= OnApplicationResuming;
            CoreApplication.Suspending -= OnApplicationSuspending;
        }
    }
}
