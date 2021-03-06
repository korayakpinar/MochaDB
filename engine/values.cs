﻿using System.Text.RegularExpressions;

namespace MochaDB.engine {
    /// <summary>
    /// Name engine of MochaDB.
    /// </summary>
    internal static class Engine_VALUES {
        /// <summary>
        /// Returns true if pass but returns false if not.
        /// </summary>
        public static bool AttributeCheck(string value) {
            var pattern = new Regex(
".*(;|:).*");
            return !pattern.IsMatch(value);
        }

        /// <summary>
        /// Check name and give exception if not pass.
        /// </summary>
        /// <param name="value">Value.</param>
        public static void AttributeCheckThrow(string value) {
            if(!AttributeCheck(value))
                throw new MochaException("The value did not meet the value conventions!");
        }
    }
}
