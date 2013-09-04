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
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;

namespace BackgroundTasks
{
    // ControlChannel
    public sealed class KeepAliveTrigger : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "KeepAliveTrigger", reason);
        }
    }

    public sealed class PushNotificationTrigger : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "PushNotificationTrigger", reason);
        }
    }

    // SystemTrigger
    public sealed class ControlChannelReset : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "ControlChannelReset", reason);
        }
    }


    public sealed class InternetAvailable : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "InternetAvailable", reason);
        }
    }


    public sealed class InternetNotAvailable : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "InternetNotAvailable", reason);
        }
    }


    public sealed class ServicingComplete : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "ServicingComplete", reason);
        }
    }


    public sealed class SessionConnected : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "SessionConnected", reason);
        }
    }


    public sealed class UserAway : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "UserAway", reason);
        }
    }


    public sealed class UserPresent : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "UserPresent", reason);
        }
    }


    public sealed class LockScreenApplicationAdded : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "LockScreenApplicationAdded", reason);
        }
    }


    public sealed class LockScreenApplicationRemoved : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "LockScreenApplicationRemoved", reason);
        }
    }


    public sealed class TimeZoneChange : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += Canceled;

            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskRunning(taskInstance);
        }

        public void Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            var backend = Runtime.Instance;
            if (backend != null)
                backend.OnBackgroundTaskCanceled(sender, "TimeZoneChange", reason);
        }
    }





}