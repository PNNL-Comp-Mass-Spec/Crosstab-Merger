using System;
using System.IO;
using PRISM;

namespace CrosstabMerger
{
    /// <summary>
    /// <para>
    /// This program merges crosstab files (aka PivotTables) that have a similar format
    /// It creates a single merged crosstab file
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    /// Program started in 2020
    /// </para>
    /// <para>
    /// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    /// Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
    /// </para>
    /// </remarks>
    internal static class Program
    {
        // Ignore Spelling: crosstab, Conf, msec

        private const string PROGRAM_DATE = "2020-12-11";

        private static DateTime mLastProgressTime;

        private static int Main(string[] args)
        {
            var exeName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            var exePath = PRISM.FileProcessor.ProcessFilesOrDirectoriesBase.GetAppPath();
            var cmdLineParser = new CommandLineParser<CrosstabMergerOptions>(exeName, GetAppVersion())
            {
                ProgramInfo = "This program merges crosstab files (aka PivotTables) that have a similar format.\n" +
                              "It creates a single merged crosstab file",
                ContactInfo = "Program written by Matthew Monroe for PNNL (Richland, WA) in 2020" + Environment.NewLine +
                              "E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov" + Environment.NewLine +
                              "Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/"
            };

            cmdLineParser.UsageExamples.Add("Program syntax:" + Environment.NewLine + Path.GetFileName(exePath) + "\n" +
                                            " /I:InputFilePathSpec [/O:OutputFileNameOrPath]\n" +
                                            " [/D:OutputDirectory] [/Y]\n" +
                                            " [/H:HeaderRowCount] [/K:KeyColumnCount]\n" +
                                            " [/Conf:KeyValueParamFilePath] [/CreateParamFile]");

            cmdLineParser.UsageExamples.Add(string.Format("{0} QuantResults*.tsv /O:MergedResults /H:1 /K:2", Path.GetFileName(exePath)));

            // The default argument name for parameter files is /ParamFile or -ParamFile
            // Also allow /Conf or /P
            cmdLineParser.AddParamFileKey("Conf");
            cmdLineParser.AddParamFileKey("P");

            var result = cmdLineParser.ParseArgs(args);
            var options = result.ParsedResults;
            if (!result.Success || !options.Validate())
            {
                // Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
                System.Threading.Thread.Sleep(750);
                return -1;
            }

            mLastProgressTime = DateTime.UtcNow;

            try
            {
                var merger = new CrosstabMerger(options);

                merger.DebugEvent += MSFileScanner_DebugEvent;
                merger.ErrorEvent += MSFileScanner_ErrorEvent;
                merger.WarningEvent += MSFileScanner_WarningEvent;
                merger.StatusEvent += MSFileScanner_MessageEvent;
                merger.ProgressUpdate += MSFileScanner_ProgressUpdate;

                merger.ShowCurrentProcessingOptions();

                var success = merger.StartProcessing();

                if (!success)
                {
                    ShowErrorMessage("Error while processing");
                    System.Threading.Thread.Sleep(1500);
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error occurred in Program->Main", ex);
                return -1;
            }
        }

        private static string GetAppVersion()
        {
            return PRISM.FileProcessor.ProcessFilesOrDirectoriesBase.GetAppVersion(PROGRAM_DATE);
        }

        private static void ShowErrorMessage(string message, Exception ex = null)
        {
            ConsoleMsgUtils.ShowError(message, ex);
        }

        private static void MSFileScanner_DebugEvent(string message)
        {
            ConsoleMsgUtils.ShowDebug(message);
        }

        private static void MSFileScanner_ErrorEvent(string message, Exception ex)
        {
            ConsoleMsgUtils.ShowErrorCustom(message, ex, false);
        }

        private static void MSFileScanner_MessageEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static void MSFileScanner_ProgressUpdate(string progressMessage, float percentComplete)
        {
            if (DateTime.UtcNow.Subtract(mLastProgressTime).TotalSeconds < 5)
                return;

            Console.WriteLine();
            mLastProgressTime = DateTime.UtcNow;
            MSFileScanner_DebugEvent(percentComplete.ToString("0.0") + "%, " + progressMessage);
        }

        private static void MSFileScanner_WarningEvent(string message)
        {
            ConsoleMsgUtils.ShowWarning(message);
        }
    }
}
