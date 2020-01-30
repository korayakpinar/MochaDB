﻿//
// MIT License
//
// Copyright (c) 2020 Mertcan Davulcu
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using MochaDB.Encryptors;

namespace MochaDB {
    /// <summary>
    /// MochaDatabase provides management of a MochaDB database.
    /// </summary>
    [Serializable]
    public sealed class MochaDatabase:IDisposable {
        #region Fields

        FileStream sourceStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new MochaDatabase. If there is no MochaDB database file on the path, it will be created automatically.
        /// </summary>
        /// <param name="path">Directory path of MochaDB database.</param>
        public MochaDatabase(string path) {
            if(!IsMochaDB(path))
                throw new Exception("The file shown is not a MochaDB database file!");

            DBPath = path;
            ExistsAutoCreate();

            Doc = XDocument.Parse(Mocha_ACE.Decrypt(File.ReadAllText(DBPath,Encoding.UTF8)));

            if(!CheckMochaDB())
                throw new Exception("The MochaDB database is corrupt!");
            if(!string.IsNullOrEmpty(GetPassword()))
                throw new Exception("The MochaDB database is password protected!");

            FileInfo fInfo = new FileInfo(path);

            Name = fInfo.Name.Substring(0,fInfo.Name.Length - fInfo.Extension.Length);

            Query = new MochaQuery(this,true);
            sourceStream = File.Open(path,FileMode.Open,FileAccess.ReadWrite);
        }

