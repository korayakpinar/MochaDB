﻿using System;
using System.Linq;

namespace MochaDB.FileSystem {
    /// <summary>
    /// MochaDB path definer.
    /// </summary>
    public class MochaPath:IMochaPath {
        #region Fields

        private string path;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new MochaPath.
        /// </summary>
        /// <param name="path">Path.</param>
        public MochaPath(string path) {
            Path=path;
        }

        #endregion

        #region Implicit & Explicit

        public static implicit operator MochaPath(string path) =>
            new MochaPath(path);

        #endregion

        #region Methods

        /// <summary>
        /// Go to the parent directory.
        /// </summary>
        public void ParentDirectory() {
            var dex = Path.LastIndexOf('/');
            Path=dex!=-1 ? Path.Substring(0,dex) : Path;
        }

        /// <summary>
        /// Return parent directory's MochaPath object.
        /// </summary>
        public MochaPath ParentPath() {
            var dex = Path.LastIndexOf('/');
            var path = dex!=-1 ? Path.Substring(0,dex) : Path;
            return path;
        }

        /// <summary>
        /// Returns name of current directory or file.
        /// </summary>
        /// <returns></returns>
        public string Name() {
            var dex = Path.LastIndexOf('/');
            return dex!=-1 ? Path.Remove(0,dex+1) : Path;
        }

        /// <summary>
        /// Return true if path is database path but returns false if not.
        /// </summary>
        public bool IsDatabasePath() =>
            Path.StartsWith("Tables") ||
            Path.StartsWith("Sectors") ||
            Path.StartsWith("Stacks") ||
            Path.StartsWith("FileSystem");

        #endregion

        #region Overrides

        /// <summary>
        /// Returns path.
        /// </summary>
        public override string ToString() {
            return Path;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Path.
        /// </summary>
        public string Path {
            get =>
                path;
            set {
                value=value.TrimStart().TrimEnd();
                if(string.IsNullOrEmpty(value))
                    throw new Exception("Path is cannot null!");

                value=value.Replace('\\','/');
                value = value.Last()=='/' ? value.Remove(value.Length-1,1) : value;
                if(value==path)
                    return;

                path=value;
            }
        }

        #endregion
    }
}
