using System;
using System.Linq;
using System.Text.RegularExpressions;
using MochaDB.mhql.engine;
using MochaDB.Mhql;

namespace MochaDB.mhql {
    /// <summary>
    /// MHQL ORDERBY must.
    /// </summary>
    internal class Mhql_MUST:MhqlKeyword {
        #region Constructors

        /// <summary>
        /// Create a new Mhql_MUST.
        /// </summary>
        /// <param name="db">Target database.</param>
        public Mhql_MUST(MochaDatabase db) {
            Command = string.Empty;
            Tdb = db;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if command is MUST command, returns if not.
        /// </summary>
        /// <param name="command">Command to check.</param>
        public bool IsMUST(string command) =>
            command.StartsWith("MUST",StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns must command.
        /// </summary>
        /// <param name="command">MHQL Command.</param>
        /// <param name="final">Command of removed must commands.</param>
        public string GetMUST(string command,out string final) {
            var pattern = new Regex("END",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var value = string.Empty;
            var count = 0;
            command = command.Substring(4);
            for(int index = 0; index < command.Length; index++) {
                var currentChar = command[index];
                if(currentChar == 'E' || currentChar == 'e') {
                    if(command.Length - 1 - index >= 3) {
                        if(count == 0 && pattern.IsMatch(command.Substring(index,3))) {
                            final = command.Substring(index+3).Trim();
                            return value.Trim();
                        }
                    }
                } else if(currentChar == '(')
                    count++;
                else if(currentChar == ')')
                    count--;

                value += currentChar;
            }
            final = command;
            return value.Trim();
        }

        /// <summary>
        /// Must by tables.
        /// </summary>
        /// <param name="command">Must command.</param>
        /// <param name="table">Table.</param>
        /// <param name="from">Use state FROM keyword.</param>
        public void MustTable(string command,ref MochaTableResult table,bool from) {
            command = command.Trim();
            var parts = Mhql_AND.GetParts(command);
            for(int index = 0; index < parts.Length; index++) {
                var partcmd = parts[index];
                MhqlEng_MUST.ProcessPart(ref partcmd,table,from);
                table.Rows = (
                        from value in table.Rows
                        where MhqlEng_MUST.IsPassTable(ref partcmd,value)
                        select value
                        ).ToArray();
            }
            table.SetDatasByRows();
        }

        #endregion
    }
}
