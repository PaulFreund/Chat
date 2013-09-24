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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using XMPP;
using XMPP.common;
using Tags = XMPP.tags;

namespace Backend.Common
{
    public class ConnectionParameters
    {
        public string Hostname = string.Empty;
        public string JID = string.Empty;
        public string Password = string.Empty;
        public bool UseSSL = false;
        public bool OldStyleSSL = false;
        public bool AuthPlain = false;
        public bool AuthMD5 = true;
        public bool AuthSCRAM = true;
        public bool AuthOAUTH2 = false;
        public int Port = 5222;
        public AccountState State = AccountState.Disabled;
        public bool UpdatedSettings = false;
        public bool RequestConnectedStandby = false;
    }

    public class ConnectionEvent : EventArgs
    {
        public readonly BackendEvent Event;
        public ConnectionEvent(BackendEvent event_) {Event = event_; }
    }

    public class Connections : IDisposable
    {
        public void Dispose() { Dispose(true); }

        virtual protected void Dispose(bool managed)
        {
            _settingMutex.Dispose();
        }

        private readonly Dictionary<string, Connection> _connectionList = new Dictionary<string, Connection>();
        private ManualResetEvent _settingMutex = new ManualResetEvent(true);
        private readonly Runtime _backend = null;

        public Connections(Runtime backend)
        {
            if (backend != null)
                _backend = backend;
            else
                throw new Exception("Connections constructor - BACKEND NOT DEFINED");

            Update();
        }

        public List<string> ConnectionIdList
        {
            get
            {
                var list = new List<string>();

                _settingMutex.WaitOne(4000);
                _settingMutex.Reset();

                foreach (var item in _connectionList)
                    list.Add(item.Key);

                _settingMutex.Set();

                return list;
            }
        }

