﻿using System;

namespace MochaDB {
    /// <summary>
    /// StackItem interface for MochaDB stack items.
    /// </summary>
    public interface IMochaStackItem:IMochaDatabaseItem {
        #region Events

        event EventHandler<EventArgs> NameChanged;

        #endregion

        #region Properties

        string Value { get; set; }
        MochaAttributeCollection Attributes { get; }
        MochaStackItemCollection Items { get; }

        #endregion
    }
}
