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
using System.Linq;
using Windows.Security.Credentials;
using Windows.Storage;
using Tags = XMPP.tags;

namespace Backend.Data
{
    public enum AccountState
    {
        Disabled,
        Enabled
    }

    public class Accounts : ICollectionStore<Account>
    {
        public Accounts() : base() { }

        public bool IsFull { get { return this.Count >= 5; } }

        public Account New()
        {
            if (IsFull)
                return null;

            return CreateItem();
        }

        public IEnumerable<Account> Enabled { get { return this.Where(account => account.persistantState == AccountState.Enabled); } }

        public bool ContainsJID(string jid)
        {
            return this.Where(x => x.jid == jid).Count() > 0 ? true : false;
        }

        public bool ContainsTitle(string title)
        {
            return this.Where(x => x.title == title).Count() > 0 ? true : false;
        }

        public bool HardwareSlotsAvailable()
        {
            var count = 0;
            foreach (var account in this)
            {
                if (account.requestConnectedStandby)
                    count++;
            }

            return count < 2 ? true : false;
        }

        public new Account this[string key]
        {
            get
            {
                if (!string.IsNullOrEmpty(key) && ContainsJID(key) )
                    return this.Where(x => x.jid == key).First();
                else
                    return null;
            }
        }
    }

    public class Account : IMixedStore
    {
        public Account(ApplicationDataContainer parent, string name) : base(parent, name)
        {
            SetDefault("persistantState", AccountState.Disabled.ToString());
            SetDefault("usessl", false);
            SetDefault("oldstylessl", false);
            SetDefault("authplain", false);
            SetDefault("authmd5", true);
            SetDefault("authscram", true);
            SetDefault("authoauth2", false);
            SetDefault("requestConnectedStandby", false);
            SetDefault("port", 5222);
            SetDefault("forceDisabled", false);
            SetDefault("OwnResource", "");
            
            this.Roster = new Roster(this.Value);

            if (!string.IsNullOrEmpty(jid))
                CreateOwnContact();

        }

        public void DeletePassword()
        {
            if (!string.IsNullOrEmpty(jid))
            {
                PasswordVault vault = new PasswordVault();
                var resources = vault.RetrieveAll();
                foreach (var res in resources)
                {
                    if (res.UserName == this.jid)
                        vault.Remove(res);
                }
            }
        }

        private void CreateOwnContact()
        {
            if (Roster[jid] == null)
                Roster.CreateContact(jid, jid);
        }

        #region runtime

        public Backend.Common.StateType _currentConnectionState = Common.StateType.Disconnected;
        public Backend.Common.StateType CurrentConnectionState
        {
            get { return _currentConnectionState; }
            set { _currentConnectionState = value; EmitPropertyChanged(); }
        }

        public Dictionary<string, Conversation> CurrentConversations = new Dictionary<string, Conversation>();


        public string serverJID = string.Empty;
        public XMPP.JID CurrentJID 
        {
            get
            {
                var jid = default(XMPP.JID);
                if (!string.IsNullOrEmpty(this.serverJID))
                    jid = new XMPP.JID(this.serverJID);
                else
                    jid = new XMPP.JID(this.jid);

                if(!string.IsNullOrEmpty(OwnResource))
                    jid.Resource = OwnResource;

                return jid;
            } 
        }

        public Tags.vcard_temp.vCard CurrentVCard = null;

        public Contact OwnContact { get { CreateOwnContact(); return Roster[jid]; } }
        public string OwnResource { get { return GetString("OwnResource"); } set { SetString("OwnResource", value); } }

        #endregion

        #region persistant

        // Roster

        public Roster Roster { get; private set; }

        // Settings

        public string title { get { return GetString("title"); } set { SetString("title",value); } }
        public string color { get { return GetString("color"); } set { SetString("color", value); } }
        public string host { get { return GetString("host"); } set { settingsChanged = true; SetString("host", value); } }
        public string jid { get { return GetString("jid"); } set { settingsChanged = true; SetString("jid", value); CreateOwnContact(); } }
        public int port { get { return GetProperty<int>("port"); } set { settingsChanged = true; SetProperty<int>("port", value); } }
        public bool usesssl { get { return GetProperty<bool>("usesssl"); } set { settingsChanged = true; SetProperty<bool>("usesssl", value); } }
        public bool oldstylessl { get { return GetProperty<bool>("oldstylessl"); } set { settingsChanged = true; SetProperty<bool>("oldstylessl", value); } }
        public bool authplain { get { return GetProperty<bool>("authplain"); } set { settingsChanged = true; SetProperty<bool>("authplain", value); } }
        public bool authmd5 { get { return GetProperty<bool>("authmd5"); } set { settingsChanged = true; SetProperty<bool>("authmd5", value); } }
        public bool authscram { get { return GetProperty<bool>("authscram"); } set { settingsChanged = true; SetProperty<bool>("authscram", value); } }
        public bool authoauth2 { get { return GetProperty<bool>("authoauth2"); } set { settingsChanged = true; SetProperty<bool>("authoauth2", value); } }
        public bool requestConnectedStandby { get { return GetProperty<bool>("requestConnectedStandby"); } set { settingsChanged = true; SetProperty<bool>("requestConnectedStandby", value); } }

        private bool settingsChanged { get { return GetProperty<bool>("settingsChanged"); } set { SetProperty<bool>("settingsChanged", value); } }

        public string password
        {
            get 
            {
                if (!string.IsNullOrEmpty(jid))
                {
                    PasswordVault vault = new PasswordVault();
                    var resources = vault.RetrieveAll();
                    foreach (var res in resources)
                    {
                        if (res.UserName == this.jid)
                        {
                            res.RetrievePassword();
                            return res.Password;
                        }
                    }
                }

                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(jid))
                {
                    PasswordVault vault = new PasswordVault();
                    var resources = vault.RetrieveAll();
                    foreach (var res in resources)
                    {
                        if (res.UserName == this.jid)
                        {
                            res.Password = value;
                            return;
                        }
                    }

                    var newcredential = new PasswordCredential("chat", this.jid, value);
                    vault.Add(newcredential);

                    this.settingsChanged = true;
                }
            }
        }

        // Returns changed state and resets it
        public bool ResetChangedState()
        {
            bool changed = settingsChanged;
            settingsChanged = false;
            return changed;
        }

        public AccountState persistantState {
            get 
            {
                try
                {
                    return (AccountState)Enum.Parse(typeof(AccountState), GetString("persistantState"), true);
                }
                catch
                {
                    SetString("persistantState", AccountState.Disabled.ToString());
                    return AccountState.Disabled;
                }

            }
            set { settingsChanged = true; SetString("persistantState", value.ToString()); }
        }

        public bool forceDisabled { get { return GetProperty<bool>("forceDisabled"); } set { SetProperty<bool>("forceDisabled", value); } }

        #endregion

        public bool IsValid()
        {
            if (
                string.IsNullOrEmpty(title) ||
                string.IsNullOrEmpty(host) ||
                string.IsNullOrEmpty(jid) ||
                string.IsNullOrEmpty(password) 
                )
            {
                return false;
            }
            else
            {   
                return true;
            }
        }
    }
}
