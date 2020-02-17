﻿using MochaDB.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MochaDB.Querying {
    /// <summary>
    /// It offers query usage and management with MochaQ.
    /// </summary>
    [Serializable]
    public class MochaQuery:IMochaQuery {
        #region Fields

        private MochaDatabase db;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new MochaQuery.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MochaDB database.</param>
        /// <param name="embedded">Is embedded query.</param>
        internal MochaQuery(MochaDatabase db,bool embedded) {
            this.db = db;
            MochaQ = "RETURNQUERY";
            IsDatabaseEmbedded=embedded;
        }

        /// <summary>
        /// Create new MochaQuery.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MochaDB database.</param>
        public MochaQuery(MochaDatabase db) {
            Database = db;
            MochaQ = "RETURNQUERY";
        }

        /// <summary>
        /// Create new MochaQuery.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MOchaDB database.</param>
        /// <param name="mochaQ">MochaQuery to use.</param>
        public MochaQuery(MochaDatabase db,string mochaQ)
            : this(db) {
            MochaQ = mochaQ;
        }

        #endregion

        #region Dynamic

        /// <summary>
        /// If the value is returned, it returns the function and performs the function; if not, it just performs the function.
        /// </summary>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public IMochaResult Dynamic(string mochaQ) {
            MochaQ = mochaQ;
            return Dynamic();
        }

        /// <summary>
        /// If the value is returned, it returns the function and performs the function; if not, it just performs the function.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MochaDB database.</param>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public IMochaResult Dynamic(MochaDatabase db,string mochaQ) {
            Database = db;
            MochaQ = mochaQ;
            return Dynamic();
        }

        /// <summary>
        /// If the value is returned, it returns the function and performs the function; if not, it just performs the function.
        /// </summary>
        public IMochaResult Dynamic() {
            if(!MochaQ.IsDynamicQuery())
                throw new Exception(@"This MochaQ command is not ""Dynamic"" type command.");

            if(Database.State!=MochaConnectionState.Connected)
                throw new Exception("Connection is not open!");

            //Check BREAKQUERY.
            if(MochaQ.Command.Contains("BREAKQUERY"))
                return null;

            try {
                string[] parts = MochaQ.Command.Split(' ');
                parts[0]=parts[0].ToUpperInvariant();
                parts[2]=parts[2].ToUpperInvariant();

                if(parts[0] == "SELECT") {

                    string[] selectedColumns = parts[1].Split(',');

                    if(parts[2] == "FROM") {
                        string tableName = parts[3];

                        MochaTable table = new MochaTable(tableName);

                        for(int index = 0; index < selectedColumns.Length; index++) {
                            table.Columns.Add(Database.GetColumn(tableName,selectedColumns[index]));
                        }

                        return new MochaResult<MochaTable>(table);

                    } else
                        throw new Exception("Table not specified!");
                } else
                    throw new Exception("The first syntax is wrong, there is no such function.");

            } catch { return null; }
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs the active MochaQ query. Even if there is an incoming value, it will not return.
        /// </summary>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public void Run(string mochaQ) {
            MochaQ = mochaQ;
            Run();
        }

        /// <summary>
        /// Runs the active MochaQ query. Even if there is an incoming value, it will not return.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MochaDB database.</param>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public void Run(MochaDatabase db,string mochaQ) {
            Database = db;
            MochaQ = mochaQ;
            Run();
        }

        /// <summary>
        /// Runs the active MochaQ query. Even if there is an incoming value, it will not return.
        /// </summary>
        public void Run() {
            if(!MochaQ.IsRunQuery())
                throw new Exception(@"This MochaQ command is not ""Run"" type command.");

            if(Database.State!=MochaConnectionState.Connected)
                throw new Exception("Connection is not open!");

            //Check null.
            if(string.IsNullOrEmpty(MochaQ))
                throw new Exception("This MochaQ query is empty, invalid!");

            //Check BREAKQUERY.
            if(MochaQ.Command.Contains("BREAKQUERY"))
                return;

            string[] queryPaths = MochaQ.Command.Split(':');
            queryPaths[0]=queryPaths[0].ToUpperInvariant();

            if(queryPaths.Length == 1) {
                if(queryPaths[0] == "RESETMOCHA") {
                    Database.Reset();
                    return;
                } else if(queryPaths[0] == "RESETTABLES") {
                    IEnumerable<XElement> tableRange = Database.Doc.Root.Element("Tables").Elements();
                    for(int index = 0; index < tableRange.Count(); index++) {
                        tableRange.ElementAt(index).Elements().Remove();
                    }

                    Database.Save();
                    return;
                } else if(queryPaths[0] == "CLEARSECTORS") {
                    Database.ClearSectors();
                    return;
                } else if(queryPaths[0] == "CLEARSTACKS") {
                    Database.ClearStacks();
                    return;
                } else if(queryPaths[0] == "CLEARTABLES") {
                    Database.ClearTables();
                    return;
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 2) {
                if(queryPaths[0] == "REMOVETABLE") {
                    Database.RemoveTable(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "CREATETABLE") {
                    Database.CreateTable(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "CREATESTACK") {
                    Database.AddStack(new MochaStack(queryPaths[1]));
                    return;
                } else if(queryPaths[0] == "REMOVESECTOR") {
                    Database.RemoveSector(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "REMOVESTACK") {
                    Database.RemoveStack(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "SETPASSWORD") {
                    Database.SetPassword(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "SETDESCRIPTION") {
                    Database.SetDescription(queryPaths[1]);
                    return;
                } else if(queryPaths[0] == "RESETTABLE") {
                    if(!Database.ExistsTable(queryPaths[1]))
                        throw new Exception("Table not found in this name!");

                    Database.Doc.Root.Element("Tables").Elements(queryPaths[1]).Elements().Remove();
                    Database.Save();
                    return;
                } else if(queryPaths[0] == "CREATEMOCHA") {
                    MochaDatabase.CreateMochaDB(Path.Combine(queryPaths[1]) + ".bjay",string.Empty,string.Empty);
                    return;
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 3) {
                if(queryPaths[0] == "REMOVECOLUMN") {
                    Database.RemoveColumn(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "SETSECTORDATA") {
                    Database.SetSectorData(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "SETSECTORDESCRIPTION") {
                    Database.SetSectorDescription(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "SETSTACKDESCRIPTION") {
                    Database.SetStackDescription(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "SETTABLEDESCRIPTION") {
                    Database.SetTableDescription(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "REMOVEROW") {
                    Database.RemoveRow(queryPaths[1],int.Parse(queryPaths[2]));
                    return;
                } else if(queryPaths[0] == "RENAMETABLE") {
                    Database.RenameTable(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "REMOVESTACKITEM") {
                    Database.RemoveStackItem(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "RENAMESECTOR") {
                    Database.RenameSector(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "RENAMESTACK") {
                    Database.RenameStack(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "CREATECOLUMN") {
                    Database.CreateColumn(queryPaths[1],queryPaths[2]);
                    return;
                } else if(queryPaths[0] == "CREATEMOCHA") {
                    MochaDatabase.CreateMochaDB(Path.Combine(queryPaths[1] + queryPaths[2]) + ".bjay",string.Empty,string.Empty);
                    return;
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 4) {
                if(queryPaths[0] == "RENAMECOLUMN") {
                    Database.RenameColumn(queryPaths[1],queryPaths[2],queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "SETCOLUMNDESCRIPTION") {
                    Database.SetColumnDescription(queryPaths[1],queryPaths[2],queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "SETCOLUMNDATATYPE") {
                    Database.SetColumnDataType(queryPaths[1],queryPaths[2],MochaData.GetDataTypeFromName(queryPaths[3]));
                    return;
                } else if(queryPaths[0] == "ADDDATA") {
                    Database.AddData(queryPaths[1],queryPaths[2],MochaData.GetDataFromString(Database.GetColumnDataType(queryPaths[1],queryPaths[2])
                        ,queryPaths[3]));
                    return;
                } else if(queryPaths[0] == "CREATESTACKITEM") {
                    Database.AddStackItem(queryPaths[1],queryPaths[3],new MochaStackItem(queryPaths[2]));
                    return;
                } else if(queryPaths[0] == "RENAMESTACKITEM") {
                    Database.RenameStackItem(queryPaths[1],queryPaths[2],queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "SETSTACKITEMVALUE") {
                    Database.SetStackItemValue(queryPaths[1],queryPaths[2],queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "SETSTACKITEMDESCRIPTION") {
                    Database.SetStackItemDescription(queryPaths[1],queryPaths[2],queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "UPDATEFIRSTDATA") {
                    Database.UpdateData(queryPaths[1],queryPaths[2],0,queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "UPDATELASTDATA") {
                    Database.UpdateData(queryPaths[1],queryPaths[2],Database.GetDataCount(queryPaths[1],queryPaths[2]) - 1,queryPaths[3]);
                    return;
                } else if(queryPaths[0] == "ADDSECTOR") {
                    MochaSector Sector = new MochaSector(queryPaths[1],queryPaths[2],queryPaths[3]);
                    Database.AddSector(Sector);
                    return;
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 5) {
                if(queryPaths[0] == "UPDATEDATA") {
                    Database.UpdateData(queryPaths[1],queryPaths[2],int.Parse(queryPaths[3]),queryPaths[4]);
                    return;
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            }
        }

        #endregion

        #region GetRun

        /// <summary>
        /// Runs the active MochaQ query. Returns the incoming value.
        /// </summary>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public IMochaResult GetRun(string mochaQ) {
            MochaQ = mochaQ;
            return GetRun();
        }

        /// <summary>
        /// Runs the active MochaQ query. Returns the incoming value.
        /// </summary>
        /// <param name="db">MochaDatabase object that provides management of the targeted MochaDB database.</param>
        /// <param name="mochaQ">MochaQ to be set as the active MochaQ Query.</param>
        public IMochaResult GetRun(MochaDatabase db,string mochaQ) {
            Database = db;
            MochaQ = mochaQ;
            return GetRun();
        }

        /// <summary>
        /// Runs the active MochaQ query. Returns the incoming value.
        /// </summary>
        public IMochaResult GetRun() {
            if(!MochaQ.IsGetRunQuery())
                throw new Exception(@"This MochaQ command is not ""GetRun"" type command.");

            if(Database.State!=MochaConnectionState.Connected)
                throw new Exception("Connection is not open!");

            //Check null.
            if(string.IsNullOrEmpty(MochaQ))
                throw new Exception("This MochaQ query is empty, invalid!");

            //Check BREAKQUERY.
            if(MochaQ.Command.Contains("BREAKQUERY"))
                return null;

            string[] queryPaths = MochaQ.Command.Split(':');
            queryPaths[0]=queryPaths[0].ToUpperInvariant();

            if(queryPaths.Length == 1) {
                if(queryPaths[0] == "GETTABLES") {
                    return Database.GetTables();
                } else if(queryPaths[0] == "GETPASSWORD") {
                    return Database.GetPassword();
                } else if(queryPaths[0] == "GETDESCRIPTION") {
                    return Database.GetDescription();
                } else if(queryPaths[0] == "GETDATAS") {
                    List<MochaData> datas = new List<MochaData>();
                    IEnumerable<XElement> tableRange = Database.Doc.Root.Element("Tables").Elements();
                    for(int index = 0; index < tableRange.Count(); index++) {
                        datas.AddRange(GETDATAS(tableRange.ElementAt(index).Name.LocalName).collection);
                    }
                    return new MochaCollectionResult<MochaData>(datas);
                } else if(queryPaths[0] == "TABLECOUNT") {
                    return new MochaResult<int>(Database.Doc.Root.Elements().Count());
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 2) {
                if(queryPaths[0] == "GETTABLE") {
                    return Database.GetTable(queryPaths[1]);
                } else if(queryPaths[0] == "GETCOLUMNS") {
                    return Database.GetColumns(queryPaths[1]);
                } else if(queryPaths[0] == "GETSECTOR") {
                    return Database.GetSector(queryPaths[1]);
                } else if(queryPaths[0] == "GETFIRSTCOLUMN_NAME") {
                    return new MochaResult<string>(GETFIRSTCOLUMN_NAME(queryPaths[1]));
                } else if(queryPaths[0] == "GETROWS") {
                    return Database.GetRows(queryPaths[1]);
                } else if(queryPaths[0] == "GETDATAS") {
                    return GETDATAS(queryPaths[1]);
                } else if(queryPaths[0] == "GETSECTORDATA") {
                    return Database.GetSectorData(queryPaths[1]);
                } else if(queryPaths[0] == "GETSECTORDESCRIPTION") {
                    return Database.GetSectorDescription(queryPaths[1]);
                } else if(queryPaths[0] == "GETTABLEDESCRIPTION") {
                    return Database.GetTableDescription(queryPaths[1]);
                } else if(queryPaths[0] == "GETSTACKDESCRIPTION") {
                    return Database.GetStackDescription(queryPaths[1]);
                } else if(queryPaths[0] == "COLUMNCOUNT") {
                    return COLUMNCOUNT(queryPaths[1]);
                } else if(queryPaths[0] == "ROWCOUNT") {
                    try {
                        return new MochaResult<int>(Database.Doc.Root.Element("Tables").Elements(queryPaths[1]).Elements(
                            GETFIRSTCOLUMN_NAME(queryPaths[1]).Value).Elements().Count());
                    } catch(Exception excep) {
                        throw excep;
                    }
                } else if(queryPaths[0] == "DATACOUNT") {
                    return new MochaResult<int>(Database.GetDataCount(queryPaths[1],GETFIRSTCOLUMN_NAME(queryPaths[1]))
                        * COLUMNCOUNT(queryPaths[1]));
                } else if(queryPaths[0] == "EXISTSTABLE") {
                    return Database.ExistsTable(queryPaths[1]);
                } else if(queryPaths[0] == "EXISTSSECTOR") {
                    return Database.ExistsSector(queryPaths[1]);
                } else if(queryPaths[0] == "EXISTSSTACK") {
                    return Database.ExistsStack(queryPaths[1]);
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 3) {
                if(queryPaths[0] == "GETCOLUMN") {
                    return Database.GetColumn(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "DATACOUNT") {
                    return Database.GetDataCount(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "EXISTSCOLUMN") {
                    return Database.ExistsColumn(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "EXISTSSTACKITEM") {
                    return Database.ExistsStackItem(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "GETSTACKITEMVALUE") {
                    return Database.GetStackItemValue(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "GETSTACKITEMDESCRIPTION") {
                    return Database.GetStackItemDescription(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "GETDATAS") {
                    return Database.GetDatas(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "GETSTACKITEM") {
                    return Database.GetStackItem(queryPaths[1],queryPaths[2]);
                } else if(queryPaths[0] == "GETCOLUMNDESCRIPTION") {
                    return Database.GetColumnDescription(queryPaths[1],queryPaths[2]);
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else if(queryPaths.Length == 4) {
                if(queryPaths[0] == "EXISTSDATA") {
                    return Database.ExistsData(queryPaths[1],queryPaths[2],queryPaths[3]);
                } else
                    throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
            } else
                throw new Exception("Invalid query. The content of the query could not be processed, wrong!");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Return column count of table.
        /// </summary>
        /// <param name="name">Name of table.</param>
        private MochaResult<int> COLUMNCOUNT(string name) {
            if(!Database.ExistsTable(name))
                throw new Exception("Table not found in this name!");

            IEnumerable<XElement> columnElements = Database.Doc.Root.Element("Tables").Elements(name).Elements();
            return columnElements.Count();
        }

        /// <summary>
        /// Return all datas of table.
        /// </summary>
        /// <param name="name">Name of table.</param>
        private MochaCollectionResult<MochaData> GETDATAS(string name) {
            if(!Database.ExistsTable(name))
                throw new Exception("Table not found in this name!");

            List<MochaData> datas = new List<MochaData>();

            IEnumerable<XElement> columnRange = Database.Doc.Root.Element("Tables").Elements(name).Elements();
            for(int index = 0; index < columnRange.Count(); index++) {
                datas.AddRange(Database.GetDatas(name,columnRange.ElementAt(index).Name.LocalName).collection);
            }

            return new MochaCollectionResult<MochaData>(datas);
        }

        /// <summary>
        /// Return first column name of table.
        /// </summary>
        /// <param name="name">Name of table.</param>
        private MochaResult<string> GETFIRSTCOLUMN_NAME(string name) {
            if(!Database.ExistsTable(name))
                throw new Exception("Table not found in this name!");

            XElement firstColumn = (XElement)Database.Doc.Root.Element("Tables").Element(name).FirstNode;
            return firstColumn == null ? null : firstColumn.Name.LocalName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Is embedded query.
        /// </summary>
        public bool IsDatabaseEmbedded { get; private set; }

        /// <summary>
        /// MochaDatabse object that provides management of the targeted MochaDB database.
        /// </summary>
        public MochaDatabase Database {
            get =>
                db;
            set {
                if(IsDatabaseEmbedded)
                    throw new Exception("This is embedded in database, can not set database!");

                if(db == null)
                    throw new Exception("This MochaDatabase is not affiliated with a MochaDB!");

                if(value == db)
                    return;


                db = value;
            }
        }

        /// <summary>
        /// Active MochaQ query.
        /// </summary>
        public MochaQCommand MochaQ { get; set; }

        #endregion
    }
}