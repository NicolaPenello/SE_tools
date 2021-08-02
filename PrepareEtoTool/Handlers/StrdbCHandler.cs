using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class StrdbCHandler
    {
        // fields
        private string _strdbCPath;
        private string _log;

        // properties
        public string StrdbCPath
        {
            get { return _strdbCPath; }
            private set { _strdbCPath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strdbCPath"></param>
        public StrdbCHandler(string strdbCPath)
        {
            this.StrdbCPath = strdbCPath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.StrdbCPath}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        public void UpdateStrdbC(string appName, string appBranch)
        {
            // validation of parameters is owned by the business logic at first and then by the preparation owner:
            // here we expect to have no troubles due to file-not-found or null-reference exceptions

            if (EtoStrings.ToEnum(appName) == EtoProjects.AMICO)
            {
                LogWrite("Don't know what to do with amico app");
                return;
            }

            // aquacentr project is related to bcwc app
            if (EtoStrings.ToEnum(appName) == EtoProjects.BCWC)
            {
                appName = EtoStrings.AQUACENTR;
            }

            var strdbCLines = File.ReadAllLines(this.StrdbCPath);

            // delete old file
            File.Delete(this.StrdbCPath);
            LogWrite($"Deleted old {appName}{EtoStrings.STRDB_C} file");
            // create new file
            File.WriteAllLines(Path.Combine(this.StrdbCPath, "..", $"{appName}_{appBranch}{EtoStrings.STRDB_C}"), strdbCLines);
            LogWrite($"Created new {appName}_{appBranch}{EtoStrings.STRDB_C} file");

        }

        // log methods
        public void ClearLog()
        {
            this._log = string.Empty;
            LogWrite("Log cleared");
        }
        private void LogWrite(string str)
        {
            this._log += $"{str}{Environment.NewLine}";
        }
    }
}
