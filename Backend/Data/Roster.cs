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
using System.Linq;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;
using Tags = XMPP.tags;

namespace Backend.Data
{
    public class Roster : ICollectionStore<Contact>
    {
        public Roster() : base() { }
        public Roster(string name) : base(name) { }
        public Roster(ApplicationDataContainer parent) : base(parent) { }
        public Roster(ApplicationDataContainer parent, string name) : base(parent, name) { }

        public Contact CreateContact(string account, string jid, string name = "")
        {
            if (!string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(jid))
            {
                var contact = this.CreateItem(jid);
                contact.LockUpdates();
                contact.jid = jid;
                contact.account = account;
                contact.name = name;
                contact.UnlockUpdates();
                return contact;
            }
            return null;
        }

        public int UnreadNotificationCount
        {
            get
            {
                int count = 0;
                foreach (var contact in this)
                    count += contact.UnreadMessageCount;

                return count;
            }
        }

        public void ClearOffline()
        {
            var offline = this.Where((contact) => {                                             // All have to be true for the contact to be deleted
                return  !contact.IsOnline &&                                                    // No available resources
                        contact.jid != contact.account &&                                       // Is not the account
                        contact.subscriptionRequest == Contact.SubscriptionRequestType.None;    // Has not requested anything
            }).ToList();
            foreach (var item in offline)
                Remove(item);
        }

        public void CleanInvalid()
        {
            var invalid = this.Where((contact) => string.IsNullOrEmpty(contact.account) || string.IsNullOrEmpty(contact.jid)).ToList();
            foreach (var item in invalid)
                Remove(item);
        }

        public void BufferImages()
        {
            foreach (var contact in this)
                contact.BufferImage();
        }
    }

    public class Contact : IStore<string> 
    {
        #region general

        public enum SubscriptionRequestType
        {
            None,
            Subscribe,
            Unsubscribe
        }

        private bool _updatesLocked = false;
        public void LockUpdates() { _updatesLocked = true; }
        public void UnlockUpdates() { _updatesLocked = false; ContactUpdated(this); }

        public event ContactUpdatedHandler OnContactUpdated;
        public delegate void ContactUpdatedHandler(object sender, EventArgs e);
        private void ContactUpdated(object sender) 
        {
            if (OnContactUpdated != null && !_updatesLocked)
                OnContactUpdated(sender, new EventArgs()); 
        }

        public Contact(ApplicationDataContainer parent, string name) : base(parent, name)
        {
            SetDefault("account", "");
            SetDefault("jid", "");
            SetDefault("name", "");
            SetDefault("nick", "");
            SetDefault("photohash", "");

            this.subscriptionRequest = SubscriptionRequestType.None;
            this.Groups = new IValueStore<string>(this.Value, "Groups");

            this.Groups.CollectionChanged += (s, e) =>
            {
                EmitPropertyChanged("Groups");
            };
        }

        #endregion

        #region common

        public string account 
        { 
            get { return this["account"]; } 
            set 
            { 
                if( this["account"] == value )
                    return;

                if (value == null)
                    this["name"] = "";

                this["account"] = value; 
            }
        }

        public string jid 
        { 
            get { return this["jid"]; } 
            set 
            {
                if (this["jid"] == value)
                    return;

                if (value == null)
                    this["name"] = "";

                this["jid"] = value; 
                EmitPropertyChanged("DisplayName"); 
                ContactUpdated(this);
            } 
        }

        public string name 
        {
            get { return this["name"]; } 
            set 
            {
                if (this["name"] == value)
                    return;

                if (value == null)
                    this["name"] = "";

                this["name"] = value; 
                EmitPropertyChanged("DisplayName"); 
                ContactUpdated(this); 
            } 
        }

        public SubscriptionRequestType subscriptionRequest { get; set; }

