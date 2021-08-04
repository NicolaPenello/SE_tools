using EtoHandlers;
using EtoUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Owners
{
    internal class EtoTouchPreparationOwner
    {
        private string _rootPath;
        private string _log;
        private List<string> _appPaths;

        public string RootPath
        {
            get { return _rootPath; }
            private set { _rootPath = value; }
        }

        public List<string> AppPaths
        {
            get { return _appPaths; }
            private set { _appPaths = value; }
        }

        public string Log
        {
            get { return _log; }
            private set { _log = value; }
        }

        /// <summary>
        /// rootPath = path of the root of the working copy of the repository
        /// </summary>
        /// <param name="rootPath"></param>
        public EtoTouchPreparationOwner(string rootPath)
        {
            this._log = string.Empty;
            LogWrite($"Trying to create the owner");
            if (rootPath == null || !Directory.Exists(rootPath))
            {
                LogWrite("Creation aborted: invalid root path");
                throw new Exception();
            }

            this.RootPath = rootPath;
            this.AppPaths = new List<string>();
            LogWrite($"Creation succeded: owner linked to {this.RootPath}");

            // look for apps' directories
            string[] retrievedAppSubdirectories;
            try
            {
                retrievedAppSubdirectories = Directory.GetDirectories(Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR), "!*");
            }
            catch (DirectoryNotFoundException)
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR)} not found: invalid directory-tree");
                return;
            }


            // no valid app-subdir was found
            if (retrievedAppSubdirectories.Length == 0)
            {
                LogWrite("Trouble! No valid app-subdirectory was found");
                return;
            }

            // valid app-subdirs were found           
            if (retrievedAppSubdirectories.Length > 1) // multiple hits
            {
                LogWrite("Trouble? More than one valid app-subdirectories were found");
            }
            else // one hit
            {
                LogWrite($"Found exactly one valid app-subdirectory: {retrievedAppSubdirectories[0]}");
            }

            // populate the list of apps
            foreach (var dir in retrievedAppSubdirectories)
            {
                this.AppPaths.Add(dir);
            }
        }


        public void UpdateHeaderC(string appName, string etoBranch, string appVersion)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the header.c file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.HEADER_C })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}")} doesn't contain file {EtoStrings.HEADER_C}");
                return;
            }

            // create the handler
            var handler = new HeaderCHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.HEADER_C }));
            // update the header.c
            handler.UpdateHeaderC(appName, etoBranch, appVersion);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);

        }

        public void UpdateMakefile(string appName, string etoBranch, string appVersion)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the makefile file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.MAKEFILE })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR)} doesn't contain file {EtoStrings.MAKEFILE}");
                return;
            }

            // create the handler
            var handler = new MakefileHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.MAKEFILE }));
            // update the makefile
            handler.UpdateMakefile(appName, etoBranch, appVersion);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);

        }

        public void UpdateStrdbC(string appName, string appBranch)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the [appName]strdb.c file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, $"{((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRDB_C}" })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR)} doesn't contain file {((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRDB_C}");
                return;
            }

            // create the handler
            var handler = new StrdbCHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, $"{((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRDB_C}" }));
            // update the [appName]strdb.c file
            handler.UpdateStrdbC(appName, appBranch);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);
        }

        public void UpdateStrlH(string appName, string appBranch)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the [appName]strl.h file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.H_DIR, $"{((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRL_H}" })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR)} doesn't contain file {((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRL_H}");
                return;
            }

            // create the handler
            var handler = new StrlHHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.H_DIR, $"{((EtoStrings.ToEnum(appName)) == EtoProjects.BCWC ? EtoStrings.AQUACENTR : appName)}{EtoStrings.STRL_H}" }));
            // update the [appName]strl.h file
            handler.UpdateStrlH(appName, appBranch);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);
        }

        public void UpdateApplangMak(string appName, string etoBranch)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the makefile file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, EtoStrings.APPLANG_MAK })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR)} doesn't contain file {EtoStrings.APPLANG_MAK}");
                return;
            }

            // create the handler
            var handler = new ApplangMakHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, EtoStrings.APPLANG_MAK }));
            // update the applang.mak
            handler.UpdateApplangMak(appName, etoBranch);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);

        }

        public void UpdateLangH(string appName, string etoBranch)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the makefile file doesn't exist => abort
            if (!File.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.H_DIR, EtoStrings.LANG_H })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.H_DIR)} doesn't contain file {EtoStrings.LANG_H}");
                return;
            }

            // create the handler
            var handler = new LangHHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.H_DIR, EtoStrings.LANG_H }));
            // update the lang.h
            handler.UpdateLangH(appName, etoBranch);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);

        }

        public void DeleteLpks(string appName)
        {
            if (!PassedDefaultChecks(appName))
            {
                return;
            }

            // if the langpack dir doesn't exist => abort
            if (!Directory.Exists(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, EtoStrings.LANGPACKS_DIR })))
            {
                LogWrite($"Path {Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, EtoStrings.LANGPACKS_DIR)} doesn't exist");
                return;
            }

            // create the handler
            var handler = new LpkHandler(Path.Combine(new string[] { this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}", EtoStrings.APPLANG_DIR, EtoStrings.LANGPACKS_DIR }));
            // update the lang.h
            handler.DeleteLpks(appName);
            // copy the handler-log into the owner-log
            LogWrite(handler.Log);

        }

        public void LaunchCommands(string appName)
        {
            if (!PassedDefaultChecks(appName))
            { 
                return;
            }
            
            if (!Directory.Exists(Path.Combine(RootPath, EtoStrings.APP_DIR)) || !File.Exists(Path.Combine(RootPath, EtoStrings.APP_DIR, EtoStrings.MAKEFILE)))
            {
                LogWrite($"Directory {Path.Combine(RootPath, EtoStrings.APP_DIR, EtoStrings.MAKEFILE)} doesn't exist");
                return;
            }


            var process = new Process();

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = this.RootPath,
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.StartInfo = startInfo;

            process.Start();
            process.StandardInput.WriteLine($"cd {EtoStrings.APP_DIR}");

            // clean
            process.StandardInput.WriteLine($"{CommandLineCommands.NMAKE} {CommandLineCommands.CLEAN}");
            LogWrite($"Command {CommandLineCommands.NMAKE} {CommandLineCommands.CLEAN} launched");

            string appLangCommand;
            string appLpkCommand;

            switch (EtoStrings.ToEnum(appName))
            {
                case EtoProjects.AMICO: appLangCommand = "amicolang"; appLpkCommand = "amicolpk"; break;
                case EtoProjects.BCWC: appLangCommand = "aquacentrlang"; appLpkCommand = "aquacentrlpk"; break;
                case EtoProjects.LEOGEN2: appLangCommand = "leogen2lang"; appLpkCommand = "leogen2lpk"; break;
                case EtoProjects.TRMCHLR: appLangCommand = "trmchlrlang"; appLpkCommand = "trmchlrlpk"; break;
                case EtoProjects.UNFLRLE_L: appLangCommand = "unflrlellang"; appLpkCommand = "unflrlellpk"; break;
                case EtoProjects.UNFLRTSA: appLangCommand = "unflrtsalang"; appLpkCommand = "unflrtsalpk"; break;
                default:
                    throw new Exception();
            }



            // lang
            process.StandardInput.WriteLine($"{CommandLineCommands.NMAKE} {appLangCommand}");
            LogWrite($"Command {CommandLineCommands.NMAKE} {appLangCommand} launched");

            // lpk
            process.StandardInput.WriteLine($"{CommandLineCommands.NMAKE} {appLpkCommand}");
            LogWrite($"Command {CommandLineCommands.NMAKE} {appLpkCommand} launched");

            process.WaitForExit();


            LogWrite($"Prompt exited");

        }

        /// <summary>
        /// Tells if the project [appName] is active or obsolete; if the prject [appName] is unexpected, then it is treated as active
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        private bool IsProjectObsolete(string appName)
        {
            var appType = EtoStrings.ToEnum(appName);
            if ((appType & EtoProjects.OBSOLETE) != 0)
            {
                return true;
            }
            return false;
        }

        private bool IsProjectUnexpected(string appName)
        {
            var appType = EtoStrings.ToEnum(appName);
            if (appType == EtoProjects.UNEXPECTED)
            {
                return true;
            }
            return false;
        }

        private bool PassedDefaultChecks(string appName)
        {
            if (IsProjectObsolete(appName))
            {
                LogWrite($"App {appName} is obsolete: process aborted.");
                return false;
            }

            if (IsProjectUnexpected(appName))
            {
                LogWrite($"Unexpected app {appName}: process aborted.");
                return false;
            }

            // if the app we want to update doesn't exist => abort
            if (!this.AppPaths.Contains(Path.Combine(this.RootPath, EtoStrings.APP_DIR, EtoStrings.DMMMB_DIR, $"!{appName}")))
            {
                LogWrite($"App {appName} doesn't exist: process aborted.");
                return false;
            }

            return true;
        }

        // log methods
        public void ClearLog()
        {
            this.Log = string.Empty;
            LogWrite("Log cleared");
        }
        private void LogWrite(string str)
        {
            this.Log += $"{str}{Environment.NewLine}";
        }
    }
}
