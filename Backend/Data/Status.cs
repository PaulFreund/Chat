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
using Windows.UI.Xaml.Media.Imaging;

namespace Backend.Data
{
    public enum StatusType
    {
        Offline = 0,
        Busy = 1,
        Away = 2,
        Available = 3
    }

    public class Status : IMixedStore
    {
        public Status() : base()
        {
            SetDefault("status", StatusType.Available.ToString());
            SetDefault("message", "");
            SetDefault("currentNotificationCount", 0);
        }

        private bool _autoAwayActive = false;
        public bool autoAwayActive
        {
            get
            {
                return _autoAwayActive;
            }
            set
            {
                _autoAwayActive = value;
                EmitPropertyChanged("status");
                EmitPropertyChanged("message");
            }
        }

        private Dictionary<string, bool> _loadingStates = new Dictionary<string, bool>();

        public void SetLoading(string account, bool loading)
        {
            if ( !_loadingStates.ContainsKey(account) )
                _loadingStates.Add(account, loading);
            else
                _loadingStates[account] = loading;

            EmitPropertyChanged("IsLoading");
        }

        public bool GetLoading()
        {
            var loading = false;
            foreach (var state in _loadingStates)
            {
                if (state.Value)
                    loading = true;
            }
            return loading;
        }

        private bool _hasInvalidAccounts = false;
        public bool HasInvalidAccounts { get { return _hasInvalidAccounts; } set { _hasInvalidAccounts = value; EmitPropertyChanged(); } }

        public bool IsLoading { get { return GetLoading(); } }

        public int currentNotificationCount { get { return GetProperty<int>("currentNotificationCount"); } set { SetProperty<int>("currentNotificationCount", value); } }

        public StatusType status
        {
            get
            {
                if (_autoAwayActive)
                    return StatusType.Away;

                var stringval = GetProperty<string>("status");
                if (!string.IsNullOrEmpty(stringval))
                    return (StatusType)Enum.Parse(typeof(StatusType), GetProperty<string>("status"));
                else
                    return StatusType.Offline;
            }
            set
            {
                if (GetProperty<string>("status") == value.ToString())
                    return;

                _autoAwayActive = false;

                SetProperty<string>("status", value.ToString());
                EmitPropertyChanged("message");
            }
        }

        public string message 
        { 
            get 
            {
                if (_autoAwayActive)
                {
                    var settings = new Settings();
                    if( !string.IsNullOrEmpty(settings.autoAwayMessage))
                        return settings.autoAwayMessage;
                }

                return GetProperty<string>("message"); 
            } 
            set
            {
                _autoAwayActive = false;
                SetProperty<string>("message", value); 
            } 
        }

        private static string avatarName = "useravatar";
        private string photohash { get { return GetProperty<string>("photohash"); } }

        public void SetAvatar(byte[] data)
        {
            if (data.Length > 0)
            {
                // Save the image
                var hash = Avatar.Set(avatarName, data);
                if (!string.IsNullOrEmpty(hash))
                {
                    SetProperty<string>("photohash", hash);
                    _imageData = Avatar.BitmapFromBytes(data);
                    EmitPropertyChanged("ImageURI");
                    EmitPropertyChanged("ImageData");
                }
            }
        }

        private BitmapImage _imageData = null;
        public BitmapImage ImageData
        {
            get
            {
                if (_imageData == null)
                    _imageData = Avatar.BitmapFromBytes(Avatar.GetFile(avatarName));

                return _imageData;
            }
        }

        public Uri ImageURI
        {
            get
            {
                if (!string.IsNullOrEmpty(avatarName) && !string.IsNullOrEmpty(this.photohash))
                    return new Uri(Avatar.GetFileURI(avatarName));
                else
                    return new Uri("ms-appx:///Assets/DefaultAvatar.png");
            }
        }
    }
}
