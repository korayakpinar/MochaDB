﻿using System;
using MochaDB.engine;

namespace MochaDB.FileSystem {
    /// <summary>
    /// Directory for MochaDB file system.
    /// </summary>
    public class MochaDirectory:IMochaDirectory {
        #region Fields

        private string name;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new MochaDirectory.
        /// </summary>
        /// <param name="name">Name of directory.</param>
        public MochaDirectory(string name) {
            Directories=new MochaDirectoryCollection();
            Files=new MochaFileCollection();
            Name=name;
            Description=string.Empty;
        }

        #endregion

        #region Operators

        public static explicit operator string(MochaDirectory value) =>
            value.ToString();

        #endregion

        #region Events

        /// <summary>
        /// This happens after name changed.
        /// </summary>
        public event EventHandler<EventArgs> NameChanged;
        private void OnNameChanged(object sender,EventArgs e) {
            //Invoke.
            NameChanged?.Invoke(this,new EventArgs());
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
        /// Directory name.
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
        /// Directory description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Files of directory.
        /// </summary>
        public MochaFileCollection Files { get; }

        /// <summary>
        /// Directories of directory.
        /// </summary>
        public MochaDirectoryCollection Directories { get; }

        #endregion
    }
}
