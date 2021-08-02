using EtoUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EtoHandlers
{
    internal class MakefileHandler
    {
        // fields
        private string _makefilePath;
        private string _log;

        // properties
        public string MakefilePath
        {
            get { return _makefilePath; }
            private set { _makefilePath = value; }
        }

        public string Log
        {
            get { return _log; }
        }

        // ctors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="makefilePath"></param>
        public MakefileHandler(string makefilePath)
        {
            this.MakefilePath = makefilePath;
            this._log = string.Empty;
            this.LogWrite($"Handler created: linked to path {this.MakefilePath}");
        }

        /// <summary>
        /// Example: appName = unflrtsa; appBranch = e12; appVersion = 601; 
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appBranch"></param>
        /// <param name="appVersion"></param>

        public void UpdateMakefile(string appName, string appBranch, string appVersion)
        {
            // validation of parameters is owned by the business logic at first and then by the preparation owner:
            // here we expect to have no troubles due to file-not-found or null-reference exceptions

            // we will count the number of changes as a further feedback and in order to optimize the updating 
            // we expect to update 3 points of the code
            int changes = 0;
            bool blockFound = false;

            var makefileLines = File.ReadAllLines(this.MakefilePath);

            var appType = EtoStrings.ToEnum(appName);

            // amico app
            if (appType == EtoProjects.AMICO)
            {
                // in amico projects 2 changes are needed
                for (int i = 0; i < makefileLines.Length && changes < 2; i++)
                {
                    // we want to get the right block of code
                    if (makefileLines[i].Contains($"dmMmb\\!{appName}") && !blockFound)
                    {
                        // once found the block, step back and look for the matches
                        i = i - 5;
                        blockFound = true;
                        continue;
                    }

                    // first hit => update line and numOfChanges, then go to next line
                    if (makefileLines[i].Contains("LANG_BUILD_REV") && changes == 0 && blockFound)
                    {
                        makefileLines[i] = $"AMICO_LANG_BUILD_REV = {appVersion}";
                        LogWrite($"1st hit - new line: {makefileLines[i]}");
                        changes++;
                        continue;
                    }
                    // second hit
                    if (makefileLines[i].Contains("LANG_NAME") && changes == 1 && blockFound)
                    {
                        makefileLines[i] = $"AMICO_LANG_NAME      = amico_{appBranch}";
                        LogWrite($"1st hit - new line: {makefileLines[i]}");
                        changes++;
                        continue;
                    }
                }
            }
            else // other apps
            {
                for (int i = 0; i < makefileLines.Length && changes < 3; i++)
                {
                    // we want to get the right block of code
                    if (makefileLines[i].Contains($"dmMmb\\!{appName}") && !blockFound)
                    {
                        // once found the block, step back and look for the matches
                        i = i - 5;
                        blockFound = true;
                        continue;
                    }

                    // first hit => update line and numOfChanges, then go to next line
                    if (makefileLines[i].Contains("LANG_BUILD_REV") && changes == 0 && blockFound)
                    {
                        switch (appType)
                        {
                            case EtoProjects.BCWC: makefileLines[i] = $"BCWC_LANG_BUILD_REV = {appVersion}"; break;
                            case EtoProjects.LEOGEN2: makefileLines[i] = $"LEOGEN2_LANG_BUILD_REV = {appVersion}"; break;
                            case EtoProjects.TRMCHLR: makefileLines[i] = $"TRMCHLR_LANG_BUILD_REV = {appVersion}"; break;
                            case EtoProjects.UNFLRLE_L: makefileLines[i] = $"UNFLRL_LANG_BUILD_REV = {appVersion}"; break;
                            case EtoProjects.UNFLRTSA: makefileLines[i] = $"BCWC_LANG_BUILD_REV = {appVersion}"; break;
                            default: throw new Exception(); 
                        }

                        LogWrite($"1st hit - new line: {makefileLines[i]}");
                        changes++;
                        continue;
                    }
                    // second hit
                    if (makefileLines[i].Contains("files") && makefileLines[i].Contains("rhodes2") && changes == 1 && blockFound)
                    {
                        switch (appType)
                        {
                            case EtoProjects.BCWC: makefileLines[i] = $"    $(GETLANG) files    aquacentr_{appBranch}	    rhodes2	    $(BCWC_PROJECT_NAME)    h\\stringdb      aos\\stringdb	app\\$(BCWC_DIR)\\h   app\\$(BCWC_DIR)\\applang"; break;
                            case EtoProjects.LEOGEN2: makefileLines[i] = $"	$(GETLANG) files  leogen2_{appBranch}	rhodes2	   $(LEOGEN2_PROJECT_NAME) h\\stringdb	 aos\\stringdb	app\\$(LEOGEN2_DIR)\\h  app\\$(LEOGEN2_DIR)\\applang"; break;
                            case EtoProjects.TRMCHLR: makefileLines[i] = $"	$(GETLANG) files  trmchlr_{appBranch}	rhodes2	   $(TRMCHLR_PROJECT_NAME) h\\stringdb	 aos\\stringdb	app\\$(TRMCHLR_DIR)\\h  app\\$(TRMCHLR_DIR)\\applang"; break;
                            case EtoProjects.UNFLRLE_L: makefileLines[i] = $"	$(GETLANG) files  unflrle_l_{appBranch}	rhodes2	   $(UNFLRL_PROJECT_NAME) h\\stringdb	 aos\\stringdb	app\\$(UNFLR_LE_L_DIR)\\h  app\\$(UNFLR_LE_L_DIR)\\applang"; break;
                            case EtoProjects.UNFLRTSA: makefileLines[i] = $"	$(GETLANG) files  unflrtsa_{appBranch}	rhodes2	   $(UNFLRTSA_PROJECT_NAME) h\\stringdb	 aos\\stringdb	app\\$(UNFLR_TSA_DIR)\\h  app\\$(UNFLR_TSA_DIR)\\applang"; break;
                            default: throw new Exception();
                        }
                        LogWrite($"2nd hit - new line: {makefileLines[i]}");
                        changes++;
                        continue;
                    }

                    // third hit
                    if (makefileLines[i].Contains("binary") && makefileLines[i].Contains("rhodes2") && changes == 2 && blockFound)
                    {
                        switch (appType)
                        {
                            case EtoProjects.BCWC: makefileLines[i] = $"    $(GETLANG) binary   aquacentr_{appBranch}	    rhodes2	    $(BCWC_LANG_BUILD_REV)  $(BCWC_PROJECT_NAME)    \\$(BCWC_DIR)\\applang\\lpktypes.txt"; break;
                            case EtoProjects.LEOGEN2: makefileLines[i] = $"	$(GETLANG) binary leogen2_{appBranch}	rhodes2	   $(LEOGEN2_LANG_BUILD_REV) $(LEOGEN2_PROJECT_NAME) \\$(LEOGEN2_DIR)\\applang\\lpktypes.txt"; break;
                            case EtoProjects.TRMCHLR: makefileLines[i] = $"	$(GETLANG) binary trmchlr_{appBranch}	rhodes2	   $(TRMCHLR_LANG_BUILD_REV) $(TRMCHLR_PROJECT_NAME) \\$(TRMCHLR_DIR)\\applang\\lpktypes.txt"; break;
                            case EtoProjects.UNFLRLE_L: makefileLines[i] = $"	$(GETLANG) binary unflrle_l_{appBranch}	rhodes2	   $(UNFLRL_LANG_BUILD_REV) $(UNFLRL_PROJECT_NAME) \\$(UNFLR_LE_L_DIR)\\applang\\lpktypes.txt"; break;
                            case EtoProjects.UNFLRTSA: makefileLines[i] = $"	$(GETLANG) binary unflrtsa_{appBranch}	rhodes2	   $(UNFLRTSA_LANG_BUILD_REV) $(UNFLRTSA_PROJECT_NAME) \\$(UNFLR_TSA_DIR)\\applang\\lpktypes.txt"; break;
                            default: throw new Exception();
                        }
                        LogWrite($"3rd hit - new line: {makefileLines[i]}");
                        changes++;
                        continue;
                        // at the next iteration the cycle ends
                    }
                }

            }


            // delete old file
            File.Delete(this.MakefilePath);
            LogWrite("Deleted old makefile file");
            // create new file
            File.WriteAllLines(this.MakefilePath, makefileLines);
            LogWrite("Created new makefile file");

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
