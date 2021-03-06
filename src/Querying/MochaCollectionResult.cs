﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MochaDB.Querying {
    /// <summary>
    /// Base for MochaDB collection results.
    /// </summary>
    /// <typeparam name="T">Type of result value.</typeparam>
    public class MochaCollectionResult<T>:MochaReadonlyCollection<T>, IMochaCollectionResult<T> {
        #region Constructors

        /// <summary>
        /// Create new MochaCollectionResult.
        /// </summary>
        public MochaCollectionResult() {
            collection=new List<T>();
        }

        /// <summary>
        /// Create new MochaCollectionResult.
        /// </summary>
        /// <param name="collection">Items.</param>
        public MochaCollectionResult(IEnumerable<T> collection) {
            this.collection=new List<T>(collection);
        }

        #endregion

        #region Queryable

        /// <summary>
        /// Select items by query.
        /// </summary>
        /// <param name="query">Query to use in filtering.</param>
        public IEnumerable<T> Select(Func<T,T> query) =>
            collection.Select(query);

        /// <summary>
        /// Select items by query.
        /// </summary>
        /// <param name="query">Query to use in filtering.</param>
        public IEnumerable<T> Select(Func<T,int,T> query) =>
            collection.Select(query);

        /// <summary>
        /// Select items by condition.
        /// </summary>
        /// <param name="query">Query to use in conditioning.</param>
        public IEnumerable<T> Where(Func<T,bool> query) =>
            collection.Where(query);

        /// <summary>
        /// Select items by condition.
        /// </summary>
        /// <param name="query">Query to use in conditioning.</param>
        public IEnumerable<T> Where(Func<T,int,bool> query) =>
            collection.Where(query);

        /// <summary>
        /// Order items descending by query.
        /// </summary>
        /// <param name="query">Query to use in ordering.</param>
        public IEnumerable<T> OrderByDescending(Func<T,T> query) =>
            collection.OrderByDescending(query);

        /// <summary>
        /// Order items ascending by query.
        /// </summary>
        /// <param name="query">Query to use in ordering.</param>
        public IEnumerable<T> OrderBy(Func<T,T> query) =>
            collection.OrderBy(query);

        /// <summary>
        /// Group items by query.
        /// </summary>
        /// <param name="query">Query to use in grouping.</param>
        public IEnumerable<IGrouping<T,T>> GroupBy(Func<T,T> query) =>
            collection.GroupBy(query);

        #endregion

        #region Methods

        /// <summary>
        /// Return first element in collection.
        /// </summary>
        public override T GetFirst() =>
            IsEmptyCollection() ? throw new MochaException("Collection is empty!") : this[0];

        /// <summary>
        /// Return last element in collection.
        /// </summary>
        public override T GetLast() =>
            IsEmptyCollection() ? throw new MochaException("Collection is empty!") : this[MaxIndex()];

        #endregion

        #region Properties

        /// <summary>
        /// This is collection result.
        /// </summary>
        public bool IsCollectionResult =>
            true;

        #endregion
    }
}
