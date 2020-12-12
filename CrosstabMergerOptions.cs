using PRISM;

namespace CrosstabMerger
{
    public class CrosstabMergerOptions
    {
        // Ignore Spelling: wildcards, wildcard

        /// <summary>
        /// Input file name and/or full path
        /// Supports * and ? as wildcards
        /// </summary>
        [Option("InputFilePathSpec", "InputFilePath", "I",
            ArgPosition = 1, Required = true, HelpShowsDefault = false,
            HelpText = "Input file name or full path; the path should contain wildcard character * or ? to match multiple files\n" +
                       "Either define this at the command line using /I or in a parameter file")]
        public string InputFilePathSpec { get; set; }

        [Option("OutputFilePath", "OutputFile", "O",
            HelpShowsDefault = false,
            HelpText = "Output file name or full path; if not provided, will be auto-defined based on the input file")]
        public string OutputFilePath { get; set; }

        [Option("OutputDirectoryPath", "OutputDirectory", "D",
            HelpShowsDefault = false,
            HelpText = "Directory where the output file should be created")]
        public string OutputDirectoryPath { get; set; }

        [Option("OverwriteExistingFile", "Overwrite", "Y",
            HelpShowsDefault = true,
            HelpText = "Overwrite existing output file")]
        public bool OverwriteExistingFile { get; set; }

        /// <summary>
        /// Number of header rows
        /// </summary>
        /// <remarks>This is typically 1, but can be more than 1 or even 0</remarks>
        [Option("HeaderRowCount", "HeaderRows", "H",
            HelpShowsDefault = true, SecondaryArg = false, Min = 1,
            HelpText = "Number of header rows")]
        public int HeaderRowCount { get; set; }

        /// <summary>
        /// Number of columns at the left that have key information
        /// </summary>
        /// <remarks>
        /// For example, if the first two columns are Protein and Peptide, KeyColumnCount should be 2
        /// The remaining columns are the data values, by dataset
        /// </remarks>
        [Option("KeyColumnCount", "KeyCols", "K",
            HelpShowsDefault = true, SecondaryArg = false, Min = 1,
            HelpText = "Number of key columns")]
        public int KeyColumnCount { get; set; }

        [Option("Preview", "PreviewMode", "V",
            HelpShowsDefault = true, SecondaryArg = true,
            HelpText = "Set to True to preview the dataset columns and number of merged result rows")]
        public bool Preview { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CrosstabMergerOptions()
        {
            HeaderRowCount = 1;
            KeyColumnCount = 1;
            OverwriteExistingFile = false;
        }

        /// <summary>
        /// Validate the options
        /// </summary>
        /// <returns>True if all options are valid</returns>
        /// <remarks>This method is called from Program.cs</remarks>
        // ReSharper disable once UnusedMember.Global
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(InputFilePathSpec))
            {
                ConsoleMsgUtils.ShowWarning("Error: Input file spec must be provided and non-empty, for example *.tsv");
                return false;
            }
            return true;
        }
    }
}