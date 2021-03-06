﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MochaDB {
    /// <summary>
    /// MochaData collector for MochaColumns.
    /// </summary>
    public class MochaColumnDataCollection:MochaReadonlyCollection<MochaData> {
        #region Fields

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
        protected virtual void OnChanged(object sender,EventArgs e) {
            //Invoke.
            Changed?.Invoke(this,e);
        }

        #endregion

        #region Methods

        #region Internal

        /// <summary>
        /// Remove all items.
        /// </summary>
        internal void Clear() {
            if(collection.Count ==0)
                return;

            collection.Clear();
            //OnChanged(this,new EventArgs());
        }

        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(MochaData item) {
            if(DataType==MochaDataType.AutoInt)
                throw new MochaException("Data cannot be added directly to a column with AutoInt!");
            if(item.DataType == MochaDataType.Unique && !string.IsNullOrEmpty(item.Data.ToString()))
                if(ContainsData(item.Data))
                    throw new MochaException("Any value can be added to a unique column only once!");

            if(item.DataType == DataType) {
                collection.Add(item);
                //Changed?.Invoke(this,new EventArgs());
            } else
                throw new MochaException("This data's datatype not compatible column datatype.");
        }

        /// <summary>
        /// Add data.
        /// </summary>
        /// <param name="data">Data to add.</param>
        internal void AddData(object data) {
            if(MochaData.IsType(DataType,data))
                Add(new MochaData(DataType,data));
            else
                throw new MochaException("This data's datatype not compatible column datatype.");
        }

        /// <summary>
        /// Add item from range.
        /// </summary>
        /// <param name="items">Range to add items.</param>
        internal void AddRange(IEnumerable<MochaData> items) {
            for(int index = 0; index < items.Count(); index++)
                Add(items.ElementAt(index));
        }

        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        internal void Remove(MochaData item) {
            collection.Remove(item);
            /*if(collection.Remove(item))
                OnChanged(this,new EventArgs());*/
        }

        /// <summary>
        /// Removes all data equal to sample data.
        /// </summary>
        /// <param name="data">Sample data.</param>
        internal void RemoveAllData(object data) {
            int count = collection.Count;
            collection = (
                from currentdata in collection
                where currentdata.Data != data
                select currentdata).ToList();

            /*if(collection.Count != count)
                OnChanged(this,new EventArgs());*/
        }

        /// <summary>
        /// Remove item by index.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        internal void RemoveAt(int index) {
            collection.RemoveAt(index);
            //OnChanged(this,new EventArgs());
        }

        #endregion

        /// <summary>
        /// Return true if data is contained but return false if not exists.
        /// </summary>
        /// <param name="data">Data to check.</param>
        public bool ContainsData(object data) {
            for(int index = 0; index < Count; index++)
                if(data ==this[index])
                    return true;

            return false;
        }

        /// <summary>
        /// Return first element in collection.
        /// </summary>
        public override MochaData GetFirst() =>
            IsEmptyCollection() ? null : this[0];

        /// <summary>
        /// Return last element in collection.
        /// </summary>
        public override MochaData GetLast() =>
            IsEmptyCollection() ? null : this[MaxIndex()];

        #endregion

        #region Properties

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

        #endregion
    }
}
