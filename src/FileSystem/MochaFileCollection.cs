﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MochaDB.FileSystem {
    /// <summary>
    /// MochaDb file system file collector.
    /// </summary>
    public class MochaFileCollection:MochaCollection<MochaFile> {
        #region Constructors

        /// <summary>
        /// Create new MochaFileCollection.
        /// </summary>
        public MochaFileCollection() {
            collection =new List<MochaFile>();
        }

        #endregion

        #region Events

        /// <summary>
        /// This happens after NameChanged event of any item in collection.
        /// </summary>
        public event EventHandler<EventArgs> FileNameChanged;
        private void OnFileNameChanged(object sender,EventArgs e) {
            //Invoke.
            FileNameChanged?.Invoke(sender,e);
        }

        /// <summary>
        /// This happens after NameChanged event of any item in collection.
        /// </summary>
        public event EventHandler<EventArgs> FileExtensionChanged;
        private void OnFileExtensionChanged(object sender,EventArgs e) {
            //Invoke.
            FileExtensionChanged?.Invoke(sender,e);
        }

        #endregion

        #region Item Events

        private void Item_NameChanged(object sender,EventArgs e) {
            var result = collection.Where(x => x.Name==(sender as IMochaFile).Name);
            if(result.Count()>1)
                throw new MochaException("There is already a file with this name and extension!");

            OnFileNameChanged(sender,e);
        }

        private void Item_ExtensionChanged(object sender,EventArgs e) {
            var result = collection.Where(x => x.Name==(sender as IMochaFile).Name);
            if(result.Count()>1)
                throw new MochaException("There is already a file with this name and extension!");

            OnFileExtensionChanged(sender,e);
        }

        #endregion

        #region Methods

        public override void Clear() {
            for(int index = 0; index < Count; index++) {
                collection[index].NameChanged-=Item_NameChanged;
                collection[index].ExtensionChanged-=Item_ExtensionChanged;
            }
            collection.Clear();
        }

        public override void Add(MochaFile item) {
            if(item == null)
                return;
            if(Contains(item.FullName))
                throw new MochaException("There is already a file with this name and extension!");

            item.NameChanged+=Item_NameChanged;
            item.ExtensionChanged+=Item_ExtensionChanged;
            collection.Add(item);
        }

        public override void AddRange(IEnumerable<MochaFile> items) {
            for(int index = 0; index < items.Count(); index++)
                Add(items.ElementAt(index));
        }

        public override void Remove(MochaFile item) {
            Remove(item.Name);
        }

        /// <summary>
        /// Remove item by name.
        /// </summary>
        /// <param name="fullName">FullName of item to remove.</param>
        public void Remove(string fullName) {
            for(int index = 0; index < Count; index++)
                if(collection[index].Name == fullName) {
                    collection[index].NameChanged-=Item_NameChanged;
                    collection[index].ExtensionChanged-=Item_ExtensionChanged;
                    collection.RemoveAt(index);
                    OnChanged(this,new EventArgs());
                    break;
                }
        }

        public override void RemoveAt(int index) {
            Remove(collection[index].Name);
        }

        /// <summary>
        /// Return index if index is find but return -1 if index is not find.
        /// </summary>
        /// <param name="fullName">FullName of item to find index.</param>
        public int IndexOf(string fullName) {
            for(int index = 0; index < Count; index++)
                if(this[index].FullName==fullName)
                    return index;
            return -1;
        }

        /// <summary>
        /// Return true if item is exists but return false if item not exists.
        /// </summary>
        /// <param name="fullName">FullName of item to exists check.</param>
        public bool Contains(string fullName) {
            return IndexOf(fullName) != -1;
        }

        public override MochaFile GetFirst() =>
            IsEmptyCollection() ? null : this[0];

        public override MochaFile GetLast() =>
            IsEmptyCollection() ? null : this[MaxIndex()];

        #endregion

        #region Properties

        /// <summary>
        /// Return item by name.
        /// </summary>
        /// <param name="name">Name of item.</param>
        public MochaFile this[string name] {
            get {
                int dex = IndexOf(name);
                if(dex!=-1)
                    return ElementAt(dex);
                return null;
            }
        }

        #endregion
    }
}
