﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MochaDB.Connection;
using MochaDB.mhql;
using MochaDB.Streams;

namespace MochaDB.Mhql {
    /// <summary>
    /// MHQL Command processor for MochaDB.
    /// </summary>
    public class MochaDbCommand:IMochaDbCommand {
        #region Fields

        private MhqlCommand command;
        private MochaDatabase db;

        internal static Regex keywordRegex = new Regex(
@"\b(USE|RETURN|ORDERBY|ASC|DESC|MUST|AND|END|GROUPBY|FROM|AS|BETWEEN|BIGGER|LOWER|EQUAL|STARTW|ENDW|
SELECT|REMOVE|NOTEQUAL)\b",
    RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);

        internal static Regex mainkeywordRegex = new Regex(
@"\b(USE|RETURN|ORDERBY|MUST|GROUPBY|SELECT|REMOVE)\b",
    RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);

        internal MochaArray<MhqlKeyword> keywords;

        internal Mhql_USE USE;
        internal Mhql_SELECT SELECT;
        internal Mhql_RETURN RETURN;
        internal Mhql_REMOVE REMOVE;
        internal Mhql_ORDERBY ORDERBY;
        internal Mhql_MUST MUST;
        internal Mhql_GROUPBY GROUPBY;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new MochaDbCommand.
        /// </summary>
        /// <param name="db">Target MochaDatabase.</param>
        public MochaDbCommand(MochaDatabase db) {
            //Load mhql core.
            USE = new Mhql_USE(Database);
            SELECT = new Mhql_SELECT(Database);
            RETURN = new Mhql_RETURN(Database);
            ORDERBY = new Mhql_ORDERBY(Database);
            GROUPBY = new Mhql_GROUPBY(Database);
            MUST = new Mhql_MUST(Database);
            REMOVE = new Mhql_REMOVE(Database);
            keywords = new MochaArray<MhqlKeyword>(USE,SELECT,REMOVE,RETURN,ORDERBY,GROUPBY,MUST);

            Database=db;
            Command=string.Empty;
        }

