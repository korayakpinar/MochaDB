﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MochaDB.Collections {
    /// <summary>
    /// MochaData collector for MochaColumns.
    /// </summary>
    public sealed class MochaColumnDataCollection:IMochaCollection<MochaData> {
        #region Fields

        internal List<MochaData> collection;
        private MochaDataType dataType;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new MochaColumnDataCollection.
        /// </summary>
        /// <param name="dataType">DataType of column.</param>
        public MochaColumnDataCollection(MochaDataType dataType) {
            collection=new List<MochaData>();
            this.dataType=dataType;
        }

        #endregion

        #region Events

        /// <summary>
        /// This happens after collection changed.
        /// </summary>
        public event EventHandler<EventArgs> Changed;
        private void OnChanged(object sender,EventArgs e) {
            //Invoke.
            Changed?.Invoke(this,new EventArgs());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Remove all items.
        /// </summary>
        public void Clear() {
            collection.Clear();
            OnChanged(this,new EventArgs());
        }

        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(MochaData item) {
            if(DataType==MochaDataType.AutoInt)
                throw new Exception("Data cannot be added directly to a column with AutoInt!");

            if(item.DataType == DataType) {
                collection.Add(item);
                Changed?.Invoke(this,new EventArgs());
            } else
                throw new Exception("This data's datatype not compatible column datatype.");
        }

        /// <summary>
        /// Add data.
        /// </summary>
        /// <param name="data">Data to add.</param>
        public void AddData(object data) {
            if(MochaData.IsType(DataType,data))
                AddData(new MochaData(DataType,data));
            else
                throw new Exception("This data's datatype not compatible column datatype.");
        }

        /// <summary>
        /// Add item from range.
        /// </summary>
        /// <param name="items">Range to add items.</param>
        public void AddRange(IEnumerable<MochaData> items) {
            for(int index = 0; index < items.Count(); index++)
                Add(items.ElementAt(index));
        }

        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(MochaData item) {
            if(collection.Remove(item))
                OnChanged(this,new EventArgs());
        }

        /// <summary>
        /// Removes all data equal to sample data.
        /// </summary>
        /// <param name="data">Sample data.</param>
        public void RemoveAllData(object data) {
            int count = collection.Count;
            collection = (
                from currentdata in collection
                where currentdata.Data != data
                select currentdata).ToList();

            if(collection.Count != count)
                OnChanged(this,new EventArgs());
        }

        /// <summary>
        /// Remove item by index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public void RemoveAt(int index) {
            collection.RemoveAt(index);
            OnChanged(this,new EventArgs());
        }

        /// <summary>
        /// Return index if index is find but return -1 if index is not find.
        /// </summary>
        /// <param name="item">Item to find index.</param>
        public int IndexOf(MochaData item) {
            return collection.IndexOf(item);
        }

        /// <summary>
        /// Return true if item is exists but return false if item not exists.
        /// </summary>
        /// <param name="item">Item to exists check.</param>
        public bool Contains(MochaData item) {
            return collection.Contains(item);
        }

        /// <summary>
        /// Return max index of item count.
        /// </summary>
        public int MaxIndex() =>
            collection.Count-1;

        #endregion

        #region Properties

        /// <summary>
        /// Return item by index.
        /// </summary>
        /// <param name="index">Index of item.</param>
        public MochaData this[int index] =>
            collection[index];

        /// <summary>
        /// Data type of column.
        /// </summary>
        public MochaDataType DataType {
            get =>
                dataType;
            internal set {
                if(value == dataType)
                    return;

                dataType = value;

                if(value == MochaDataType.AutoInt) {
                    return;
                }

                for(int index = 0; index < Count; index++)
                    collection[index].DataType = dataType;
            }
        }

        /// <summary>
        /// Count of data.
        /// </summary>
        public int Count =>
            collection.Count;

        #endregion
    }
}
