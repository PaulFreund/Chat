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

using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage;
using Windows.UI.Core;
using XMPP;
using Tags = XMPP.tags;

namespace Backend.Common
{
    public enum EventType
    {
        Windows,    // Windows Messages
        Message,    // All Tags
        Request,    // For example a request for background access
        State,      // Connected, disconnected and alike
        Log,        // Log messages
        Error       // As obvious
    }

    public enum WindowsType
    {
        ApplicationSuspending,
        ApplicationResuming,
        ApplicationExiting,
        ControlChannelReset,
        InternetAvailable,
        InternetNotAvailable,
        ServicingComplete,
        SessionConnected,
        UserAway,
        UserPresent,
        LockScreenApplicationAdded,
        LockScreenApplicationRemoved,
        TimeZoneChange
    }
    
    public enum RequestType
    {
        CoreWindow,
        BackgroundAccess
    }

    public enum StateType
    {
        Connecting,
        Disconnecting,
        Connected,
        Disconnected,
        Running,
        ResourceBound
    }

    public class BackendEvent
    {
        public bool Persist = false;

        public readonly string Id;
        public readonly EventType Type;
        public BackendEvent(string id, EventType type) { Id = id; Type = type; }

        public override string ToString()
        {
            var be = this as BackendEvent;
            switch (Type)
            {
                case EventType.Windows:
                    be = this as BackendEventWindows;
                    return be.ToString();
                case EventType.Message:
                    be = this as BackendEventMessage;
                    return be.ToString();
                case EventType.Request:
                    be = this as BackendEventRequest;
                    return be.ToString();
                case EventType.State:
                    be = this as BackendEventState;
                    return be.ToString();
                case EventType.Log:
                    be = this as BackendEventLog;
                    return be.ToString();
                case EventType.Error:
                    be = this as BackendEventError;
                    return be.ToString();
            }
            return "";
        }
    }

    public class BackendEventWindows : BackendEvent
    {
        public readonly WindowsType WindowsType;
        public readonly bool Canceled;
        public readonly string Reason;
        public BackendEventWindows(string id, WindowsType windowsType, bool canceled = false, string reason = "") : base(id, EventType.Windows) { WindowsType = windowsType; Canceled = canceled; Reason = reason; }

        public override string ToString()
        {
            return "[" + Id + "]" + "[" + Type.ToString() + "]" + "[" + Canceled.ToString() + "]" + "[" + Reason + "]" + WindowsType.ToString();
        }
    }

    public class BackendEventMessage : BackendEvent
    {
        public readonly Tags.Tag Tag;
        public readonly string UUID;
        public BackendEventMessage(string id, Tags.Tag tag, string uuid = "") : base(id, EventType.Message) 
        { 
            Tag = tag;
            if (uuid == "")
                UUID = Guid.NewGuid().ToString();
            else
                UUID = uuid;
        }

        public override string ToString()
        {
            return "["+Id+"]"+"["+Type.ToString()+"]"+ Tag;
        }
    }

    public class BackendEventRequest : BackendEvent
    {
        public readonly RequestType RequestType;
        public BackendEventRequest(string id, RequestType requestType) : base(id, EventType.Request) { RequestType = requestType; }

        public override string ToString()
        {
            return "[" + Id + "]" + "[" + Type.ToString() + "]" + RequestType.ToString();
        }
    }

    public class BackendEventState : BackendEvent
    {
        public readonly StateType StateType;
        public BackendEventState(string id, StateType stateType) : base(id, EventType.State) { StateType = stateType; }

        public override string ToString()
        {
            return "[" + Id + "]" + "[" + Type.ToString() + "]" + StateType.ToString();
        }
    }

    public class BackendEventLog : BackendEvent
    {
        public readonly LogType LogType;
        public readonly string LogMessage;
        public BackendEventLog(string id, LogType logType, string logMessage) : base(id, EventType.Log) { LogType = logType; LogMessage = logMessage; }

        public override string ToString()
        {
            return "[" + Id + "]" + "[" + Type.ToString() + "]" + "[" + LogType.ToString() + "]" + LogMessage;
        }
    }

    public class BackendEventError : BackendEvent
    {
        public readonly ErrorType Error;
        public readonly ErrorPolicyType Policy;
        public readonly string Message;
        public BackendEventError(string id, ErrorType errorType, ErrorPolicyType policy, string errorMessage = "") : base(id, EventType.Error) { Error = errorType; Policy = policy; Message = errorMessage; }

        public override string ToString()
        {
            return "[" + Id + "]" + "[" + Type.ToString() + "]" + "[" + Policy.ToString() + "]" + "[" + Error.ToString() + "]" + " (" + Message + ")";
        }
    }

    
    public class Events : IDisposable
    {
        public void Dispose() { Dispose(true); }

        virtual protected void Dispose(bool managed)
        {
            _accessMutex.Dispose();
        }

        private readonly int MAX_MESSAGE_SIZE = 2048; // 8K SHOULD BE Max Value Size, but every char has up to 4Bytes

        private ApplicationDataContainer _persistantQueue = ApplicationData.Current.LocalSettings.CreateContainer("Queue", ApplicationDataCreateDisposition.Always);

