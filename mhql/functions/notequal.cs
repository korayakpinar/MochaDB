﻿namespace MochaDB.mhql.functions {
    /// <summary>
    /// MHQL NOTEQUAL function.
    /// </summary>
    internal class MhqlFunc_NOTEQUAL {
        /// <summary>
        /// Pass command?
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="row">Row.</param>
        public static bool Pass(string command,MochaRow row) {
            var parts = command.Split(',');
            if(parts.Length < 2 || parts.Length > 2)
                throw new MochaException("NOTEQUAL function is cannot processed!");

            int dex;
            decimal range;

            if(!int.TryParse(parts[0].TrimStart().TrimEnd(),out dex))
                throw new MochaException("NOTEQUAL function is cannot processed!");
            if(!decimal.TryParse(parts[1].TrimStart().TrimEnd(),out range))
                throw new MochaException("NOTEQUAL function is cannot processed!");

            decimal value;
            if(!decimal.TryParse(row.Datas[dex].Data.ToString(),out value))
                throw new MochaException("NOTEQUAL function is cannot processed!");

            return value != range;
        }
    }
}