        public Tags.jabber.iq.roster.item.askEnum ask
        {
            get
            {
                var stringval = this["ask"];
                if (!string.IsNullOrEmpty(stringval))
                    return (Tags.jabber.iq.roster.item.askEnum)Enum.Parse(typeof(Tags.jabber.iq.roster.item.askEnum), stringval);
                else
                    return Tags.jabber.iq.roster.item.askEnum.none;
            }
            set
            {
                if (this["ask"] == value.ToString())
                    return;

                this["ask"] = value.ToString();
                ContactUpdated(this);
            }
        }

        public Tags.jabber.iq.roster.item.subscriptionEnum subscription 
        { 
            get 
            {
                var stringval = this["subscription"];
                if (!string.IsNullOrEmpty(stringval))
                    return (Tags.jabber.iq.roster.item.subscriptionEnum)Enum.Parse(typeof(Tags.jabber.iq.roster.item.subscriptionEnum), stringval);
                else
                    return Tags.jabber.iq.roster.item.subscriptionEnum.none;
            } 
            set 
            { 
                if( this["subscription"] == value.ToString())
                    return;

                this["subscription"] = value.ToString();
                ContactUpdated(this);
            } 
        }

        public IValueStore<string> Groups { get; private set; }

        public string nick 
        { 
            get { return this["nick"]; } 
            set 
            {
                if (this["nick"] == value)
                    return;

                if (value == null)
                    this["name"] = "";

                this["nick"] = value; 
                EmitPropertyChanged("DisplayName"); 
                ContactUpdated(this); 
            } 
        }

        public string DisplayName // Roster Name > Nick > Jid
        {
            get
            {
                if (!string.IsNullOrEmpty(this.name))
                    return this.name;
                else if (!string.IsNullOrEmpty(this.nick))
                    return this.nick;
                else
                    return this.jid;
            }
        }

        public bool HideContact 
        { 
            get 
            {
                if (string.IsNullOrEmpty(jid))
                    return true;

                return (jid == account || (!jid.Contains("@"))); 
            } 
        } // We don't want to see ourselves or servers

        #endregion

        #region avatar

        public bool vCardRequested = false;

        public string photohash { get { return this["photohash"]; } }

        public void SetAvatar(byte[] data)
        {
            if (data.Length > 0 && !string.IsNullOrEmpty(this.jid))
            {
                // Save the image
                var hash = Avatar.Set(jid, data);
                if (!string.IsNullOrEmpty(hash))
                {
                    this["photohash"] = hash;
                    _imageData = Avatar.BitmapFromBytes(data);
                    EmitPropertyChanged("ImageURI"); 
                    EmitPropertyChanged("ImageData"); 
                    ContactUpdated(this); 
                }
            }
        }

        private BitmapImage _imageData = null;
        public BitmapImage ImageData
        {
            get
            {
                BufferImage();
                return _imageData;
            }
        }

        public async void BufferImage()
        {
            if (_imageData == null)
            {
                byte[] data = null;
                await ThreadPool.RunAsync(delegate { data = Avatar.GetFile(this.jid); });

                if( data != null )
                    _imageData = Avatar.BitmapFromBytes(data);
            }
        }

        public Uri ImageURI { get { return new Uri(Avatar.GetFileURI(this.jid)); } }

        #endregion  

        #region resources

        private ObservableDictionary<string, Resource> Resources = new ObservableDictionary<string, Resource>();

        public void SetResource(
            string id, 
            int priority = 0, 
            Tags.jabber.client.show.valueEnum status = Tags.jabber.client.show.valueEnum.none, 
            string statusMessage = ""
        )
        {
            if (!string.IsNullOrEmpty(id))
            {
                var resource = default(Resource);
                if (Resources.ContainsKey(id))
                    resource = Resources[id];
                else
                {
                    Resources[id] = new Resource();
                    resource = Resources[id];
                }

                resource.id = id;
                resource.priority = priority;
                resource.status = status;
                resource.statusMessage = statusMessage;

                EmitPropertyChanged("IsOnline");
                EmitPropertyChanged("Resources");
                EmitPropertyChanged("CurrentResource");
                EmitPropertyChanged("HasStatusMessage");
                EmitPropertyChanged("CurrentStatus");
                EmitPropertyChanged("CurrentStatusMessage");
                ContactUpdated(this);
            }
        }

