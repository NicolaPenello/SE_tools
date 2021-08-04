using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using Owners;

namespace PrepareEtoTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool readyToUpdate = false;

        private Regex regexApp = new Regex(@"^[a-zA-Z0-9_]*$");
        private Regex regexBranch = new Regex(@"^e[0-9]\d{0,1}$");
        private Regex regexVersion = new Regex(@"^[0-9][0-9][0-9]$");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseRootDir_LeftClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            fileDialog.Description = $"Seleziona la cartella della root";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringRootDir.Text = fileDialog.SelectedPath;
            }

        }

        private void CheckButton_LeftClick(object sender, RoutedEventArgs e)
        {
            // a path was inserted in the respective field of the form
            if (string.IsNullOrEmpty(StringRootDir.Text))
            {
                LogWrite($"Invalid source path (no path indicated in the form)");
                return;
            }

            if (!Directory.Exists(StringRootDir.Text))
            {
                LogWrite($"Invalid source path (directory not found)");
                return;
            }

            // check whether everything is OK to be processed
            readyToUpdate = true;

            // app
            if (!regexApp.IsMatch(StringApp.Text))
            {
                LogWrite($"Invalid app name");
                readyToUpdate = false;
            }
            else
            {
                LogWrite($"App name ==> {StringApp.Text}");
            }

            // branch
            if (!regexBranch.IsMatch(StringBranch.Text))
            {
                LogWrite($"Invalid branch name");
                readyToUpdate = false;
            }
            else
            {
                LogWrite($"Branch name ==> {StringBranch.Text}");
            }

            // version
            if (!regexVersion.IsMatch(StringVer.Text))
            {
                LogWrite($"Invalid version");
                readyToUpdate = false;
            }
            else
            {
                LogWrite($"Version ==> {StringVer.Text}");
            }

            UpdateBtn.IsEnabled = readyToUpdate;

            if (!readyToUpdate)
            {
                LogWrite($"Output cannot be generated: see Log");
                return;
            }

            LogWrite($"Ready to update the path {StringRootDir.Text}");
        }

        private void UpdateButton_LeftClick(object sender, RoutedEventArgs e)
        {
            // this frame of code should never be executed
            if (!readyToUpdate)
            {
                LogWrite($"Generation-check not yet passed: click \"{CheckBtn.Content}\" and see Log for info");
                return;
            }

            string rootPath = StringRootDir.Text;

            string appName = StringApp.Text;
            string appBranch = StringBranch.Text;
            string appVersion = StringVer.Text;

            // message box to warn the user about the app in the PhraseDb
            string msg = $"Prima di lanciare la modifica è necessario che sia stata creata l'app \"{appName}_{appBranch}\" in PhraseDB.\nE' già stata creata?";
            string caption = $"Creazione app in PhraseDB";
            MessageBoxResult phraseDbWarningResult = System.Windows.MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (phraseDbWarningResult == MessageBoxResult.No)
            {
                LogWrite($"Update aborted by the user: app {appName}_{appBranch} not present in PhraseDB");
                return;
            }
            LogWrite($"Update confirmed by the user: app {appName}_{appBranch} present in PhraseDB");


            EtoTouchPreparationOwner owner = new EtoTouchPreparationOwner(rootPath);
            owner.UpdateHeaderC(appName, appBranch, appVersion);
            owner.UpdateMakefile(appName, appBranch, appVersion);
            owner.UpdateStrdbC(appName, appBranch);
            owner.UpdateStrlH(appName, appBranch);
            owner.UpdateApplangMak(appName, appBranch);
            owner.UpdateLangH(appName, appBranch);
            owner.DeleteLpks(appName);
            owner.LaunchCommands(appName);

            File.WriteAllText(System.IO.Path.Combine(rootPath, "preparationReport.txt"), owner.Log);

        }


        private void StringApp_LostFocus(object sender, RoutedEventArgs e)
        {
            StringApp.Text = regexApp.IsMatch(StringApp.Text) ? StringApp.Text : "Not valid app name";
            readyToUpdate = false;
            UpdateBtn.IsEnabled = readyToUpdate;
        }

        private void StringBranch_LostFocus(object sender, RoutedEventArgs e)
        {
            StringBranch.Text = regexApp.IsMatch(StringBranch.Text) ? StringBranch.Text : "Not valid branch";
            readyToUpdate = false;
        }

        private void StringVer_LostFocus(object sender, RoutedEventArgs e)
        {
            StringVer.Text = regexApp.IsMatch(StringVer.Text) ? StringVer.Text : "Not valid version";
            readyToUpdate = false;
        }


        // app log
        private void LogWrite(string str)
        {
            StringLog.Text += $"{str} \n";
        }

        private void LogClear()
        {
            StringLog.Text = string.Empty;
        }

    }
}
