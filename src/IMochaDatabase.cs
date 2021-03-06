﻿using System;
using MochaDB.Connection;

namespace MochaDB {
    /// <summary>
    /// Interface for MochaDB database managers.
    /// </summary>
    public interface IMochaDatabase:IDisposable {
        #region Events

        event EventHandler<EventArgs> Changing;
        event EventHandler<EventArgs> Changed;

        #endregion

        #region Methods

        void Connect();
        void Disconnect();

        void AddSector(MochaSector sector);
        bool RemoveSector(string name);
        MochaSector GetSector(string name);
        bool ExistsSector(string name);
        void AddSectorAttribute(string name,IMochaAttribute attr);
        IMochaAttribute GetSectorAttribute(string name,string attrname);
        bool RemoveSectorAttribute(string name,string attrname);

        void AddStack(MochaStack stack);
        bool RemoveStack(string name);
        MochaStack GetStack(string name);
        bool ExistsStack(string name);
        void AddStackAttribute(string name,IMochaAttribute attr);
        IMochaAttribute GetStackAttribute(string name,string attrname);
        bool RemoveStackAttribute(string name,string attrname);

        void AddStackItem(string name,string path,MochaStackItem item);
        bool RemoveStackItem(string name,string path);
        MochaStackItem GetStackItem(string name,string path);
        bool ExistsStackItem(string name,string path);
        void AddStackItemAttribute(string name,string path,IMochaAttribute attr);
        IMochaAttribute GetStackItemAttribute(string name,string path,string attrname);
        bool RemoveStackItemAttribute(string name,string path,string attrname);

        void AddTable(MochaTable table);
        bool RemoveTable(string name);
        MochaTable GetTable(string name);
        bool ExistsTable(string name);
        void AddTableAttribute(string name,IMochaAttribute attr);
        IMochaAttribute GetTableAttribute(string name,string attrname);
        bool RemoveTableAttribute(string name,string attrname);

        void AddColumn(string tableName,MochaColumn column);
        bool RemoveColumn(string tableName,string name);
        MochaColumn GetColumn(string tableName,string name);
        bool ExistsColumn(string tableName,string name);
        void AddColumnAttribute(string tableName,string name,IMochaAttribute attr);
        IMochaAttribute GetColumnAttribute(string tableName,string name,string attrname);
        bool RemoveColumnAttribute(string tableName,string name,string attrname);

        void AddRow(string tableName,MochaRow row);
        bool RemoveRow(string tableName,int index);
        MochaRow GetRow(string tableName,int index);

        void AddData(string tableName,string columnName,MochaData data);
        void UpdateData(string tableName,string columnName,int index,object data);
        MochaData GetData(string tableName,string columnName,int index);

        void ClearLogs();
        void RestoreToLog(string id);

        #endregion

        #region Properties

        MochaProvider Provider { get; }
        MochaConnectionState State { get; }
        string Name { get; }

        #endregion
    }
}
