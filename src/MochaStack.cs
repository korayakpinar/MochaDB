﻿using System;
using MochaDB.engine;

namespace MochaDB {
    /// <summary>
    /// This is stack object for MochaDB.
    /// </summary>
    public class MochaStack:IMochaStack {
        #region Fields

        private string name;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new MochaStack.
        /// </summary>
        /// <param name="name">Name of stack.</param>
        public MochaStack(string name) {
            Items= new MochaStackItemCollection();
            Attributes = new MochaAttributeCollection();
            Name=name;
            Description=string.Empty;
        }

        /// <summary>
        /// Create new MochaStack.
        /// </summary>
        /// <param name="name">Name of stack.</param>
        /// <param name="description">Description of stack.</param>
        public MochaStack(string name,string description) :
            this(name) {
            Description=description;
        }

        #endregion

        #region Operators

        public static explicit operator string(MochaStack value) =>
            value.ToString();

        #endregion

        #region Events

        /// <summary>
        /// This happens after name changed;
        /// </summary>
        public event EventHandler<EventArgs> NameChanged;
        private void OnNameChanged(object sender,EventArgs e) {
            //Invoke.
            NameChanged?.Invoke(sender,e);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns <see cref="Name"/>.
        /// </summary>
        public override string ToString() {
            return Name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name.
        /// </summary>
        public string Name {
            get =>
                name;
            set {
                value=value.Trim();
                if(string.IsNullOrWhiteSpace(value))
                    throw new MochaException("Name is cannot null or whitespace!");

                Engine_NAMES.CheckThrow(value);

                if(value==name)
                    return;

                name=value;
                OnNameChanged(this,new EventArgs());
            }
        }

        /// <summary>
        /// Description of stack.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Items of stack.
        /// </summary>
        public MochaStackItemCollection Items { get; }

        /// <summary>
        /// Attributes of stack.
        /// </summary>
        public MochaAttributeCollection Attributes { get; }

        #endregion
    }
}