        public void RemoveResource(string id)
        {
            Resources.Remove(id);
            EmitPropertyChanged("IsOnline");
            EmitPropertyChanged("Resources");
            EmitPropertyChanged("CurrentResource");
            EmitPropertyChanged("HasStatusMessage");
            EmitPropertyChanged("CurrentStatus");
            EmitPropertyChanged("CurrentStatusMessage");
            ContactUpdated(this);
        }

        public void ClearResources()
        {
            Resources.Clear();
            EmitPropertyChanged("IsOnline");
            EmitPropertyChanged("Resources");
            EmitPropertyChanged("CurrentResource");
            EmitPropertyChanged("HasStatusMessage");
            EmitPropertyChanged("CurrentStatus");
            EmitPropertyChanged("CurrentStatusMessage");
            ContactUpdated(this);
        }

        public void SetLastSender(string id)
        {
            if (!string.IsNullOrEmpty(id) && Resources.ContainsKey(id))
            {
                foreach (var res in Resources)
                    res.Value.lastSender = false;

                Resources[id].lastSender = true;
            }
        }

        private Resource CurrentResource
        {
            get
            {
                int maxPriority = -1;
                Resource maxResource = null;
                foreach (var resource in Resources)
                {
                    if (resource.Value.lastSender)
                        return resource.Value;

                    if (resource.Value.priority >= maxPriority)
                    {
                        maxPriority = resource.Value.priority;
                        maxResource = resource.Value;
                    }
                }

                return maxResource;
            }
        }

        public string CurrentJID
        {
            get
            {
                var curjid = new XMPP.JID(jid);

                var resource = CurrentResource;
                if (resource != null)
                    curjid.Resource = resource.id;

                return curjid.ToString();
            }
        }

        public bool IsOnline { get { return Resources.Count > 0; } }
        public bool HasStatusMessage { get { return !string.IsNullOrEmpty(CurrentStatusMessage); } }

        public StatusType CurrentStatus
        {
            get
            {
                var resource = CurrentResource;
                if (resource != null)
                {
                    switch (resource.status)
                    {
                        case Tags.jabber.client.show.valueEnum.away:
                            return StatusType.Away;
                        case Tags.jabber.client.show.valueEnum.xa:
                            return StatusType.Away;
                        case Tags.jabber.client.show.valueEnum.chat:
                            return StatusType.Available;
                        case Tags.jabber.client.show.valueEnum.dnd:
                            return StatusType.Busy;
                        case Tags.jabber.client.show.valueEnum.none:
                            return StatusType.Available; 
                    }
                }

                return StatusType.Offline;
            }
        }

        public string CurrentStatusMessage 
        {
            get 
            {
                var resource = CurrentResource;
                if( resource != null )
                    return resource.statusMessage; 
                else
                    return string.Empty;
            } 
        }


        #endregion

        #region messagecount

        private int _unreadMessageCount = 0;
        public int UnreadMessageCount 
        { 
            get 
            { 
                return _unreadMessageCount; 
            } 
            set 
            {
                if (_unreadMessageCount != value)
                {
                    _unreadMessageCount = value;
                    EmitPropertyChanged("UnreadMessageCount");
                    EmitPropertyChanged("HasUnreadMessages");
                    ContactUpdated(this);
                }
            } 
        }

        public bool HasUnreadMessages { get { return UnreadMessageCount > 0; } }
        
        #endregion

    }

    public class Resource
    {
        public Resource() 
        {
            id = string.Empty;
            priority = 0;
            status = Tags.jabber.client.show.valueEnum.none;
            statusMessage = string.Empty;
            lastSender = false;
        }

        public string id { get; set; }
        public int priority { get; set; }
        public Tags.jabber.client.show.valueEnum status { get; set; }
        public string statusMessage { get; set; }
        public bool lastSender { get; set; }
    }
}