        /// <summary>
        /// Create a new MochaDbCommand.
        /// </summary>
        /// <param name="command">MQL Command.</param>
        /// <param name="db">Target MochaDatabase.</param>
        public MochaDbCommand(string command,MochaDatabase db) :
            this(db) {
            Command=command;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Check connection and database.
        /// </summary>
        internal void CheckConnection() {
            if(Database==null)
                throw new MochaException("Target database is cannot null!");
            if(Database.State!=MochaConnectionState.Connected)
                throw new MochaException("Connection is not open!");
        }

        #endregion

        #region ExecuteCommand

        /// <summary>
        /// Run command.
        /// </summary>
        /// <param name="command">MQL Command to set.</param>
        public void ExecuteCommand(string command) {
            Command=command;
            ExecuteCommand();
        }

        /// <summary>
        /// Run command.
        /// </summary>
        public void ExecuteCommand() {
            CheckConnection();
            if(RETURN.IsReturnableCmd())
                return;

            string lastcommand;
            var tags = Mhql_AT.GetATS(Command,out lastcommand);
            if(lastcommand.StartsWith("USE",StringComparison.OrdinalIgnoreCase)) {
                throw new MochaException("USE keyword is not supported by execute command!");
            } else if(lastcommand.StartsWith("SELECT",StringComparison.OrdinalIgnoreCase)) {
                var select = SELECT.GetSELECT(out lastcommand);
                List<object> collection = new List<object>();
                if(tags.Length == 0)
                    collection.AddRange(SELECT.GetTables(select));
                else {
                    bool
                        tables = false,
                        sectors = false,
                        stacks = false;
                    for(int index = 0; index < tags.Length; index++) {
                        if(tags.ElementAt(index).Equals("@TABLES",StringComparison.OrdinalIgnoreCase)) {
                            if(tables)
                                throw new MochaException("@TABLES cannot be targeted more than once!");

                            tables = true;
                            collection.AddRange(SELECT.GetTables(select));
                        } else if(tags.ElementAt(index).Equals("@SECTORS",StringComparison.OrdinalIgnoreCase)) {
                            if(sectors)
                                throw new MochaException("@SECTORS cannot be targeted more than once!");

                            sectors = true;
                            collection.AddRange(SELECT.GetSectors(select));
                        } else if(tags.ElementAt(index).Equals("@STACKS",StringComparison.OrdinalIgnoreCase)) {
                            if(stacks)
                                throw new MochaException("@STACKS cannot be targeted more than once!");

                            stacks = true;
                            collection.AddRange(SELECT.GetStacks(select));
                        } else
                            throw new MochaException("@ mark is cannot processed!");
                    }
                }

                do {
                    //Orderby.
                    if(ORDERBY.IsORDERBY(lastcommand)) {
                        throw new MochaException("ORDERBY keyword is canot used with SELECT keyword!");
                    }
                    //Groupby.
                    else if(GROUPBY.IsGROUPBY(lastcommand)) {
                        throw new MochaException("GROUPBY keyword is canot used with SELECT keyword!");
                    }
                    //Must.
                    else if(MUST.IsMUST(lastcommand)) {
                        throw new MochaException("MUST keyword is canot used with SELECT keyword!");
                    }
                    //Remove.
                    else if(lastcommand.Equals("REMOVE",StringComparison.OrdinalIgnoreCase)) {
                        for(int index = 0; index < collection.Count; index++) {
                            var item = (IMochaDatabaseItem)collection[index];
                            Database.RemoveDatabaseItem(item);
                        }
                        return;
                    } else
                        throw new MochaException($"'{lastcommand}' command is cannot processed!");
                } while(true);
            } else
                throw new MochaException("MHQL is cannot processed!");
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Returns first data as MochaTableResult.
        /// </summary>
        public MochaTableResult ExecuteScalarTable() {
            return ExecuteScalar() as MochaTableResult;
        }

        /// <summary>
        /// Returns first data as MochaTableResult.
        /// </summary>
        /// <param name="command">MHQL Command to set.</param>
        public MochaTableResult ExecuteScalarTable(string command) {
            return ExecuteScalar(command) as MochaTableResult;
        }

        /// <summary>
        /// Returns first result or null.
        /// </summary>
        /// <param name="command">MQL Command to set.</param>
        public object ExecuteScalar(string command) {
            Command=command;
            return ExecuteScalar();
        }

        /// <summary>
        /// Returns first result or null.
        /// </summary>
        public object ExecuteScalar() {
            var reader = ExecuteReader();
            if(reader.Read())
                return reader.Value;
            return null;
        }

        #endregion

        #region ExecuteReader

        /// <summary>
        /// Read returned results.
        /// </summary>
        /// <param name="command">MQL Command to set.</param>
        public MochaReader<object> ExecuteReader(string command) {
            Command=command;
            return ExecuteReader();
        }

        /// <summary>
        /// Read returned results.
        /// </summary>
        public MochaReader<object> ExecuteReader() {
            CheckConnection();
            var reader = new MochaReader<object>();
            if(!RETURN.IsReturnableCmd())
                return reader;
            bool
                fromkw,
                orderby = false,
                groupby = false;

            string lastcommand;
            var tags = Mhql_AT.GetATS(Command,out lastcommand);
            if(lastcommand.StartsWith("USE",StringComparison.OrdinalIgnoreCase)) {
                if(tags.Length > 1)
                    throw new MochaException("Multi tags is cannot used with USE keyword!");

                string tag =
                    tags.Length == 0 ?
                        string.Empty :
                        tags.GetFirst();

                if(tag.Equals("@STACKS",StringComparison.OrdinalIgnoreCase))
                    throw new MochaException("@STACKS is cannot target if used with USE keyword!");

                var use = USE.GetUSE(out lastcommand);
                fromkw = use.IndexOf("FROM",StringComparison.OrdinalIgnoreCase) != -1;
                var table =
                    string.IsNullOrEmpty(tag) || tag.Equals("@TABLES",StringComparison.OrdinalIgnoreCase) ?
                        USE.GetTable(use,fromkw) :
                        tag.Equals("@SECTORS",StringComparison.OrdinalIgnoreCase) ?
                            USE.GetSector(use,fromkw) :
                            throw new MochaException("@ mark is cannot processed!");
                do {
                    //Orderby.
                    if(ORDERBY.IsORDERBY(lastcommand)) {
                        orderby=true;
                        if(groupby)
                            throw new MochaException("GROUPBY keyword must be specified before ORDERBY!");
                        ORDERBY.OrderBy(ORDERBY.GetORDERBY(lastcommand,out lastcommand),ref table);
                    }
                    //Groupby.
                    else if(GROUPBY.IsGROUPBY(lastcommand)) {
                        groupby=true;
                        GROUPBY.GroupBy(GROUPBY.GetGROUPBY(lastcommand,out lastcommand),ref table);
                    }
                    //Must.
                    else if(MUST.IsMUST(lastcommand)) {
                        if(orderby)
                            throw new MochaException("MUST keyword must be specified before ORDERBY!");
                        else if(groupby)
                            throw new MochaException("MUST keyword must be specified before GROUPBY!");

                        MUST.MustTable(MUST.GetMUST(lastcommand,out lastcommand),ref table,fromkw);
                    }
                    //Return.
                    else if(lastcommand.Equals("RETURN",StringComparison.OrdinalIgnoreCase))
                        break;
                    else
                        throw new MochaException($"'{lastcommand}' command is cannot processed!");
                } while(true);

                reader.array = new MochaArray<object>(table);
            } else if(lastcommand.StartsWith("SELECT",StringComparison.OrdinalIgnoreCase)) {
                var select = SELECT.GetSELECT(out lastcommand);
                fromkw = select.IndexOf("FROM",StringComparison.OrdinalIgnoreCase) != -1;

                if(fromkw)
                    throw new MochaException("FROM keyword is cannot use with SELECT keyword!");

                List<object> collection = new List<object>();
                if(tags.Length == 0)
                    collection.AddRange(SELECT.GetTables(select));
                else {
                    bool
                        tables = false,
                        sectors = false,
                        stacks = false;
                    for(int index = 0; index < tags.Length; index++) {
                        if(tags.ElementAt(index).Equals("@TABLES",StringComparison.OrdinalIgnoreCase)) {
                            if(tables)
                                throw new MochaException("@TABLES cannot be targeted more than once!");

                            tables = true;
                            collection.AddRange(SELECT.GetTables(select));
                        } else if(tags.ElementAt(index).Equals("@SECTORS",StringComparison.OrdinalIgnoreCase)) {
                            if(sectors)
                                throw new MochaException("@SECTORS cannot be targeted more than once!");

                            sectors = true;
                            collection.AddRange(SELECT.GetSectors(select));
                        } else if(tags.ElementAt(index).Equals("@STACKS",StringComparison.OrdinalIgnoreCase)) {
                            if(stacks)
                                throw new MochaException("@STACKS cannot be targeted more than once!");

                            stacks = true;
                            collection.AddRange(SELECT.GetStacks(select));
                        } else
                            throw new MochaException(tags.ElementAt(index));
                    }
                }

                do {
                    //Orderby.
                    if(ORDERBY.IsORDERBY(lastcommand)) {
                        throw new MochaException("ORDERBY keyword is canot used with SELECT keyword!");
                    }
                    //Groupby.
                    else if(GROUPBY.IsGROUPBY(lastcommand)) {
                        throw new MochaException("GROUPBY keyword is canot used with SELECT keyword!");
                    }
                    //Must.
                    else if(MUST.IsMUST(lastcommand)) {
                        throw new MochaException("MUST keyword is canot used with SELECT keyword!");
                    }
                    //Return.
                    else if(lastcommand.Equals("RETURN",StringComparison.OrdinalIgnoreCase))
                        break;
                    else
                        throw new MochaException($"'{lastcommand}' command is cannot processed!");
                } while(true);

                reader.array = new MochaArray<object>(collection);
            } else
                throw new MochaException("MHQL is cannot processed!");

            return reader;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns command.
        /// </summary>
        public override string ToString() {
            return Command;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current MQL command.
        /// </summary>
        public MhqlCommand Command {
            get =>
                command;
            set {
                if(value==command)
                    return;

                command = value;
                for(int index = 0; index < keywords.Length; index++)
                    keywords[index].Command = value;
            }
        }

        /// <summary>
        /// Target database.
        /// </summary>
        public MochaDatabase Database {
            get =>
                db;
            set {
                if(value==null)
                    throw new MochaException("This MochaDatabase is not affiliated with a database!");
                if(value==db)
                    return;

                db = value;
                for(int index = 0; index < keywords.Length; index++)
                    keywords[index].Tdb = value;
            }
        }

        #endregion
    }
}
