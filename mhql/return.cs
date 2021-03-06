using System;

namespace MochaDB.mhql {
    /// <summary>
    /// MHQL RETURN keyword.
    /// </summary>
    internal class Mhql_RETURN:MhqlKeyword {
        #region Constructors

        /// <summary>
        /// Create a new Mhql_RETURN.
        /// </summary>
        /// <param name="db">Target database.</param>
        public Mhql_RETURN(MochaDatabase db) {
            Command = string.Empty;
            Tdb = db;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if command is returnable, return false if not.
        /// </summary>
        public bool IsReturnableCmd() =>
            Command.EndsWith("RETURN",StringComparison.OrdinalIgnoreCase);

        #endregion
    }
}
