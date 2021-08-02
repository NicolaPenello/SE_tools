using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class ApplangMakHandler
    {
        // fields
        private string _applangMakPath;

        private string _log;

        // properties
        public string ApplangMakPath
        {
            get { return _applangMakPath; }
            private set { _applangMakPath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applangMakPath"></param>
        public ApplangMakHandler(string applangMakPath)
        {
            this.ApplangMakPath = applangMakPath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.ApplangMakPath}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        public void UpdateApplangMak(string appName, string appBranch)
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

            var applangMakLines = File.ReadAllLines(this.ApplangMakPath);

            for (int i = 0; i < applangMakLines.Length; i++)
            {
                if (applangMakLines[i].Contains("$(OBJDIR)") && !applangMakLines[i].Contains("OBJS"))
                {
                    applangMakLines[i] = $"                   $(OBJDIR)\\{appName}_{appBranch}strdb.obj";
                    break;
                }
            }

            // delete old file
            File.Delete(this.ApplangMakPath);
            LogWrite($"Deleted old {EtoStrings.APPLANG_MAK} file");
            // create new file
            File.WriteAllLines(this.ApplangMakPath, applangMakLines);
            LogWrite($"Created new {EtoStrings.APPLANG_MAK} file");

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
