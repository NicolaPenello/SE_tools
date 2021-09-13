using dotSrc_dotBit.LZMA;
using dotSrc_dotBin;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Diagnostics;

namespace dotSrc_dotBit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _VERSION_ = "1.0.3";

        private const string sourceSuffix = "src"; // unused
        private const string binarySuffix = "bin";
        private const string simulationBinarySuffix = "simbin";
        private const string zipSuffix = "zip";
        private const string sevenZipSuffix = "7z";
        private const string twoCfSuffix = "2cf";
        private const string blbSuffix = "blb";

        private const string DEVICE_DESCRIPTION = "DEVICE_DESCRIPTION";
        private const string PRJ_NAME = "PRJ_NAME";
        private const string PRJ_VERSION = "PRJ_VERSION";

        private string fullName;            // STD =>  [app]_[verSTD]; ETO => [app]_[spec]_[verETO]; in case of redundancy: [fullname]_[datetime]
        private string sevenZipFileName;    // [fullname]_src.7z        
        private string srcFolderPath;       // 
        private string binTargetPLAN1ADDRESSxxxPath;
        private string binSimulatorProjectPath;

        private bool readyToGenerate = false;

        private Regex regexApp = new Regex(@"^[a-zA-Z0-9]*$");
        private Regex regexSpec = new Regex(@"^[a-zA-Z0-9]*$");
        private Regex regexVersionSpecial = new Regex(@"^[1-9]\.[0-9]\d{0,1}\.[0-9][0-9][0-9]$");
        private Regex regexVersionStandard = new Regex(@"^[1-9]\.[0-9]\d{0,1}$");
        private Regex regexVersionObsolete = new Regex(@"^[1-9]\.[0-9]\d{0,1}$");


        public MainWindow()
        {
            InitializeComponent();
            this.Title += $" - {_VERSION_}";
            IsSpecEnabled.IsChecked = false;
        }


        private void ChooseOtsButton_LeftClick(object sender, RoutedEventArgs e)
        {
            LogClear();

            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "OneToolSolution files(*.ots)| *.ots";
            if (fileDialog.ShowDialog() == true)
            {
                // source path is one level below from the .ots file
                srcFolderPath = System.IO.Path.GetFullPath(fileDialog.FileName + $"\\..");

                // print in the log textbox
                StringPath.Text = srcFolderPath;

                // look for the ADDRESS_*** directory and eventually look for the .2cf file
                string binTargetPLAN1Path = $"{srcFolderPath}\\Bin\\Target\\pLAN1";

                // first of all check for the pLAN1 folder: if it doesn't exist then abort
                if (!Directory.Exists(binTargetPLAN1Path))
                {
                    LogWrite($"Directory {binTargetPLAN1Path} was not found: process aborted. Check for problems in the file-tree of the solution folder");
                    return;
                }
                
                var arrDirectories = Directory.GetDirectories(binTargetPLAN1Path);

                // if no sub-directories or two or more sub-directories are found, then return
                if (arrDirectories.Length == 0)
                {
                    LogWrite("Directory ADDRESS_*** does not exist");
                    return;
                }
                else if (arrDirectories.Length > 1)
                {
                    LogWrite("Found more than one directory ADDRESS_***");
                    return;
                }

                // exactly one sub-directory was found 
                binTargetPLAN1ADDRESSxxxPath = arrDirectories.FirstOrDefault();

                LogWrite($"Found one directory \"{binTargetPLAN1ADDRESSxxxPath}\"");

                string file2cfPath = Directory.GetFiles(binTargetPLAN1ADDRESSxxxPath, $"*.{twoCfSuffix}").FirstOrDefault();

                // if no .2cf file was found, then return
                if (file2cfPath == null)
                {
                    LogWrite("File .2cf does not exist.");
                    return;
                }

                // at least one .2cf file was found
                LogWrite($"Found .2cf file at \"{file2cfPath}\"");

                // try to retrieve the informations to populate the fields of the form by parsing the .2cf file (xml)
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                try
                {
                    xmlDoc.LoadXml(File.ReadAllText(file2cfPath));
                    LogWrite($"File .2cf correctly loaded");
                }
                catch (Exception exc)
                {
                    LogWrite($"File .2cf not loaded because of the following exception\n{exc.Message}\n");
                    return;
                }

                XmlNode root = xmlDoc.GetElementsByTagName(DEVICE_DESCRIPTION)[0];
                if (root == null)
                {
                    LogWrite($"Cannot find the xml node {DEVICE_DESCRIPTION}");
                    return;
                }
                LogWrite($"Xml node {DEVICE_DESCRIPTION} was found");


                string strPRJ_NAME = (root as XmlElement).GetAttribute(PRJ_NAME);
                if (strPRJ_NAME == null)
                {
                    LogWrite($"Cannot find the xml attribute {PRJ_NAME}");
                    return;
                }
                LogWrite($"Xml attribute \"{PRJ_NAME}\" was found and it is \"{strPRJ_NAME}\"");


                string strPRJ_VERSION = (root as XmlElement).GetAttribute(PRJ_VERSION);
                LogWrite(strPRJ_VERSION == null? $"Cannot find the xml attribute {PRJ_VERSION}" : $"Xml attribute {PRJ_VERSION} was found and it is {strPRJ_VERSION}");

                // populate the form according to the just-retrieved informations
                string[] splittedPRJ_NAME = strPRJ_NAME.Split('_');
                // check if PRJ_NAME was correctly formatted  [app]_[spec]
                if (splittedPRJ_NAME.Count() > 2)
                {
                    LogWrite("PRJ_NAME not correctly formatted in .2cf file");
                    return;
                }
                else if(IsSpecEnabled.IsChecked == false && splittedPRJ_NAME.Count() > 1)
                {
                    LogWrite("PRJ_NAME for standard SW not correctly formatted in .2cf file");
                    return;
                }
                else if(IsSpecEnabled.IsChecked == true && splittedPRJ_NAME.Count() < 2)
                {
                    LogWrite("PRJ_NAME for special SW not correctly formatted in .2cf file");
                    return;
                }
                // we have the right format of the PRJ_NAME i.e. standard -> [app] ; special -> [app]_[spec]


                StringApp.Text = regexApp.IsMatch(splittedPRJ_NAME[0]) ? splittedPRJ_NAME[0] : $"{splittedPRJ_NAME[0]} ==> Not valid app name in .2cf file ";
                if (IsSpecEnabled.IsChecked == true) // SPECIAL
                {
                    StringSpec.Text = regexApp.IsMatch(splittedPRJ_NAME[1]) ? splittedPRJ_NAME[1] : $"{splittedPRJ_NAME[1]} ==> Not valid app name in .2cf file ";
                }

                
                Regex regexVersion;
                if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == false) // SPECIAL
                {
                    regexVersion = regexVersionSpecial;                  
                }
                else if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == true) // OBSOLETE-ETO
                {
                    regexVersion = regexVersionObsolete;
                }
                else // STANDARD
                {
                    regexVersion = regexVersionStandard;
                }

                // only for special version, try first to "reconstruct" a valid 
                string[] splitted_PRJ_VERSION;
                if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == false) // SPECIAL
                {
                    // check if the version is in the style x.y(y).(zz)z and in that case left-pad with zeroes the right-most field
                    if ( (new Regex(@"^[1-9]\.[0-9]\d{0,1}\.[0-9]\d{0,2}$")).IsMatch(strPRJ_VERSION) )
                    {
                        splitted_PRJ_VERSION = strPRJ_VERSION.Split('.');
                        splitted_PRJ_VERSION[2] = splitted_PRJ_VERSION[2].PadLeft(3,'0'); // padding

                        // overwrite the version to be processed
                        strPRJ_VERSION = string.Join(".", splitted_PRJ_VERSION);
                    }
                }
                else // for the standard and obsolete-eto, ignore the rightmost field if it is zero-valued
                {
                    // check if the version is in the style x.y(y).0 and in that case ignore the right-most field
                    if ((new Regex(@"^[1-9]\.[0-9]\d{0,1}\.0$")).IsMatch(strPRJ_VERSION))
                    {
                        splitted_PRJ_VERSION = strPRJ_VERSION.Split('.');

                        // overwrite the version to be processed
                        strPRJ_VERSION = $"{splitted_PRJ_VERSION[0]}.{splitted_PRJ_VERSION[1]}";
                    }
                }

                StringVer.Text = regexVersion.IsMatch(strPRJ_VERSION) ? strPRJ_VERSION : "Not valid version number in .2cf file";     

                // the default output folder is one level below from the src folder
                StringDestDir.Text = System.IO.Path.GetFullPath(srcFolderPath + $"\\..");

            }
        }

        private void ChooseDestDir_LeftClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            fileDialog.Description = $"Seleziona la cartella di output";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringDestDir.Text = fileDialog.SelectedPath;
            }
        }

        private void CheckButton_LeftClick(object sender, RoutedEventArgs e)
        {
            // a path was inserted in the respective field of the form
            if (string.IsNullOrEmpty(srcFolderPath))
            {
                LogWrite($"Invalid source path (no path indicated in the form)");
                return;
            }

            // check whether everything is OK to be processed
            readyToGenerate = true;
            if (!regexApp.IsMatch(StringApp.Text))
            {
                LogWrite($"Invalid app name");
                readyToGenerate = false;
            }
            else
            {
                LogWrite($"App name ==> {StringApp.Text}");
            }

            if (IsSpecEnabled.IsChecked == true) // SPECIAL
            {
                if (!regexSpec.IsMatch(StringSpec.Text))
                {
                    LogWrite($"Invalid specialty name");
                    readyToGenerate = false;
                }
                else
                {
                    LogWrite($"Specialty name ==> {StringSpec.Text}");
                }
                
            }

            if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == false) // SPECIAL
            {
                if (!regexVersionSpecial.IsMatch(StringVer.Text))
                {
                    LogWrite($"Invalid special version number");
                    readyToGenerate = false;
                }
                else
                {
                    LogWrite($"Special version number ==> {StringVer.Text}");
                }
            }
            else if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == false) // OBSOLETE-ETO
            {
                if (!regexVersionObsolete.IsMatch(StringVer.Text))
                {
                    LogWrite($"Invalid obsolete-special version number");
                    readyToGenerate = false;
                }
                else
                {
                    LogWrite($"Special version number ==> {StringVer.Text}");
                }
            }
            else // STANDARD
            {
                if (!regexVersionStandard.IsMatch(StringVer.Text))
                {
                    LogWrite($"Invalid standard version number");
                    readyToGenerate = false;
                }
                else
                {
                    LogWrite($"Standard version number ==> {StringVer.Text}");
                }
            }

            GenerateBtn.IsEnabled = readyToGenerate;

            if (!readyToGenerate)
            {
                LogWrite($"Output cannot be generated: see Log");
                return;
            }

            if (IsSpecEnabled.IsChecked == true) // SPECIAL
            {
                fullName = $"{StringApp.Text}_{StringSpec.Text}_{StringVer.Text}";
            }
            else
            {
                fullName = $"{StringApp.Text}_{StringVer.Text}";
            }

            sevenZipFileName = $"{fullName}.{sevenZipSuffix}";

            LogWrite($"Ready to generate the archive {sevenZipFileName}");


        }

        private void AttachmentButton_LeftClick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            fileDialog.Description = $"Seleziona la cartella dei file aggiuntivi";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringAttachmentPath.Text = fileDialog.SelectedPath;
            }
        }

        private async void GenerateButton_LeftClick(object sender, RoutedEventArgs e)
        {
            // this frame of code should never be executed
            if (!readyToGenerate)
            {
                LogWrite($"Generation-check not yet passed: click \"{CheckBtn.Content}\" and see Log for info");
                return;
            }
            // zip archive will be saved on the output path shown in the respective field of the form
            string destinationPath = StringDestDir.Text;

            // show the progress bar
            ProgressBarWindow progressBarWindow = new ProgressBarWindow();
            progressBarWindow.Show();

            // binaries' archive
            if (IsBinZipGenerationEnabled.IsChecked == true)
            {
                progressBarWindow.ProgressBarLabel.Text = $"Sto generando l'archivio .zip dei binari";

                // check if a file with the same name already exists: in this case append date and hour to the name of the archive
                if (File.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}.{zipSuffix}")) ||
                    Directory.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}")))
                {
                    string caption = "Archivio .zip dei binari già esistente";
                    string msg = $"Sovrascrivere il file {System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}.{zipSuffix}")}?\nCliccando No, all'archivio verranno accodate data e ora di creazione. ";
                    MessageBoxResult binDialogResult = System.Windows.MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    // if the overwriting was chosen, then delete the old files or directories
                    if (binDialogResult == MessageBoxResult.Yes)
                    {
                        if (File.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}.{zipSuffix}")))
                        {
                            File.Delete(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}.{zipSuffix}"));
                        }
                        if (Directory.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}")))
                        {
                            Directory.Delete(System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}"));
                        }
                    }
                    if (binDialogResult == MessageBoxResult.No)
                    {
                        fullName += $"_{System.DateTime.Now.ToString("ddMMyyyy_hhmm")}";
                        // fullName += $"{System.DateTime.Now.Day}{System.DateTime.Now.Month}{System.DateTime.Now.Year}_{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}{System.DateTime.Now.Second}";
                    }
                }

                // create the binaries
                LogWrite($"Generation at the path \"{destinationPath}\" of the binaries' zip-archive named \"{fullName}_{binarySuffix}.{zipSuffix}\"");
                PrepareTargetBinariesFolder(binTargetPLAN1ADDRESSxxxPath, StringDestDir.Text);

                LogWrite($"FINISHED");

            }

            // 7zip source code 
            if (IsSrc7zipGenerationEnabled.IsChecked == true)
            {
                // check if a file with the same name already exists: in this case append date and hour to the name of the archive
                if (File.Exists(System.IO.Path.Combine(destinationPath, $"{sevenZipFileName}")))
                {
                    string caption = "Archivio sorgenti .7zip già esistente";
                    string msg = $"Sovrascrivere il file {System.IO.Path.Combine(destinationPath, $"{sevenZipFileName}")}?\nCliccando No, all'archivio verranno accodate data e ora di creazione. ";
                    MessageBoxResult srcDialogResult = System.Windows.MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    // if the overwriting was chosen, then delete the old files or directories
                    if (srcDialogResult == MessageBoxResult.Yes)
                    {
                        File.Delete(System.IO.Path.Combine(destinationPath, $"{sevenZipFileName}"));
                    }
                    if (srcDialogResult == MessageBoxResult.No)
                    {
                        string[] splittedSevenZipFileName = sevenZipFileName.Split('.');
                        splittedSevenZipFileName[splittedSevenZipFileName.Length-2] += $"_{System.DateTime.Now.ToString("ddMMyyyy_hhmm")}";
                        sevenZipFileName = string.Join('.', splittedSevenZipFileName);
                    }
                }

                LogWrite($"Generation at the path \"{destinationPath}\" of the source's 7zip-archive named \"{sevenZipFileName}\" of the directory \"{srcFolderPath}\"");

                progressBarWindow.ProgressBarLabel.Text = $"Sto generando l'archivio .7zip dei sorgenti: l'operazione può richiedere diversi minuti";
                // since the LZMA compressioni is lengthy, let's make it async(ronous)ly
                await GenerateSource7zip(srcFolderPath, destinationPath, sevenZipFileName);
                //LZMAUtils.CreateArchive(srcFolderPath, destinationPath, sevenZipFileName);

                LogWrite($"FINISHED");
            }

            // simulation binaries
            if (IsSimulationZipGenerationEnabled.IsChecked == true)
            {
                progressBarWindow.ProgressBarLabel.Text = $"Sto generando i binari per la simulazione";

                // check if a file with the same name already exists: in this case append date and hour to the name of the archive
                if (File.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}.{zipSuffix}")) ||
                    Directory.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}")))
                {
                    string caption = "Archivio .zip dei binari già esistente";
                    string msg = $"Sovrascrivere il file {System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}.{zipSuffix}")}?\nCliccando No, all'archivio verranno accodate data e ora di creazione. ";
                    MessageBoxResult binDialogResult = System.Windows.MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                    // if the overwriting was chosen, then delete the old files or directories
                    if (binDialogResult == MessageBoxResult.Yes)
                    {
                        if (File.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}.{zipSuffix}")))
                        {
                            File.Delete(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}.{zipSuffix}"));
                        }
                        if (Directory.Exists(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}")))
                        {
                            Directory.Delete(System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}"));
                        }
                    }
                    if (binDialogResult == MessageBoxResult.No)
                    {
                        fullName += $"_{System.DateTime.Now.ToString("ddMMyyyy_hhmm")}";
                        // fullName += $"{System.DateTime.Now.Day}{System.DateTime.Now.Month}{System.DateTime.Now.Year}_{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}{System.DateTime.Now.Second}";
                    }
                }

                // create the simulation binaries
                LogWrite($"Generation at the path \"{destinationPath}\" of the zip-archive of simulation binaries named \"{fullName}_{simulationBinarySuffix}.{zipSuffix}\"");

                PrepareSimulatorBinariesFolder(destinationPath);

                LogWrite($"FINISHED");
            }


            // hide the progress bar
            progressBarWindow.Hide();

            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        // async wrapper for LZMA compression
        private async Task GenerateSource7zip(string srcPath, string destPath, string archivePath)
        {
            await Task.Run(()=> { LZMAUtils.CreateArchive(srcPath, destPath, archivePath); });           
        }

        private void IsSpecEnabled_Checked(object sender, RoutedEventArgs e)
        {
            StringSpec.IsEnabled = true;
            IsObsoleteEnabled.IsEnabled = true;
            ModeLabel.Text = "ETO mode";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        private void IsSpecEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            StringSpec.IsEnabled = false;
            IsObsoleteEnabled.IsEnabled = false;
            IsObsoleteEnabled.IsChecked = false;
            ModeLabel.Text = "STANDARD mode";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        private void IsObsoleteEnabled_Checked(object sender, RoutedEventArgs e)
        {
            ModeLabel.Text = "OBSOLETE-ETO mode";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        private void IsObsoleteEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            ModeLabel.Text = "ETO mode";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        private void IsSrc7zipGenerationEnabled_Checked(object sender, RoutedEventArgs e)
        { 
            //
        }

        private void IsSrc7zipGenerationEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void IsBinZipGenerationEnabled_Checked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void IsBinZipGenerationEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void IsSimulationZipGenerationEnabled_Checked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void IsSimulationZipGenerationEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            //
        }


        private void LogWrite(string str)
        {
            StringLog.Text += $"{str} \n";
        }

        private void LogClear()
        {
            StringLog.Text = string.Empty;
        }

        private void PrepareSimulatorBinariesFolder(string destinationPath)
        {
            // look for the Simulator directory and check if it is populated
            string binSimulatorPath = System.IO.Path.Combine(srcFolderPath, $"Bin", $"Simulator");
            if (!Directory.Exists(binSimulatorPath))
            {
                LogWrite($"Directory {binSimulatorPath} was not found: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }
 
            var arrSimulatorDirectories = Directory.GetDirectories(binSimulatorPath);
            if (arrSimulatorDirectories.Length == 0)
            {
                LogWrite($"Directory {binSimulatorPath} is empty: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }
            else if (arrSimulatorDirectories.Length > 1)
            {
                LogWrite($"Directory {binSimulatorPath} contains two sub-directories: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }

            LogWrite($"Found one directory \"{binSimulatorProjectPath}\"");
            binSimulatorProjectPath = arrSimulatorDirectories.FirstOrDefault();

            var files = Directory.GetFiles(binSimulatorProjectPath);

            if (files.Length == 0)
            {
                LogWrite($"Directory {binSimulatorProjectPath} is empty: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }

            // add to directory the files related to the calculation of the crc
            string crcBat = @".\CRC\crc.bat";
            string crcExe = @".\CRC\CRC.exe";

            File.Copy(crcBat, $"{binSimulatorProjectPath}\\crc.bat", true);
            File.Copy(crcExe, $"{binSimulatorProjectPath}\\CRC.exe", true);

            // check CRC
            var process = new Process();

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = binSimulatorProjectPath,
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = $"cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            process.StartInfo = startInfo;

            process.Start();

            /* passare per argomento al batch il file .blb */
            process.StandardInput.WriteLine($"crc.bat {binSimulatorProjectPath.Substring(binSimulatorPath.Length + 1)}.{blbSuffix}");
            process.StandardInput.WriteLine($"exit");

            // 2 seconds are more than enough to end the CRC validation
            process.WaitForExit(2000);

            var tempOP = process.StandardOutput.ReadToEnd().Split(Environment.NewLine);

            process.Close();

            // delete the just-added files
            File.Delete($"{binSimulatorProjectPath}\\crc.bat");
            File.Delete($"{binSimulatorProjectPath}\\CRC.exe");

            
            foreach (var line in tempOP)
            {
                LogWrite(line);
            }

            if (tempOP.Length < 3)
            {
                LogWrite($"Unexpected output of cmd.exe during CRC validation: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }
            var penultimateRow = tempOP[tempOP.Length-3];

            int? crcErrorLevel;
            try
            {
                crcErrorLevel = int.Parse(penultimateRow.Split(' ').Last());
            }
            catch (Exception)
            {
                LogWrite($"Unexpected value of %ERRORLEVEL% during CRC validation: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }

            // if the process fails (i.e. %errorlevel% != 0) then abort generation of simulation binaries
            if (crcErrorLevel != 0 || crcErrorLevel == null) 
            {
                LogWrite($"CRC check failed: Simulator binaries cannot be generated");
                binSimulatorProjectPath = null;
                return;
            }

            // create the output file
            ZipFile.CreateFromDirectory(binSimulatorProjectPath, System.IO.Path.Combine(destinationPath, $"{fullName}_{simulationBinarySuffix}.{zipSuffix}"), CompressionLevel.Optimal, true);
        }

        private void PrepareTargetBinariesFolder(string targetPlanAddressxxxPath, string destinationPath)
        {
            // create tree of the output directory
            string binDir = System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}");
            Directory.CreateDirectory(binDir);
            string otherFilesDir = System.IO.Path.Combine(binDir, $"Altri files");
            Directory.CreateDirectory(otherFilesDir);

            // look for the .2cd, .2cf, .2ct, .grp, .dev, .pvt files and copy if they exist
            string[] otherFilesExtensions = { "2cd","2cf","2ct","grp","dev","pvt"};
            string[] tempFiles;
            List<string> otherFiles = new List<string>();
            foreach (var ext in otherFilesExtensions)
            {
                tempFiles = Directory.GetFiles(targetPlanAddressxxxPath, $"*.{ext}");
                if (tempFiles.Count() == 0)
                {
                    LogWrite($"Not found .{ext} file");
                }
                else
                { 
                    if (tempFiles.Count() > 1)
                    {
                        LogWrite($"Found two or more .{ext} files");
                    }
                    else
                    {
                        LogWrite($"Found exactly one .{ext} file");
                    }
                    if (ext == $"pvt") // there may be multiple .pvt files: copy them all
                    {
                        foreach (var pvtFile in tempFiles)
                        {
                            otherFiles.Add(pvtFile);
                            LogWrite($"Added \"{tempFiles[0]}\" to directory \"Altri files\"");

                            File.Copy(pvtFile, System.IO.Path.Combine(otherFilesDir, pvtFile.Substring(targetPlanAddressxxxPath.Length + 1)), true);
                        }
                    }
                    else // in any other case there should be one file per extension: so just copy the first one
                    {
                        otherFiles.Add(tempFiles[0]);
                        LogWrite($"Added \"{tempFiles[0]}\" to directory \"Altri files\"");

                        File.Copy(tempFiles[0], System.IO.Path.Combine(otherFilesDir, tempFiles[0].Substring(targetPlanAddressxxxPath.Length + 1)), true);
                    }
                    
                }
            }


            // look for the .bin, .grt, .vvv, .vv2, .iup files
            string[] itTheRootFilesExtensions = { "bin", "grt", "vvv", "vv2", "iup"};
            List<string> inTheRootFiles = new List<string>();
            foreach (var ext in itTheRootFilesExtensions)
            {
                tempFiles = Directory.GetFiles(targetPlanAddressxxxPath, $"*.{ext}");
                if (tempFiles.Count() == 0)
                {
                    LogWrite($"Not found .{ext} file");
                }
                else
                {
                    if (tempFiles.Count() > 1)
                    {
                        LogWrite($"Found two or more .{ext} files");
                    }
                    else
                    {
                        LogWrite($"Found exactly one .{ext} file");
                    }
                    if (ext == $"iup") // there may be multiple .iup files: copy them all
                    {
                        foreach (var iupFile in tempFiles)
                        {
                            inTheRootFiles.Add(iupFile);
                            LogWrite($"Added \"{tempFiles[0]}\" to directory \"Altri files\"");

                            File.Copy(iupFile, System.IO.Path.Combine(binDir, iupFile.Substring(targetPlanAddressxxxPath.Length + 1)), true);
                        }
                    }
                    else // in any other case there should be one file per extension: so just copy the first one
                    {
                        inTheRootFiles.Add(tempFiles[0]);
                        LogWrite($"Added \"{tempFiles[0]}\" to directory \"Altri files\"");

                        File.Copy(tempFiles[0], System.IO.Path.Combine(binDir, tempFiles[0].Substring(targetPlanAddressxxxPath.Length + 1)), true);
                    }
                }
            }

            // copy all the files with the tree from the "File aggiuntivi" directory
            string fileAggiuntiviDir = StringAttachmentPath.Text;

            if (string.IsNullOrEmpty(fileAggiuntiviDir) || string.IsNullOrWhiteSpace(fileAggiuntiviDir)) // if no attachments directory was indicated, just warn me
            {
                LogWrite("No file aggiuntivo was indicated");
            }
            else // instead if the attachments directory was indicated, then copy the files
            {
                tempFiles = Directory.GetFiles(fileAggiuntiviDir);
                if (tempFiles.Count() == 0)
                {
                    LogWrite($"No file was found in the \"File aggiuntivi\" path");
                }
                else
                {               
                    LogWrite($"Found exactly {tempFiles.Count()} files aggiuntivi");

                    foreach (var file in tempFiles)
                    {
                        inTheRootFiles.Add(file);
                        LogWrite($"Added file aggiuntivo \"{file}\"");

                        File.Copy(file, System.IO.Path.Combine(binDir, file.Substring(fileAggiuntiviDir.Length + 1)), true);
                    }
                }
            }

            // create the .zip archive for the binaries (exclude the root)
            ZipFile.CreateFromDirectory(binDir, System.IO.Path.Combine(destinationPath, $"{fullName}_{binarySuffix}.{zipSuffix}"), CompressionLevel.Optimal, false);

            // delete the unzipped binary folder
            Directory.Delete(binDir, true);

        }

        private void StringApp_LostFocus(object sender, RoutedEventArgs e)
        {
            StringApp.Text = regexApp.IsMatch(StringApp.Text) ? StringApp.Text : "Not valid app name";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }

        private void StringSpec_LostFocus(object sender, RoutedEventArgs e)
        {
            StringSpec.Text = regexApp.IsMatch(StringSpec.Text) ? StringSpec.Text : "Not valid specialty name";
            readyToGenerate = false;
        }

        private void StringVer_LostFocus(object sender, RoutedEventArgs e)
        {
            Regex regexVersion;
            if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == false) // SPECIAL
            {
                regexVersion = regexVersionSpecial;
            }
            else if (IsSpecEnabled.IsChecked == true && IsObsoleteEnabled.IsChecked == true) // OBSOLETE-ETO
            {
                regexVersion = regexVersionObsolete;
            }
            else // STANDARD
            {
                regexVersion = regexVersionStandard;
            }

            StringVer.Text = regexVersion.IsMatch(StringVer.Text) ? StringVer.Text : "Not valid version number";
            readyToGenerate = false;
            GenerateBtn.IsEnabled = readyToGenerate;
        }
    }

   


}
