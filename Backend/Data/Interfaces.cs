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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Backend.Data
{
    public class ObservableDictionary<K, V> : IObservableMap<K, V>
    {
        private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<K>
        {
            public ObservableDictionaryChangedEventArgs(CollectionChange change, K key)
            {
                this.CollectionChange = change;
                this.Key = key;
            }

            public CollectionChange CollectionChange { get; private set; }
            public K Key { get; private set; }
        }

        private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
        public event MapChangedEventHandler<K, V> MapChanged;

        private void InvokeMapChanged(CollectionChange change, K key)
        {
            var eventHandler = MapChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new ObservableDictionaryChangedEventArgs(CollectionChange.ItemInserted, key));
            }
        }

        public void Add(K key, V value)
        {
            this._dictionary.Add(key, value);
            this.InvokeMapChanged(CollectionChange.ItemInserted, key);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Remove(K key)
        {
            if (this._dictionary.Remove(key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            V currentValue;
            if (this._dictionary.TryGetValue(item.Key, out currentValue) &&
                Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                return true;
            }
            return false;
        }

        public V this[K key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                this._dictionary[key] = value;
                this.InvokeMapChanged(CollectionChange.ItemChanged, key);
            }
        }

        public void Clear()
        {
            var priorKeys = this._dictionary.Keys.ToArray();
            this._dictionary.Clear();
            foreach (var key in priorKeys)
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
            }
        }

        public ICollection<K> Keys
        {
            get { return this._dictionary.Keys; }
        }

        public bool ContainsKey(K key)
        {
            return this._dictionary.ContainsKey(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        public ICollection<V> Values
        {
            get { return this._dictionary.Values; }
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return this._dictionary.Contains(item);
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            int arraySize = array.Length;
            foreach (var pair in this._dictionary)
            {
                if (arrayIndex >= arraySize) break;
                array[arrayIndex++] = pair;
            }
        }
    }


    public class IStoreInterface
    {
        protected readonly string _containerName;
        protected readonly ApplicationDataContainer _containerParent;
        protected ApplicationDataContainer _container { get { return _containerParent.CreateContainer(_containerName, ApplicationDataCreateDisposition.Always); } }

        public string Key { get { return _containerName; } }
        public ApplicationDataContainer Value { get { return _container; } }

        public ApplicationDataContainer CreateChild(string name)
        {
            return _container.CreateContainer(name, ApplicationDataCreateDisposition.Always);
        }

        public void Delete()
        {
            _containerParent.DeleteContainer(_containerName);
        }

        public IStoreInterface()
        {
            _containerParent = ApplicationData.Current.LocalSettings;
            _containerName = this.GetType().Name;
        }
        public IStoreInterface(string name)
        {
            _containerParent = ApplicationData.Current.LocalSettings;
            _containerName = name;
        }

        public IStoreInterface(ApplicationDataContainer parent)
        {
            _containerParent = parent;
            _containerName = this.GetType().Name;
        }

        public IStoreInterface(ApplicationDataContainer parent, string name)
        {
            _containerParent = parent;
            _containerName = name;
        }
    }

    public class IStoreCollectionInterface<T> : Collection<T>
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected string NewGuid { get { return Guid.NewGuid().ToString(); } }

        protected readonly string _containerName;
        protected readonly ApplicationDataContainer _containerParent;
        protected ApplicationDataContainer _container { get { return _containerParent.CreateContainer(_containerName, ApplicationDataCreateDisposition.Always); } }

        public string Key { get { return _containerName; } }
        public ApplicationDataContainer Value { get { return _container; } }

        public ApplicationDataContainer CreateChild(string name)
        {
            return _container.CreateContainer(name, ApplicationDataCreateDisposition.Always);
        }

        public void Delete()
        {
            _containerParent.DeleteContainer(_containerName);
        }

        public IStoreCollectionInterface()
        {
            _containerParent = ApplicationData.Current.LocalSettings;
            _containerName = this.GetType().Name;
        }

        public IStoreCollectionInterface(string name)
        {
            _containerParent = ApplicationData.Current.LocalSettings;
            _containerName = name;
        }

        public IStoreCollectionInterface(ApplicationDataContainer parent)
        {
            _containerParent = parent;
            _containerName = this.GetType().Name;
        }

        public IStoreCollectionInterface(ApplicationDataContainer parent, string name)
        {
            _containerParent = parent;
            _containerName = name;
        }

        protected void EmitCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
                this.CollectionChanged(sender, e);
        }
    }

    public class IMixedStore : IStoreInterface, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IMixedStore() : base() { Init(); }
        public IMixedStore(string name) : base(name) { Init(); }
        public IMixedStore(ApplicationDataContainer parent) : base(parent) { Init(); }
        public IMixedStore(ApplicationDataContainer parent, string name) : base(parent, name) { Init(); }

        private Dictionary<string, object> _internalCache = new Dictionary<string, object>();

        private void Init()
        {
            _containerParent.CreateContainer(_containerName, ApplicationDataCreateDisposition.Always);

            foreach (var item in this._container.Values)
                _internalCache.Add(item.Key, item.Value);
        }

        public void SetDefault(string name, object value)
        {
            if (!this._internalCache.ContainsKey(name))
                this._internalCache.Add(name, value);

            if (!this._container.Values.ContainsKey(name))
                this._container.Values[name] = value;
        }

        public void RemoveProperty(string name)
        {
            if (this._internalCache.ContainsKey(name))
                this._internalCache.Remove(name);

            if (this._container.Values[name] != null)
                this._container.Values.Remove(name);
        }

        public T GetProperty<T>(string name)
        {
            if (this._internalCache.ContainsKey(name))
                return (T)this._internalCache[name];

            if (this._container.Values.ContainsKey(name))
                return (T)this._container.Values[name];

            return default(T);
        }

        public bool SetProperty<T>(string name, T value, [CallerMemberName] string propertyName = null)
        {
            if (!this._internalCache.ContainsKey(name))
                this._internalCache.Add(name, value);
            else
                this._internalCache[name] = value;

            _container.Values[name] = value;
            this.EmitPropertyChanged(name);
            return true;
        }

        public string GetString(string name)
        {
            return GetProperty<string>(name);
        }

        public bool SetString(string name, string value, [CallerMemberName] string propertyName = null)
        {
            return SetProperty<string>(name, value, propertyName);
        }

        protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class IStore<T> : IStoreInterface, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IStore() : base() { Init(); }
        public IStore(string name) : base(name) { Init(); }
        public IStore(ApplicationDataContainer parent) : base(parent) { Init(); }
        public IStore(ApplicationDataContainer parent, string name) : base(parent, name) { Init(); }

        private Dictionary<string, T> _internalCache = new Dictionary<string, T>();

        private void Init()
        {
            _containerParent.CreateContainer(_containerName, ApplicationDataCreateDisposition.Always);

            foreach (var item in this._container.Values)
                _internalCache.Add(item.Key, (T)item.Value);

        }

        public T this[string key]
        {
            get
            {
                return GetProperty(key);
            }
            set
            {
                if (value != null)
                    SetProperty(key, value);
                else
                    RemoveProperty(key);
            }
        }

        public void SetDefault(string name, T value)
        {
            if (!this._internalCache.ContainsKey(name))
                this._internalCache.Add(name, value);

            if (!this._container.Values.ContainsKey(name))
                this._container.Values[name] = value;
        }

        public void RemoveProperty(string name)
        {
            if (this._internalCache.ContainsKey(name))
                this._internalCache.Remove(name);

            if (this._container.Values[name] != null)
                this._container.Values.Remove(name);
        }

        public T GetProperty(string name)
        {
            if (this._internalCache.ContainsKey(name))
                return this._internalCache[name];

            if (this._container.Values.ContainsKey(name))
                return (T)this._container.Values[name];

            return default(T);
        }

        public bool SetProperty(string name, T value, [CallerMemberName] string propertyName = null)
        {
            if (!this._internalCache.ContainsKey(name))
                this._internalCache.Add(name, value);
            else
                this._internalCache[name] = value;

            _container.Values[name] = value;
            this.EmitPropertyChanged(name);
            return true;
        }

        protected void EmitPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class IValueStore<T> : IStoreCollectionInterface<T>, INotifyCollectionChanged
    {
        private bool _updateing = false;

        public IValueStore() : base() { Update(); }
        public IValueStore(string name) : base(name) { Update(); }
        public IValueStore(ApplicationDataContainer parent) : base(parent) { Update(); }
        public IValueStore(ApplicationDataContainer parent, string name) : base(parent, name) { Update(); }

        public void Update()
        {
            _updateing = true;
            this.Clear();
            _updateing = false;

            foreach (var item in _container.Values)
                Add((T)item.Value);
        }

        public T CreateItem()
        {
            T child = (T)Activator.CreateInstance(typeof(T));
            Add(child);
            return child;
        }

        protected override void ClearItems()
        {
            if( !_updateing )
                _container.Values.Clear();
                
            base.ClearItems();
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, T item)
        {
            _container.Values[index.ToString()] = item;

            base.InsertItem(index, item);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {

            _container.Values.Remove(index.ToString());

            var itemTemp = this[index];

            base.RemoveItem(index);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemTemp, index));
        }

        protected override void SetItem(int index, T item)
        {
            _container.Values[index.ToString()] = item;

            base.SetItem(index, item);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
        }
    }

    public class ICollectionStore<T> : IStoreCollectionInterface<T>, INotifyCollectionChanged where T : IStoreInterface
    {
        public ICollectionStore() : base() { Update(); }
        public ICollectionStore(string name) : base(name) { Update(); }
        public ICollectionStore(ApplicationDataContainer parent) : base(parent) { Update(); }
        public ICollectionStore(ApplicationDataContainer parent, string name) : base(parent, name) { Update(); }

        public void Update()
        {
            // Collect obsolete items
            List<string> removeList = new List<string>();
            foreach (var child in this)
            {
                if (!_container.Containers.ContainsKey(child.Key))
                    removeList.Add(child.Key);
            }

            // The remove itself
            foreach( var key in removeList )
                RemoveItem(key);
            
            // Add all missing
            foreach (var item in _container.Containers)
            {
                if (IndexOf(item.Key) == -1)
                    Add((T)Activator.CreateInstance(typeof(T), new object[] { _container, item.Key }));
            }
        }

        public bool ContainsKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var encKey = Helper.EncodeBASE64(key);
                if (FindItem(encKey) == null)
                    return false;
                else
                    return true;
            }
            return false;
        }

        public T CreateItem()
        {
            var encKey = Helper.EncodeBASE64(NewGuid);
            return CreateItem(encKey);
        }

        public T CreateItem(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var encKey = Helper.EncodeBASE64(key);
                T child = (T)Activator.CreateInstance(typeof(T), new object[] { _container, encKey });
                Add(child);
                return child;
            }
            return null;
        }

        public T this[string key]
        {
            get
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var encKey = Helper.EncodeBASE64(key);
                    return FindItem(encKey);
                }

                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var encKey = Helper.EncodeBASE64(key);
                    SetItem(encKey, value);
                }
            }
        }

        public void RemoveItem(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var encKey = Helper.EncodeBASE64(key);
                var index = IndexOf(encKey);
                if (index != -1)
                    RemoveItem(index);
            }
        }

        public void SetItem(string key, T value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var encKey = Helper.EncodeBASE64(key);
                var index = IndexOf(encKey);
                if (index != -1)
                    SetItem(index, value);
            }
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
                item.Delete();

            base.ClearItems();
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            this[index].Delete();
            var itemTemp = this[index];

            base.RemoveItem(index);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemTemp, index));
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            EmitCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
        }

        private T FindItem(string key)
        {
            foreach (var item in this)
            {
                if (item.Key == key)
                    return item;
            }
            return null;
        }

        private int IndexOf(string key)
        {
            foreach (var item in this)
            {
                if (item.Key == key)
                    return IndexOf(item);
            }
            return -1;
        }
    }
}