        /// <summary>
        /// Creates a new MochaDatabase. If there is no MochaDB database file on the path, it will be created automatically.
        /// </summary>
        /// <param name="path">Directory path of MochaDB database.</param>
        /// <param name="password">Password of MochaDB database.</param>
        public MochaDatabase(string path,string password) {
            if(!IsMochaDB(path))
                throw new Exception("The file shown is not a MochaDB database file!");

            DBPath = path;
            ExistsAutoCreate();

            Doc = XDocument.Parse(Mocha_ACE.Decrypt(File.ReadAllText(DBPath,Encoding.UTF8)));

            if(!CheckMochaDB())
                throw new Exception("The MochaDB database is corrupt!");
            if(GetPassword() != password)
                throw new Exception("MochaDB database password does not match the password specified!");

            FileInfo fInfo = new FileInfo(path);

            Name = fInfo.Name.Substring(0,fInfo.Name.Length - fInfo.Extension.Length);

            Query = new MochaQuery(this,true);
            sourceStream = File.Open(path,FileMode.Open,FileAccess.ReadWrite);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns true if the file in the path is Mocha DB and false otherwise.
        /// </summary>
        /// <param name="path">Path to check.</param>
        public static bool IsMochaDB(string path) {
            if(!File.Exists(path))
                return false;

            FileInfo fInfo = new FileInfo(path);

            if(fInfo.Extension != ".mochadb")
                return false;

            return true;
        }

        /// <summary>
        /// Create new MochaDB database.
        /// </summary>
        /// <param name="path">The file path to be created. (Including name, excluding extension)</param>
        /// <param name="description">Description of database.</param>
        /// <param name="password">Password of database.</param>
        public static void CreateMochaDB(string path,string description,string password) {
            string content = EmptyContent;

            if(!string.IsNullOrEmpty(password)) {
                int dex = content.IndexOf("</Password>");
                content = content.Insert(dex,password);
            }
            if(!string.IsNullOrEmpty(description)) {
                int dex = content.IndexOf("</Description>");
                content = content.Insert(dex,description);
            }

            File.WriteAllText(path + ".mochadb",Mocha_ACE.Encrypt(content));
        }

        /// <summary>
        /// Checks the suitability and robustness of the MochaDB database.
        /// </summary>
        /// <param name="path">The file path of the MochaDB database to be checked.</param>
        public static bool CheckMochaDB(string path) {
            if(!IsMochaDB(path))
                throw new Exception("The file shown is not a MochaDB database file!");

            try {
                XDocument Document = XDocument.Parse(Mocha_ACE.Decrypt(File.ReadAllText(path)));
                if(Document.Root.Name.LocalName != "Mocha")
                    return false;
                else if(!ExistsElement(path,"Root/Password"))
                    return false;
                else if(!ExistsElement(path,"Root/Description"))
                    return false;
                else if(!ExistsElement(path,"Sectors"))
                    return false;
                else
                    return true;
            } catch { return false; }
        }

        /// <summary>
        /// Checks for the presence of the element. Example path: MyTable/MyColumn
        /// </summary>
        /// <param name="path">The MochaDB database file path to check.</param>
        /// <param name="elementPath">Path of element.</param>
        public static bool ExistsElement(string path,string elementPath) {
            if(!IsMochaDB(path))
                throw new Exception("The file shown is not a MochaDB database file!");

            string[] elementsName = elementPath.Split('/');

            try {
                XDocument document = XDocument.Parse(Mocha_ACE.Decrypt(File.ReadAllText(path)));
                XElement element = document.Root.Element(elementsName[0]);

                if(element.Name.LocalName != elementsName[0])
                    return false;

                for(int i = 1; i < elementsName.Length; i++) {
                    element = element.Element(elementsName[i]);
                    if(element.Name.LocalName != elementsName[i])
                        return false;
                }
                return true;
            } catch(NullReferenceException) { return false; }
        }

        #endregion

        #region Events

        /// <summary>
        /// This happens after content changed.
        /// </summary>
        public event EventHandler<EventArgs> ChangeContent;
        private void OnChangeContent(object sender,EventArgs e) {
            //Invoke.
            ChangeContent?.Invoke(sender,e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks the suitability and robustness of the MochaDB database.
        /// </summary>
        public bool CheckMochaDB() {
            try {
                if(Doc.Root.Name.LocalName != "Mocha")
                    return false;
                else if(!ExistsElement("Root/Password"))
                    return false;
                else if(!ExistsElement("Root/Description"))
                    return false;
                else if(!ExistsElement("Sectors"))
                    return false;
                else
                    return true;
            } catch { return false; }
        }

        /// <summary>
        /// Checks for the presence of the element. Example path: MyTable/MyColumn
        /// </summary>
        /// <param name="elementPath">Path of element.</param>
        public bool ExistsElement(string elementPath) {
            string[] elementsName = elementPath.Split('/');

            try {
                XElement element = Doc.Root.Element(elementsName[0]);

                if(element.Name.LocalName != elementsName[0])
                    return false;

                for(int i = 1; i < elementsName.Length; i++) {
                    element = element.Element(elementsName[i]);
                    if(element.Name.LocalName != elementsName[i])
                        return false;
                }
                return true;
            } catch(NullReferenceException) { return false; }
        }

        /// <summary>
        /// Returns the password of the MochaDB database.
        /// </summary>
        public string GetPassword() =>
            Doc.Root.Element("Root").Element("Password").Value;

        /// <summary>
        /// Save MochaDB database.
        /// </summary>
        public void Save() {
            sourceStream.Dispose();
            File.WriteAllText(DBPath,Mocha_ACE.Encrypt(Doc.ToString()));
            sourceStream = File.Open(DBPath,FileMode.Open,FileAccess.ReadWrite);

            OnChangeContent(this,new EventArgs());
        }

        /// <summary>
        /// MochaDB checks the existence of the database file and if not creates a new file.
        /// </summary>
        public void ExistsAutoCreate() {
            if(!File.Exists(DBPath))
                Reset();
        }

        /// <summary>
        /// MochaDB checks the existence of the database file and if not creates a new file. ALL DATA IS LOST!
        /// </summary>
        public void Reset() {
            File.WriteAllText(DBPath,Mocha_ACE.Encrypt(EmptyContent));
        }

        /// <summary>
        /// Sets the MochaDB Database password.
        /// </summary>
        /// <param name="password">Password to set.</param>
        public void SetPassword(string password) {
            Doc.Root.Element("Root").Element("Password").Value = password;
            Save();
        }

        /// <summary>
        /// Returns the description of the database.
        /// </summary>
        public string GetDescription() =>
            Doc.Root.Element("Root").Element("Description").Value;

        /// <summary>
        /// Sets the description of the database.
        /// </summary>
        /// <param name="Description">Description to set.</param>
        public void SetDescription(string Description) {
            Doc.Root.Element("Root").Element("Description").Value = Description;
            Save();
        }

        /// <summary>
        /// Returns whether the syntax is prohibited.
        /// </summary>
        /// <param name="syntax">Syntax to check.</param>
        public bool IsBannedSyntax(string syntax) =>
            syntax switch
            {
                "Root" => true,
                "Sectors" => true,
                _ => false
            };

        /// <summary>
        /// Return xml schema of database.
        /// </summary>
        public string GetXML() {
            XDocument doc = XDocument.Parse(Doc.ToString());
            doc.Root.Element("Root").Remove();
            return doc.ToString();
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose() {
            sourceStream.Dispose();
        }

        #endregion

        #region Sector

        /// <summary>
        /// Add sector.
        /// </summary>
        /// <param name="sector">MochaSector object to add.</param>
        public void AddSector(MochaSector sector) {
            if(ExistsElement("Sectors/" + sector.Name))
                throw new Exception("There is already a sector with this name!");

            XElement xSector = new XElement(sector.Name,sector.Data);
            xSector.Add(new XAttribute("Description",sector.Description));

            Doc.Root.Element("Sectors").Add(xSector);
            Save();
        }

        /// <summary>
        /// Add sector.
        /// </summary>
        /// <param name="name">Name of sector to add.</param>
        public void AddSector(string name) {
            AddSector(new MochaSector(name));
        }

        /// <summary>
        /// Add sector.
        /// </summary>
        /// <param name="name">Name of sector.</param>
        /// <param name="data">Data of sector.</param>
        public void AddSector(string name,string data) {
            AddSector(new MochaSector(name,data));
        }

        /// <summary>
        /// Remove sector by name.
        /// </summary>
        /// <param name="name">Name of sector to remove.</param>
        public void RemoveSector(string name) {
            if(!ExistsSector(name))
                return;

            Doc.Root.Element("Sectors").Element(name).Remove();
            Save();
        }

        /// <summary>
        /// Rename sector.
        /// </summary>
        /// <param name="name">Name of sector to rename.</param>
        /// <param name="newName">New name of sector.</param>
        public void RenameSector(string name,string newName) {
            if(!ExistsSector(name))
                throw new Exception("Sector not found in this name!");
            if(ExistsSector(newName))
                throw new Exception("There is already a sector with this name!");

            Doc.Root.Element("Sectors").Element(name).Name=newName;
            Save();
        }

        /// <summary>
        /// Return data of sector.
        /// </summary>
        /// <param name="name">Name of sector.</param>
        public string GetSectorData(string name) {
            if(!ExistsSector(name))
                throw new Exception("Sector not found in this name!");

            return Doc.Root.Element("Sectors").Element(name).Value;
        }

        /// <summary>
        /// Set data of sector by name.
        /// </summary>
        /// <param name="name">Name of sector.</param>
        /// <param name="data">Data to set.</param>
        public void SetSectorData(string name,string data) {
            if(!ExistsSector(name))
                throw new Exception("Sector not found in this name!");

            Doc.Root.Element("Sectors").Element(name).Value = data;
            Save();
        }

        /// <summary>
        /// Get description of sector by name.
        /// </summary>
        /// <param name="name">Name of sector.</param>
        public string GetSectorDescription(string name) {
            if(!ExistsSector(name))
                throw new Exception("Sector not found in this name!");

            return Doc.Root.Element("Sectors").Element(name).Attribute("Description").Value;
        }

        /// <summary>
        /// Set description of sector by name.
        /// </summary>
        /// <param name="name">Name of sector.</param>
        /// <param name="description">Description to set.</param>
        public void SetSectorDescription(string name,string description) {
            if(!ExistsSector(name))
                throw new Exception("Sector not found in this name!");

            Doc.Root.Element("Sectors").Element(name).Attribute("Description").Value = description;
            Save();
        }

        /// <summary>
        /// Return sectors in database.
        /// </summary>
        public IList<MochaSector> GetSectors() {
            List<MochaSector> sectors = new List<MochaSector>();

            MochaSector sector;
            foreach(XElement xSector in Doc.Root.Element("Sectors").Elements()) {
                sector =
                    new MochaSector(xSector.Name.LocalName,xSector.Value,xSector.Attribute("Description").Value);
                sectors.Add(sector);
            }

            return sectors;
        }

        /// <summary>
        /// Returns whether there is a sector with the specified name.
        /// </summary>
        /// <param name="name">Name of sector to check.</param>
        public bool ExistsSector(string name) =>
            ExistsElement("Sectors/" + name);

        #endregion

        #region Table

        /// <summary>
        /// Add table.
        /// </summary>
        /// <param name="table">MochaTable object to add.</param>
        public void AddTable(MochaTable table) {
            if(ExistsTable(table.Name))
                throw new Exception("There is already a table with this name!");

            XElement Xtable = new XElement(table.Name);

            for(int columnIndex = 0; columnIndex < table.ColumnCount; columnIndex++) {
                XElement column = new XElement(table.Columns[columnIndex].Name);
                column.Add(new XAttribute("DataType",table.Columns[columnIndex].DataType));
                column.Add(new XAttribute("Description",table.Columns[columnIndex].Description));
                for(int DataIndex = 0; DataIndex < table.Columns[columnIndex].Datas.Count; DataIndex++)
                    column.Add(new XElement("Data",table.Columns[columnIndex].Datas[DataIndex].Data));
                Xtable.Add(column);
            }

            Doc.Root.Add(Xtable);
            Save();
        }

        /// <summary>
        /// Create new table.
        /// </summary>
        /// <param name="name">Name of table.</param>
        public void CreateTable(string name) {
            MochaTable table = new MochaTable(name);
            AddTable(table);
        }

        /// <summary>
        /// Remove table by name.
        /// </summary>
        /// <param name="name">Name of table.</param>
        public void RemoveTable(string name) {
            if(!ExistsElement(name))
                return;

            Doc.Root.Element(name).Remove();
            Save();
        }

        /// <summary>
        /// Rename table.
        /// </summary>
        /// <param name="name">Name of table to rename.</param>
        /// <param name="newName">New name of table.</param>
        public void RenameTable(string name,string newName) {
            if(!ExistsTable(name))
                throw new Exception("Table not found in this name!");
            if(ExistsTable(newName))
                throw new Exception("There is already a table with this name!");

            Doc.Root.Element(name).Name=newName;
            Save();
        }

        /// <summary>
        /// Get table by name.
        /// </summary>
        /// <param name="name">Name of table.</param>
        public MochaTable GetTable(string name) {
            if(IsBannedSyntax(name))
                return null;

            if(!ExistsTable(name))
                throw new Exception("Table not found in this name!");

            MochaTable Table = new MochaTable(name);

            foreach(XElement Column in Doc.Root.Element(name).Elements()) {
                MochaColumn column = new MochaColumn(Column.Name.LocalName,
                    Enum.Parse<MochaDataType>(Column.Attribute("DataType").Value));
                column.Description = Column.Attribute("Description").Value;
                foreach(XElement Data in Column.Elements())
                    column.AddData(new MochaData(column.DataType,Data.Value));
                Table.AddColumn(column);
            }

            foreach(MochaRow Row in GetRows(name))
                Table.AddRow(Row);

            return Table;
        }

        /// <summary>
        /// Get tables in database.
        /// </summary>
        public IList<MochaTable> GetTables() {
            List<MochaTable> tables = new List<MochaTable>();

            IEnumerable<XElement> tableRange = Doc.Root.Elements();
            foreach(XElement table in tableRange) {
                if(IsBannedSyntax(table.Name.LocalName))
                    continue;

                tables.Add(GetTable(table.Name.LocalName));
            }
            return tables;
        }

        /// <summary>
        /// Returns whether there is a table with the specified name.
        /// </summary>
        /// <param name="name">Name of table.</param>
        public bool ExistsTable(string name) =>
            ExistsElement(name);

        #endregion

        #region Column

        /// <summary>
        /// Add colum in table.
        /// </summary>
        /// <param name="tableName">Name of column.</param>
        /// <param name="column">MochaColumn object to add.</param>
        public void AddColumn(string tableName,MochaColumn column) {
            if(!ExistsTable(tableName))
                throw new Exception("Table not found in this name!");
            if(ExistsElement(tableName + '/' + column.Name))
                throw new Exception("There is no such table or there is already a table with this name!");

            XElement Xcolumn = new XElement(column.Name);
            Xcolumn.Add(new XAttribute("DataType",column.DataType));
            Xcolumn.Add(new XAttribute("Description",column.Description));

            int RowCount = (int)Query.GetRun("ROWCOUNT:" + tableName);

            if(column.Datas.Count == 0 || column.Datas.Count < RowCount) {
                for(int Index = column.Datas.Count; Index <= RowCount; Index++)
                    column.AddData(new MochaData(column.DataType,MochaData.TryGetData(column.DataType,"")));
            }

            column.AdapteDatasValue();

            Doc.Root.Element(tableName).Add(Xcolumn);
            Save();
        }

        /// <summary>
        /// Create column in table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public void CreateColumn(string tableName,string name) {
            AddColumn(tableName,new MochaColumn(name,MochaDataType.String));
        }

        /// <summary>
        /// Remove column from table by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public void RemoveColumn(string tableName,string name) {
            if(!ExistsElement(tableName + '/' + name))
                return;

            Doc.Root.Element(tableName).Element(name).Remove();
            Save();
        }

        /// <summary>
        /// Rename column.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column to rename.</param>
        /// <param name="newName">New name of column.</param>
        public void RenameColumn(string tableName,string name,string newName) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");
            if(ExistsColumn(tableName,newName))
                throw new Exception("There is already a column with this name!");

            Doc.Root.Element(tableName).Element(name).Name=newName;
            Save();
        }

        /// <summary>
        /// Get description of column by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public string GetColumnDescription(string tableName,string name) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            return Doc.Root.Element(tableName).Element(name).Attribute("Description").Value;
        }

