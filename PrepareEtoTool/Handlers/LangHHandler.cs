using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class LangHHandler
    {
        // fields
        private string _langHPath;

        private string _log;

        // properties
        public string LangHPath
        {
            get { return _langHPath; }
            private set { _langHPath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="langHPath"></param>
        public LangHHandler(string langHPath)
        {
            this.LangHPath = langHPath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.LangHPath}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        public void UpdateLangH(string appName, string appBranch)
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

            var langHLines = File.ReadAllLines(this.LangHPath);

            for (int i = 0; i < langHLines.Length; i++)
            {
                if (langHLines[i].Contains("#include"))
                {
                    langHLines[i] = $"#include \"{appName}_{appBranch}{EtoStrings.STRL_H}\"";
                    break;
                }
            }

            // delete old file
            File.Delete(this.LangHPath);
            LogWrite($"Deleted old {EtoStrings.LANG_H} file");
            // create new file
            File.WriteAllLines(this.LangHPath, langHLines);
            LogWrite($"Created new {EtoStrings.LANG_H} file");

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