        public void Update() 
        {
            _settingMutex.WaitOne(4000);
            _settingMutex.Reset();

            Accounts accounts = new Accounts();
            Status status = new Status();

            // get obsolete
            List<string> removeList = new List<string>();
            foreach( var connection in this._connectionList )
            {
                if( accounts[connection.Key] == null )
                    removeList.Add(connection.Key);
            }

            // Remove obsolete
            foreach (var item in removeList)
            {
                _connectionList[item].Clean();
                _connectionList.Remove(item);
            }
            // Add new and update existing
            foreach (Account server in accounts)
            {
                if (server.IsValid())
                {
                    // Create new parameters
                    var parameters = new ConnectionParameters();
                    parameters.Hostname = server.host;
                    parameters.JID = server.jid;
                    parameters.Password = server.password;
                    parameters.UseSSL = server.usesssl;
                    parameters.OldStyleSSL = server.oldstylessl;
                    parameters.AuthPlain = server.authplain;
                    parameters.AuthMD5 = server.authmd5;
                    parameters.AuthSCRAM = server.authscram;
                    parameters.AuthOAUTH2= server.authoauth2;
                    parameters.RequestConnectedStandby = server.requestConnectedStandby;
                    parameters.Port = server.port;

                    // Messanger is offline? Do so!
                    if (status.status == StatusType.Offline)
                        parameters.State = AccountState.Disabled;
                    else
                        parameters.State = server.persistantState;

                    parameters.UpdatedSettings = server.ResetChangedState();

                    if (parameters.RequestConnectedStandby && BackgroundExecutionManager.GetAccessStatus() != BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                        _backend.OnConnectionEvent(this, new ConnectionEvent(new BackendEventError(parameters.JID, ErrorType.NoHardwareSlotsAllowed, ErrorPolicyType.Deactivate)));

                    // Add new server
                    if (!_connectionList.ContainsKey(server.jid))
                    {
                        try
                        {
                            Connection newConnection = new Connection(server.jid);
                            newConnection.OnEvent += _backend.OnConnectionEvent;
                            _connectionList[server.jid] = newConnection;

                            parameters.UpdatedSettings = true;
#if DEBUG
                            _backend.OnConnectionEvent(this, new ConnectionEvent(new BackendEventLog("", LogType.Info, "Server with name " + server.title + " added")));
#endif
                        }
                        catch
                        {
#if DEBUG
                            _backend.OnConnectionEvent(this, new ConnectionEvent(new BackendEventLog("", LogType.Error, "Adding server with name " + server.title + " failed")));
#endif
                            _settingMutex.Set();
                        }
                    }

                    // Get connection
                    Connection currentConnection = _connectionList[server.jid];

                    // Update
                    currentConnection.Update(parameters);
                }
                else if( !server.forceDisabled )
                {
                    _backend.OnConnectionEvent(this, new ConnectionEvent(new BackendEventError(server.jid, ErrorType.InvalidSettings, ErrorPolicyType.Deactivate)));
#if DEBUG
                    _backend.OnConnectionEvent(this, new ConnectionEvent(new BackendEventLog("", LogType.Error, "Server with name " + server.title + " is invalid")));
#endif
                }
            }

            _settingMutex.Set();        
        }

        public bool Send(string id, Tags.Tag tag)
        {
            Connection connection = _connectionList[id];
            if (connection != null)
            {
                connection.Send(tag);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CheckKeepAlive(string id, ControlChannelTrigger trigger)
        {
            Connection connection = _connectionList[id];
            if (connection != null)
                connection.CheckKeepAlive(trigger);
        }

        public void WaitProcessing(string id)
        {
            Connection connection = _connectionList[id];
            if (connection != null)
                connection.WaitProcessing();
        }
    }

    public class Connection : IDisposable
    {
        public void Dispose() { Dispose(true); }

        virtual protected void Dispose(bool managed)
        {
            _updateMutex.Dispose();
            _XMPP.Dispose();
            _controlChannel.Dispose();
            _XMPP.OnLogMessage -= OnLogMessage;
            _XMPP.OnReceive -= OnReceive;
            _XMPP.OnReady -= OnReady;
            _XMPP.OnConnected -= OnConnected;
            _XMPP.OnDisconnected -= OnDisconnected;
            _XMPP.OnResourceBound -= OnResourceBound;
        }

        #region publicproperties

        public readonly string Id;

        #endregion

        #region privateproperties

        private XMPP.Client _XMPP = new XMPP.Client();
        private ControlChannelTrigger _controlChannel;
        private ConnectionParameters _currentParameters = null;
        private bool _updating = false;
        private ManualResetEvent _updateMutex = new ManualResetEvent(true);
        private bool _tryReconnect = false;
        private ErrorType _lastError = ErrorType.None;
        private DateTime _lastReceiveTime = DateTime.Now;

        private bool IsInternetAvailable 
        {
            get
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                    return (profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
                else
                    return false;
            }
        }

        #endregion

        #region publicmethods

        public Connection(string id)
        {
            if (!string.IsNullOrEmpty(id))
                Id = id;
            else
                PushEvent(new BackendEventError("", ErrorType.InvalidConnectionId, ErrorPolicyType.Deactivate));

            UnregisterControlChannel();

            _XMPP.OnLogMessage += OnLogMessage;
            _XMPP.OnError += OnError;
            _XMPP.OnReceive += OnReceive;
            _XMPP.OnReady += OnReady;
            _XMPP.OnConnected += OnConnected;
            _XMPP.OnDisconnected += OnDisconnected;
            _XMPP.OnResourceBound += OnResourceBound;
        }

        public async void Update(ConnectionParameters parameters) 
        {
            bool disconnected = false;
            bool errorState = false;
            _updateMutex.WaitOne(4000);
            _updateMutex.Reset();
            _updating = true;

            // Copy new parameters
            _currentParameters = parameters;

            // Connection break up
            if (_XMPP.Connected && !IsInternetAvailable)
            {
                PushEvent(ErrorType.NoInternet, ErrorPolicyType.Informative);
                Disconnect();
                disconnected = true;
            }

            // Settings changed
            if (_currentParameters.UpdatedSettings)
            {
                if (!disconnected)
                {
#if DEBUG
                    PushEvent(LogType.Info, "Settings changed, disconnecting");
#endif
                    Disconnect();
                    disconnected = true;
                }

                if (string.IsNullOrEmpty(_currentParameters.Hostname))
                {
                    PushEvent(ErrorType.InvalidHostname, ErrorPolicyType.Deactivate);
                    errorState = true;
                }

                if (string.IsNullOrEmpty(_currentParameters.JID))
                {
                    PushEvent(ErrorType.InvalidJID, ErrorPolicyType.Deactivate);
                    errorState = true;
                }

                if (string.IsNullOrEmpty(_currentParameters.Password))
                {
                    PushEvent(ErrorType.MissingPassword, ErrorPolicyType.Deactivate);
                    errorState = true;
                }

                _XMPP.Settings.Account = this.Id;
                _XMPP.Settings.Id = _currentParameters.JID;
                _XMPP.Settings.Password = _currentParameters.Password;
                _XMPP.Settings.Hostname = _currentParameters.Hostname;
                _XMPP.Settings.Port = _currentParameters.Port; 
                _XMPP.Settings.SSL = _currentParameters.UseSSL;
                _XMPP.Settings.OldSSL = _currentParameters.OldStyleSSL;
                _XMPP.Settings.AuthenticationTypes = MechanismType.None;
                if (_currentParameters.AuthPlain) _XMPP.Settings.AuthenticationTypes |= MechanismType.Plain;
                if (_currentParameters.AuthMD5) _XMPP.Settings.AuthenticationTypes |= MechanismType.DigestMD5;
                if (_currentParameters.AuthSCRAM) _XMPP.Settings.AuthenticationTypes |= MechanismType.SCRAM;
                if (_currentParameters.AuthOAUTH2) _XMPP.Settings.AuthenticationTypes |= MechanismType.XOAUTH2;
            }

            if (!errorState)
            {
                // Set offline
                if (_currentParameters.State == AccountState.Disabled)
                {
                    _lastError = ErrorType.None;
                    if (!disconnected)
                    {
#if DEBUG
                        PushEvent(LogType.Info, "Disconnecting");
#endif
                        Disconnect();
                        disconnected = true;
                    }
                    _lastError = ErrorType.None;
                }

                // Set online
                else if (_currentParameters.State == AccountState.Enabled)
                {
                    if (_XMPP.Connected)
                    {
#if DEBUG
                        PushEvent(LogType.Info, "Already connected");
#endif
                    }
                    else if (!IsInternetAvailable)
                    {
                        PushEvent(ErrorType.NoInternet, ErrorPolicyType.Informative);
                    }
                    else
                    {
#if DEBUG
                        PushEvent(LogType.Info, "Connecting");
#endif
                        Connect();
                    }
                }
            }

            // Unset changed status
            _currentParameters.UpdatedSettings = false;
            _updateMutex.Set();

            if (_tryReconnect)
            {
                await Task.Delay(Runtime._eventDelayMS);
                _tryReconnect = false;
                _currentParameters.UpdatedSettings = true;
                Update(_currentParameters);
            }

            _updating = false;
        }

        public bool Send(Tags.Tag tag)
        {
            if (_XMPP.Connected)
            {
                _XMPP.Send(tag);
                return true;
            }

            PushEvent(ErrorType.NotConnected, ErrorPolicyType.Informative);
            return false;
        }

        public void Clean()
        {
            Disconnect();
            _currentParameters = null;
            _lastError = ErrorType.None;
        }

        public void WaitProcessing()
        {
            _XMPP.ProcessComplete.WaitOne(4000);
        }

        public void CheckKeepAlive(ControlChannelTrigger trigger)
        {
            // Send keepalive for the next check
            var pingIq = new XMPP.tags.jabber.client.iq();
            pingIq.type = Tags.jabber.client.iq.typeEnum.get;
            pingIq.from = Id;
            pingIq.Add(new XMPP.tags.xmpp.ping.ping());
            Send(pingIq);

            // Check how long since the last packet
            var diffTime = DateTime.Now - _lastReceiveTime;
            var diffTimeMinutes = (uint)diffTime.TotalMinutes;

            var keepAliveMinutes = (trigger != null) ? trigger.CurrentKeepAliveIntervalInMinutes : 15; // 15 is default

            if (diffTimeMinutes > keepAliveMinutes)
            {
                trigger.DecreaseNetworkKeepAliveInterval();
                OnError(this, new ErrorEventArgs("Connection to server lost", ErrorType.NotConnected, ErrorPolicyType.Reconnect));
            }
        }

        #endregion

        #region events

        public event NewEvent OnEvent;
        public delegate void NewEvent(object sender, ConnectionEvent e);
        private void PushEvent(Tags.Tag tag) { PushEvent(new BackendEventMessage(Id, tag)); }
        private void PushEvent(RequestType requestType) { PushEvent(new BackendEventRequest(Id, requestType)); }
        private void PushEvent(StateType stateType) { PushEvent(new BackendEventState(Id, stateType)); }
        private void PushEvent(LogType logType, string logMessage) { PushEvent(new BackendEventLog(Id, logType, logMessage)); }
        private void PushEvent(ErrorType errorType, ErrorPolicyType errorPolicy,  string message = "") { PushEvent(new BackendEventError(Id, errorType, errorPolicy, message)); }
        private void PushEvent(BackendEvent event_) { PushEvent(this, new ConnectionEvent(event_)); }
        private void PushEvent(object sender, ConnectionEvent e) { if (OnEvent != null)OnEvent(sender, e); }

        private void OnReady(object sender, EventArgs e)
        {
            _lastError = ErrorType.None;
            PushEvent(StateType.Running);

            Status statusStore = new Status();
            Helper.PublishState(Id, statusStore.status, statusStore.message);
        }

        private void OnConnected(object sender, EventArgs e)
        {
            try
            {
                _controlChannel.WaitForPushEnabled();
            }
            catch
            {
                PushEvent(ErrorType.WaitForPushEnabled, ErrorPolicyType.Reconnect);
                Disconnect();
            }

            PushEvent(StateType.Connected);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            PushEvent(StateType.Disconnected);
        }

        private void OnReceive(object sender, TagEventArgs e)
        {
            _lastReceiveTime = DateTime.Now;
            PushEvent(e.tag);
        }

        private void OnLogMessage(object sender, LogEventArgs e)
        {
            PushEvent(e.type, e.message);
        }

        private async void OnError(object sender, ErrorEventArgs e)
        {
            if (e.policy != ErrorPolicyType.Informative)
                PushEvent(StateType.Disconnected);

            if (e.policy == ErrorPolicyType.Reconnect)
            {
                if (IsInternetAvailable)
                {
                    if (_lastError == e.type && e.type != ErrorType.None)
                    {
                        e.policy = ErrorPolicyType.Deactivate;
                        _lastError = ErrorType.None;
                    }
                    else
                    {
                        // Remember the error
                        _lastError = e.type;

                        if (_updating)
                        {
                            _tryReconnect = true;
                        }
                        else
                        {
                            await Task.Delay(Runtime._eventDelayMS);
                            if (_currentParameters != null)
                            {
                                _currentParameters.UpdatedSettings = true;
                                Update(_currentParameters);
                            }
                        }

                        return;
                    }
                }
                else
                {          
                    return;
                }
            }
            
            PushEvent(e.type, e.policy, e.message);
        }

        private void OnResourceBound(object sender, ResourceBoundEventArgs e)
        {
            PushEvent(StateType.ResourceBound);
        }

        #endregion

        #region privatemethods

        private void Connect()
        {
            if (IsInternetAvailable)
            {
                if (RegisterControlChannel())
                {
                    _XMPP.Connect();
                    PushEvent(StateType.Connecting);
                }
                else if( IsInternetAvailable )
                {
                    PushEvent(ErrorType.BackgroundTaskCreate, ErrorPolicyType.Reconnect);
                }
            }
        }

        private void Disconnect()
        {
            PushEvent(StateType.Disconnecting);
            _XMPP.Disconnect();
            UnregisterControlChannel();
        }

        private bool RegisterControlChannel()
        {
            if (!UnregisterControlChannel())
                return false;

            if (IsInternetAvailable)
            {
                _XMPP.Socket = new StreamSocket();

                try
                {
                    // Create controlchannel
                    var slotType = _currentParameters.RequestConnectedStandby ? ControlChannelTriggerResourceType.RequestHardwareSlot : ControlChannelTriggerResourceType.RequestSoftwareSlot;
                    _controlChannel = new ControlChannelTrigger("CT" + Id, 15, slotType);
                    _controlChannel.UsingTransport(_XMPP.Socket);

                    // Register package received event
                    BackgroundTaskBuilder pushNotificationTrigger = new BackgroundTaskBuilder();
                    pushNotificationTrigger.Name = "PN" + Id;
                    pushNotificationTrigger.TaskEntryPoint = "BackgroundTasks.PushNotificationTrigger";
                    pushNotificationTrigger.SetTrigger(_controlChannel.PushNotificationTrigger);
                    pushNotificationTrigger.Register();

                    // Register keepalive event
                    BackgroundTaskBuilder keepAliveTrigger = new BackgroundTaskBuilder();
                    keepAliveTrigger.Name = "KA" + Id;
                    keepAliveTrigger.TaskEntryPoint = "BackgroundTasks.KeepAliveTrigger";
                    keepAliveTrigger.SetTrigger(_controlChannel.KeepAliveTrigger);
                    keepAliveTrigger.Register();
                }
                catch
                {
                    PushEvent(ErrorType.RegisterControlChannel, ErrorPolicyType.Reconnect);
                    return false;
                }
#if DEBUG
                PushEvent(LogType.Info, "ControlChanel registered");
#endif
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UnregisterControlChannel()
        {
            try
            {
                // Renew Socket
                if( _XMPP.Socket != null )
                    _XMPP.Socket.Dispose();

                // Unregister leftovers
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name.StartsWith("PN"+Id) || task.Value.Name.StartsWith("KA"+Id))
                        task.Value.Unregister(true);
                }

                if (_controlChannel != null)
                    _controlChannel.Dispose();
            }
            catch
            {
                PushEvent(ErrorType.UnregisterControlChannel, ErrorPolicyType.Reconnect);
                return false;
            }
#if DEBUG
            PushEvent(LogType.Info, "ControlChanel unregistered");
#endif
            return true;
        }

        #endregion
    }
}
