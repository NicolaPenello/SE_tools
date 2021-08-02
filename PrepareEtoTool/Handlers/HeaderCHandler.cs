using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EtoUtils;

namespace EtoHandlers
{
    internal class HeaderCHandler
    {
        // fields
        private string _headerCPath;
        private string _log;

        // properties
        public string HeaderCPath
        {
            get { return _headerCPath; }
            private set { _headerCPath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerCPath"></param>
        public HeaderCHandler(string headerCPath)
        {
            this.HeaderCPath = headerCPath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.HeaderCPath}");
        }

        /// <summary>
        /// Example: appName = unflrtsa; appBranch = e12; appVersion = 601; 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        /// <param name="appVersion"></param>

        public void UpdateHeaderC(string appName, string appBranch, string appVersion)
        {
            // validation of parameters is owned by the business logic at first and then by the preparation owner:
            // here we expect to have no troubles due to file-not-found or null-reference exceptions

            // we will count the number of changes as a further feedback and in order to optimize the updating 
            // we expect to update 3 points of the code
            int changes = 0;

            var headerCLines = File.ReadAllLines(this.HeaderCPath);

            for (int i = 0; i < headerCLines.Length && changes < 3; i++)
            {
                // first hit => update line and numOfChanges, then go to next line
                if (headerCLines[i].Contains("#define MOD_NAME") && changes == 0)
                {
                    headerCLines[i] = $"#define MOD_NAME        \"{appName}_{appBranch}\"";
                    LogWrite($"1st hit: #define MOD_NAME updated with value \"{appName}_{appBranch}\"");
                    changes++;
                    continue;
                }
                // second hit
                if (headerCLines[i].Contains("#define MOD_VERSION") && !headerCLines[i].Contains("TAG_VERSION") && changes == 1)
                {
                    headerCLines[i] = $"  #define MOD_VERSION                           \"s{appVersion}\" SVN_VERSION";
                    LogWrite($"2nd hit: #define MOD_VERSION updated with value \"s{appVersion}\"");
                    changes++;
                    continue;
                }

                // third hit
                if (headerCLines[i].Contains("#define MOD_VERSION") && changes == 2)
                {
                    headerCLines[i] = $"  #define MOD_VERSION                           \"{appVersion}" + (headerCLines[i].Contains("DEV") ? "DEV" : "") + "\"";
                    LogWrite($"3rd hit: #define MOD_VERSION updated with value \"{appVersion}" + (headerCLines[i].Contains("DEV") ? "DEV" : "") + "\"");
                    changes++;
                    continue;
                    // at the next iteration the cycle ends
                }
            }

            // delete old file
            File.Delete(this.HeaderCPath);
            LogWrite("Deleted old header.c file");
            // create new file
            File.WriteAllLines(this.HeaderCPath, headerCLines);
            LogWrite("Created new header.c file");

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