        /// <summary>
        /// Set description of column by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        /// <param name="description">Description to set.</param>
        public void SetColumnDescription(string tableName,string name,string description) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            Doc.Root.Element(tableName).Element(name).Attribute("Description").Value = description;
            Save();
        }

        /// <summary>
        /// Returns whether there is a column with the specified name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public bool ExistsColumn(string tableName,string name) =>
            ExistsElement(tableName + '/' + name);

        /// <summary>
        /// Get column from table by name
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public MochaColumn GetColumn(string tableName,string name) {
            if(IsBannedSyntax(tableName))
                return null;

            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            MochaColumn column = new MochaColumn(name,GetColumnDataType(tableName,name));
            column.Description = GetColumnDescription(tableName,name);

            int dex = 0;
            IEnumerable<XElement> dataRange = Doc.Root.Element(tableName).Element(name).Elements();
            foreach(XElement data in dataRange) {
                column.AddData(GetData(tableName,name,dex));
                dex++;
            }
            return column;
        }

        /// <summary>
        /// Return columns in table by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        public IList<MochaColumn> GetColumns(string tableName) {
            List<MochaColumn> columns = new List<MochaColumn>();

            if(IsBannedSyntax(tableName))
                return new MochaColumn[0];

            IEnumerable<XElement> columnsRange = Doc.Root.Element(tableName).Elements();
            foreach(XElement column in columnsRange) {
                columns.Add(GetColumn(tableName,column.Name.LocalName));
            }

