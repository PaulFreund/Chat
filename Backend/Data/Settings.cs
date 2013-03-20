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


namespace Backend.Data
{
    public class Settings : IMixedStore
    {
        public Settings() : base() 
        {
            SetDefault("invertOwnMessages", true);
            SetDefault("showOffline", false);
            SetDefault("allowUnknownSenders", false);
            SetDefault("autoAway", true);
            SetDefault("autoAwayMessage", "Away");
            SetDefault("notificationToast", true);
            SetDefault("notificationSound", true);
            SetDefault("notificationVisible", false);
            SetDefault("autoSortRoster", true);
            SetDefault("autoScrollRoster", true);
            SetDefault("stickyRosterContacts", true);
            SetDefault("autoDownloadAvatars", true);
            SetDefault("showInformativeErrors", false);
            SetDefault("invertInterface", false);
        }

        public bool showInformativeErrors { get { return GetProperty<bool>("showInformativeErrors"); } set { SetProperty<bool>("showInformativeErrors", value); } }        
        public bool invertOwnMessages { get { return GetProperty<bool>("invertOwnMessages"); } set { SetProperty<bool>("invertOwnMessages", value); } }            
        public bool showOffline { get { return GetProperty<bool>("showOffline"); } set { SetProperty<bool>("showOffline", value); } }                              
        public bool allowUnknownSenders { get { return GetProperty<bool>("allowUnknownSenders"); } set { SetProperty<bool>("allowUnknownSenders", value); } }      
        public bool autoAway { get { return GetProperty<bool>("autoAway"); } set { SetProperty<bool>("autoAway", value); } }
        public string autoAwayMessage { get { return GetProperty<string>("autoAwayMessage"); } set { SetProperty<string>("autoAwayMessage", value); } }            
        public bool notificationToast { get { return GetProperty<bool>("notificationToast"); } set { SetProperty<bool>("notificationToast", value); } }            
        public bool notificationSound { get { return GetProperty<bool>("notificationSound"); } set { SetProperty<bool>("notificationSound", value); } }            
        public bool notificationVisible { get { return GetProperty<bool>("notificationVisible"); } set { SetProperty<bool>("notificationVisible", value); } }      
        public bool autoSortRoster { get { return GetProperty<bool>("autoSortRoster"); } set { SetProperty<bool>("autoSortRoster", value); } }                     
        public bool autoScrollRoster { get { return GetProperty<bool>("autoScrollRoster"); } set { SetProperty<bool>("autoScrollRoster", value); } }               
        public bool stickyRosterContacts { get { return GetProperty<bool>("stickyRosterContacts"); } set { SetProperty<bool>("stickyRosterContacts", value); } }
        public bool autoDownloadAvatars { get { return GetProperty<bool>("autoDownloadAvatars"); } set { SetProperty<bool>("autoDownloadAvatars", value); } }
        public bool invertInterface { get { return GetProperty<bool>("invertInterface"); } set { SetProperty<bool>("invertInterface", value); } }      
    }
}
