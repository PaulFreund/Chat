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
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }

    public sealed class PushNotificationTrigger : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete(); 
        }
    }

    // SystemTrigger
    public sealed class ControlChannelReset : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class InternetAvailable : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class InternetNotAvailable : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class ServicingComplete : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class SessionConnected : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class UserAway : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }

    public sealed class UserPresent : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class LockScreenApplicationAdded : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class LockScreenApplicationRemoved : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }


    public sealed class TimeZoneChange : IBackgroundTask
    {
        private BackgroundTaskDeferral defferal = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get defferal
            defferal = taskInstance.GetDeferral();

            try
            {
                var backend = Runtime.Instance;
                if (backend != null)
                    backend.OnBackgroundTaskRunning(taskInstance);
            }
            catch
            {
                if (defferal != null)
                    defferal.Complete();
            }

            if (defferal != null)
                defferal.Complete();
        }
    }





}