            return columns;
        }

        /// <summary>
        /// Return column datatype by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public MochaDataType GetColumnDataType(string tableName,string name) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            return Enum.Parse<MochaDataType>(Doc.Root.Element(tableName).Element(name).Attribute("DataType").Value);
        }

        /// <summary>
        /// Set column datatype by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        /// <param name="dataType">MochaDataType to set.</param>
        public void SetColumnDataType(string tableName,string name,MochaDataType dataType) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            XElement column = Doc.Root.Element(tableName).Element(name);
            column.Attribute("DataType").Value = dataType.ToString();

            IEnumerable<XElement> dataRange = column.Elements();
            if(dataType == MochaDataType.AutoInt) {
                int index = -1;
                foreach(XElement data in dataRange) {
                    index++;
                    data.Value = index.ToString();
                }
                return;
            }

            foreach(XElement data in dataRange) {
                if(!MochaData.IsType(dataType,data.Value))
                    data.Value = MochaData.TryGetData(dataType,data.Value).ToString();
            }

            Save();
        }

        /// <summary>
        /// Return column's last AutoInt value by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="name">Name of column.</param>
        public int GetColumnAutoIntState(string tableName,string name) {
            if(!ExistsElement(tableName + '/' + name))
                throw new Exception("There is no such table or column!");

            XElement lastData = (XElement)Doc.Root.Element(tableName).Element(name).LastNode;

            MochaDataType dataType = GetColumnDataType(tableName,name);

            if(dataType != MochaDataType.AutoInt)
                throw new Exception("This column's datatype is not AutoInt!");

            if(lastData != null)
                return int.Parse(lastData.Value);
            else
                return 0;
        }

        #endregion

        #region Row

        /// <summary>
        /// Add row in table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="row">MochaRow object to add.</param>
        public void AddRow(string tableName,MochaRow row) {
            int dex = -1;
            IEnumerable<XElement> columnRange = Doc.Root.Element(tableName).Elements();
            foreach(XElement xColumn in columnRange) {
                dex++;
                AddData(tableName,xColumn.Name.LocalName,row.Datas[dex]);
            }
            Save();
        }

        /// <summary>
        /// Remove row from table by index.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="index">Index of row to remove.</param>
        public void RemoveRow(string tableName,int index) {
            int dex = 0;

            IEnumerable<XElement> columnRange = Doc.Root.Element(tableName).Elements();
            foreach(XElement xColumn in columnRange) {
                IEnumerable<XElement> dataRange = xColumn.Elements();
                foreach(XElement xData in dataRange) {
                    if(dex == index) {
                        xData.Remove();
                        break;
                    }
                    dex++;
                }
                dex = 0;
            }

            Save();
        }

        /// <summary>
        /// Return row from table by index.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="index">Index of row.</param>
        public MochaRow GetRow(string tableName,int index) {
            if(IsBannedSyntax(tableName))
                return new MochaRow();

            IList<MochaColumn> columns = GetColumns(tableName);

            if(columns.Count == 0)
                return new MochaRow();

            MochaRow row = new MochaRow();
            MochaData[] datas = new MochaData[columns.Count];

            for(int columnIndex = 0; columnIndex < columns.Count; columnIndex++) {
                datas[columnIndex] = columns[columnIndex].Datas[index];
            }

            //

            for(int dataIndex = 0; dataIndex < datas.Length; dataIndex++) {
                if(datas[dataIndex] != null)
                    continue;

                datas[dataIndex] = new MochaData(columns[dataIndex].DataType,
                    MochaData.TryGetData(columns[dataIndex].DataType,""));
            }

            row.Datas = datas;

            return row;
        }

        /// <summary>
        /// Return rows from table.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        public IList<MochaRow> GetRows(string tableName) {
            if(IsBannedSyntax(tableName))
                return new MochaRow[0];

            try {
                XElement firstColumn = (XElement)Doc.Root.Element(tableName).FirstNode;

                List<MochaRow> rows = new List<MochaRow>();

                int dataCount = GetDataCount(tableName,firstColumn.Name.LocalName);
                for(int index = 0; index < dataCount; index++) {
                    rows.Add(GetRow(tableName,index));
                }

                return rows;
            } catch {
                return new MochaRow[0];
            }
        }

        #endregion

        #region Data

        /// <summary>
        /// Add data.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">MochaData object to add.</param>
        public void AddData(string tableName,string columnName,MochaData data) {
            if(!ExistsElement(tableName + '/' + columnName))
                throw new Exception("There is no such table or column!");

            XElement Xdata = new XElement("Data",data.Data);

            XElement column = Doc.Root.Element(tableName).Element(columnName);

            MochaDataType dataType = Enum.Parse<MochaDataType>(column.Attribute("DataType").Value);
            if(dataType == MochaDataType.AutoInt) {
                Xdata.Value = (1 + GetColumnAutoIntState(tableName,columnName)).ToString();
            } else if(dataType == MochaDataType.Unique && !string.IsNullOrEmpty(data.Data.ToString())) {
                if(ExistsData(tableName,columnName,data))
                    throw new Exception("Any value can be added to a unique column only once!");
            }

            if(!MochaData.IsType(dataType,data.Data))
                throw new Exception("The submitted data is not compatible with the targeted data!");

            Doc.Root.Element(tableName).Element(columnName).Add(Xdata);
            Save();
        }

        /// <summary>
        /// Add data.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">Data to add.</param>
        public void AddData(string tableName,string columnName,object data) {
            if(!ExistsElement(tableName + '/' + columnName))
                throw new Exception("There is no such table or column!");

            AddData(tableName,columnName,new MochaData(GetColumnDataType(tableName,columnName),data));
        }

        /// <summary>
        /// Update data.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">Data to replace.</param>
        /// <param name="index">Index of data.</param>
        public void UpdateData(string tableName,string columnName,int index,object data) {
            if(!ExistsElement(tableName + '/' + columnName))
                throw new Exception("There is no such table or column!");

            int dex = 0;
            if(data == null)
                data = "";

            XElement xColumn = Doc.Root.Element(tableName).Element(columnName);

            MochaDataType dataType = GetColumnDataType(tableName,columnName);
            if(dataType == MochaDataType.AutoInt) {
                return;
            } else if(dataType == MochaDataType.Unique && !string.IsNullOrEmpty(data.ToString())) {
                if(ExistsData(tableName,columnName,data.ToString()))
                    throw new Exception("Any value can be added to a unique column only once!");
            }

            IEnumerable<XElement> dataRange = xColumn.Elements();
            foreach(XElement xData in dataRange) {
                if(dex == index) {
                    if(MochaData.IsType(dataType,data)) {
                        xData.SetValue(data);
                        break;
                    } else
                        throw new Exception("The submitted data is not compatible with the targeted data!");
                }
                dex++;
            }
            Save();
        }

        /// <summary>
        /// Returns whether there is a data with the specified.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">MochaData object to check.</param>
        public bool ExistsData(string tableName,string columnName,MochaData data) {
            if(ExistsElement(tableName + '/' + columnName)) {
                string stringData = data.Data.ToString();
                IEnumerable<XElement> dataRange = Doc.Root.Element(tableName).Element(columnName).Elements();
                foreach(XElement xData in dataRange)
                    if(xData.Value == stringData)
                        return true;
                return false;
            } else
                return false;
        }

        /// <summary>
        /// Returns whether there is a data with the specified.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">Data to check.</param>
        public bool ExistsData(string tableName,string columnName,object data) {
            return ExistsData(tableName,columnName,data);
        }

        /// <summary>
        /// Return data by index.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="index">Index of data.</param>
        public MochaData GetData(string tableName,string columnName,int index) {
            if(!ExistsElement(tableName + '/' + columnName))
                throw new Exception("There is no such table or column!");

            int dex = 0;
            MochaDataType dataType = GetColumnDataType(tableName,columnName);

            IEnumerable<XElement> dataRange = Doc.Root.Element(tableName).Element(columnName).Elements();
            foreach(XElement xData in dataRange) {
                if(dex == index) {
                    return new MochaData(dataType,MochaData.GetDataFromString(dataType,xData.Value));
                }

                dex++;
            }

            throw new Exception("This index is larger than the maximum number of data in the table!");
        }

        /// <summary>
        /// Return data index. If there are two of the same data, it returns the index of the one you found first!
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        /// <param name="data">Data to find index.</param>
        public int GetDataIndex(string tableName,string columnName,object data) {
            int dex = 0;
            string stringData = data.ToString();

            IEnumerable<XElement> dataRange = Doc.Root.Element(tableName).Element(columnName).Elements();
            foreach(XElement currentData in dataRange) {
                if(currentData.Value == stringData)
                    return dex;

                dex++;
            }

            return -1;
        }

        /// <summary>
        /// Return datas in column from table by name.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        public IList<MochaData> GetDatas(string tableName,string columnName) {
            if(IsBannedSyntax(tableName))
                return new MochaData[0];

            List<MochaData> datas = new List<MochaData>();

            IEnumerable<XElement> dataRange = Doc.Root.Element(tableName).Element(columnName).Elements();
            int dex = 0;
            foreach(XElement xData in dataRange) {
                datas.Add(GetData(tableName,columnName,dex));
                dex++;
            }
            return datas;
        }

        /// <summary>
        /// Get data count of table's column.
        /// </summary>
        /// <param name="tableName">Name of table.</param>
        /// <param name="columnName">Name of column.</param>
        public int GetDataCount(string tableName,string columnName) {
            if(IsBannedSyntax(tableName))
                return -1;
            int count = 0;

            XElement column = Doc.Root.Element(tableName).Element(columnName);

            IEnumerable<XElement> dataRange = column.Elements();
            foreach(XElement xData in dataRange)
                count++;

            return count;
        }

        #endregion

        #region Properties

        /// <summary>
        /// MochaQuery object.
        /// </summary>
        public MochaQuery Query { get; private set; }

        /// <summary>
        /// Directory path of database.
        /// </summary>
        public string DBPath { get; private set; }

        /// <summary>
        /// Name of database.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Accessibility to the database.
        /// </summary>
        public bool IsConnected =>
            File.Exists(DBPath);

        /// <summary>
        /// XML Document.
        /// </summary>
        internal XDocument Doc { get; set; }

        /// <summary>
        /// The most basic content of the database.
        /// </summary>
        internal static string EmptyContent => "<?MochaDB?>\n" +
                        "<Mocha>\n" +
                        "  <Root>\n" +
                        "    <Password DataType=\"String\" Description=\"Password of database.\"></Password>\n" +
                        "    <Description DataType=\"String\" Description=\"Description of database.\"></Description>" +
                        "  </Root>\n" +
                        "  <Sectors>\n" +
                        "  </Sectors>\n" +
                        "</Mocha>";

        /// <summary>
        /// Version of MochaDB.
        /// </summary>
        internal static string Version =>
            "1.0.0";

        #endregion
    }
}