        private Queue<BackendEvent> _events = new Queue<BackendEvent>();
        private ManualResetEvent _accessMutex = new ManualResetEvent(true);

        private CoreDispatcher _UIDispatcher = null;
        public CoreDispatcher UIDispatcher 
        { 
            private get
            {
                return _UIDispatcher;
            }

            set
            {
                if (value != null)
                {
                    _UIDispatcher = value;
                    NewEvent();
                }
            }
        }

        public Events() 
        {
            _events.Clear(); 
            UIDispatcher = null;
        }

        private event EventNotification _internalOnEvent;
        public event EventNotification OnEvent
        {
            add
            {
                if (_internalOnEvent != null)
                {
                    // Prevent multiple adding
                    foreach (Delegate existing in _internalOnEvent.GetInvocationList())
                    {
                        if (existing == (Delegate)value)
                            return;
                    }
                }

                _internalOnEvent += value;
            }

            remove
            {
                if (_internalOnEvent != null)
                    _internalOnEvent -= value;
            }
        }

        public delegate void EventNotification(object sender, EventArgs e);

        public void Restore()
        {
            var old = GetPersistant();
            foreach (var item in old)
            {
                var tag = Tags.Tag.Get((string)item.Value);
                var message = new BackendEventMessage("", tag, item.Key);
                _events.Enqueue(message);
            }
        }

        public void Clear()
        {
            ClearPersistant();
        }

        public void NewEvent() 
        {
            RunDispatched(() =>
            {
                if (_internalOnEvent != null)
                    _internalOnEvent(this, default(EventArgs));
            });
        }
 
        public void Enqueue(BackendEvent newEvent)
        {
            _accessMutex.WaitOne(4000);
            _accessMutex.Reset();

            if (newEvent.Type == EventType.Message)
            {
                var messageEvent = newEvent as BackendEventMessage;
                if( newEvent.Persist )
                    AddPersistant(messageEvent.UUID, messageEvent.Tag.ToString());
            }

            _events.Enqueue(newEvent);
            _accessMutex.Set();

            NewEvent();
        }

        public BackendEvent Dequeue()
        {
            BackendEvent dequeueEvent = null;

            _accessMutex.WaitOne(4000);
            _accessMutex.Reset();

            if (_events.Count > 0)
            {
                dequeueEvent = _events.Dequeue();

                if (dequeueEvent != null && dequeueEvent.Type == EventType.Message)
                {
                    var messageEvent = dequeueEvent as BackendEventMessage;
                    RemovePersistant(messageEvent.UUID);
                }

            }
            _accessMutex.Set();

            return dequeueEvent;
        }

        private void RunDispatched(DispatchedHandler handler)
        {
            if (UIDispatcher != null)
                UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, handler).AsTask();
        }

        private void AddPersistant(string id, string value)
        {
            if (value.Length > MAX_MESSAGE_SIZE)
            {
                var fragmentNr = 0;
                var fragment = string.Empty;
                while (value.Length > 0)
                {
                    if (value.Length > MAX_MESSAGE_SIZE)
                    {
                        fragment = value.Substring(0, MAX_MESSAGE_SIZE);
                        value = value.Substring(MAX_MESSAGE_SIZE);
                    }
                    else
                    {
                        fragment = value;
                        value = string.Empty;
                    }

                    try
                    {
                        _persistantQueue.Values.Add(id + '#' + fragmentNr, fragment);
                    }
                    catch
                    {
                        ClearPersistant();
                    }

                    fragmentNr++;
                }
            }
            else
            {
                _persistantQueue.Values.Add(id, value);
            }
        }

        private void RemovePersistant(string id)
        {
            try
            {
                List<string> removeList = new List<string>();
                foreach (var key in _persistantQueue.Values.Keys)
                {
                    if (key.StartsWith(id))
                        removeList.Add(key);
                }

                foreach (var key in removeList)
                    _persistantQueue.Values.Remove(key);
            }
            catch {}
        }

        private List<KeyValuePair<string, string>> GetPersistant()
        {

            Dictionary<string, Dictionary<int, string>> elements = new Dictionary<string, Dictionary<int, string>>();
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            try
            {
                foreach (var key in _persistantQueue.Values.Keys)
                {
                    string id = key.Substring(0, 36);
                    int fragmentNo = 0;

                    // Create Object
                    if (!elements.ContainsKey(id))
                        elements.Add(id, new Dictionary<int, string>());

                    // Get fragment No if any
                    if (key.Length > 36) // Bigger than a GUID => fragment
                        fragmentNo = Convert.ToInt32(key.Split('#')[1]);

                    elements[id][fragmentNo] = _persistantQueue.Values[key].ToString();
                }


                foreach (var key in elements.Keys)
                {
                    var elementFragments = elements[key];
                    var elementValue = string.Empty;

                    int fragmentNo = 0;
                    while (elementFragments.ContainsKey(fragmentNo))
                    {
                        elementValue += elementFragments[fragmentNo];
                        fragmentNo++;
                    }

                    list.Add(new KeyValuePair<string, string>(key, elementValue));
                }

                ClearPersistant();
            }
            catch { }

            return list;
        }

        public void ClearPersistant()
        {
            try
            {
                _persistantQueue.Values.Clear();
            }
            catch { }
        }
    }
}
