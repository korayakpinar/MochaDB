using System;
using System.Linq;
using MochaDB.Mhql;

namespace MochaDB.mhql {
    /// <summary>
    /// MHQL GROUPBY keyword.
    /// </summary>
    internal class Mhql_GROUPBY:MhqlKeyword {
        #region Constructors

        /// <summary>
        /// Create a new Mhql_GROUPBY.
        /// </summary>
        /// <param name="db">Target database.</param>
        public Mhql_GROUPBY(MochaDatabase db) {
            Tdb=db;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if command is GROUPBY command, returns if not.
        /// </summary>
        /// <param name="command">Command to check.</param>
        public bool IsGROUPBY(string command) =>
            command.StartsWith("GROUPBY",StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns groupby command.
        /// </summary>
        /// <param name="command">MHQL Command.</param>
        /// <param name="final">Command of removed groupby commands.</param>
        public string GetGROUPBY(string command,out string final) {
            int groupbydex = command.IndexOf("GROUPBY",StringComparison.OrdinalIgnoreCase);
            if(groupbydex==-1)
                throw new MochaException("GROUPBY command is cannot processed!");
            var match = MochaDbCommand.mainkeywordRegex.Match(command,groupbydex+7);
            int finaldex = match.Index;
            if(finaldex==0)
                throw new MochaException("GROUPBY command is cannot processed!");
            var groupbycommand = command.Substring(groupbydex+7,finaldex-(groupbydex+7));

            final = command.Substring(finaldex);
            return groupbycommand;
        }

        /// <summary>
        /// Groupby by command.
        /// </summary>
        /// <param name="command">Orderby command.</param>
        /// <param name="table">Table to ordering.</param>
        /// <param name="final">Command of removed use commands.</param>
        public void GroupBy(string command,ref MochaTableResult table) {
            command = command.TrimStart().TrimEnd();
            int dex =
                command.StartsWith("ASC",StringComparison.OrdinalIgnoreCase) ?
                3 :
                command.StartsWith("DESC",StringComparison.OrdinalIgnoreCase) ?
                4 : 0;

            int columndex;
            if(!int.TryParse(command.Substring(dex),out columndex))
                throw new MochaException("Item index is cannot processed!");

            var result =
                from value in table.Columns[columndex].Datas
                group value by value.Data into grouped
                select new { Data = grouped.Key,Count = grouped.Count() };


            table.Columns = new[] { new MochaColumn("Datas"),new MochaColumn("Count") };
            table.Rows = new MochaRow[result.Count()];
            for(int index = 0; index < table.Rows.Length; index++) {
                var item = result.ElementAt(index);
                table.Rows[index] = new MochaRow(
                    new MochaData {
                        dataType = MochaDataType.String,
                        data = item.Data
                    },
                    new MochaData {
                        dataType = MochaDataType.Int32,
                        data = item.Count
                    });
            }
            table.SetDatasByRows();
        }

        #endregion
    }
}
