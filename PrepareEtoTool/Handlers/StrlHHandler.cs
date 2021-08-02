using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class StrlHHandler
    {
        // fields
        private string _strlHPath;

        private string _log;

        // properties
        public string StrlHPath
        {
            get { return _strlHPath; }
            private set { _strlHPath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strlHPath"></param>
        public StrlHHandler(string strlHPath)
        {
            this.StrlHPath = strlHPath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.StrlHPath}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        public void UpdateStrlH(string appName, string appBranch)
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

            var strlHLines = File.ReadAllLines(this.StrlHPath);

            // delete old file
            File.Delete(this.StrlHPath);
            LogWrite($"Deleted old {appName}{EtoStrings.STRDB_C} file");
            // create new file
            File.WriteAllLines(Path.Combine(this.StrlHPath, "..", $"{appName}_{appBranch}{EtoStrings.STRL_H}"), strlHLines);
            LogWrite($"Created new {appName}_{appBranch}{EtoStrings.STRL_H} file");

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
