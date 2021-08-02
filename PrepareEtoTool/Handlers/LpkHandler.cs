using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class LpkHandler
    {
        // fields
        private string _langPacksDir;

        private string _log;

        // properties
        public string LangPacksDir
        {
            get { return _langPacksDir; }
            private set { _langPacksDir = value; }
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
        public LpkHandler(string langPacksDir)
        {
            this.LangPacksDir = langPacksDir;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.LangPacksDir}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        public void DeleteLpks(string appName)
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

            var lpkFiles = Directory.GetFiles(this.LangPacksDir, $"*.{EtoStrings.LPK_SFX}");

            // delete old lpk files
            foreach (var lpkFile in lpkFiles)
            {
                File.Delete(lpkFile);
                LogWrite($"Deleted file {lpkFile}");
            }
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
