using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CrosstabMerger
{
    public class CrosstabMerger : PRISM.EventNotifier
    {
        // Ignore Spelling: crosstab

        /// <summary>
        /// Processing Options
        /// </summary>
        public CrosstabMergerOptions Options { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public CrosstabMerger(CrosstabMergerOptions options)
        {
            Options = options;
        }

        private string GetCleanInputFilePath(out string searchPattern)
        {
            var lastSepChar = Options.InputFilePathSpec.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastSepChar >= 0)
            {
                searchPattern = Options.InputFilePathSpec.Substring(lastSepChar + 1);
            }
            else
            {
                searchPattern = Options.InputFilePathSpec;
            }

            return Options.InputFilePathSpec.Replace("*", string.Empty).Replace("?", string.Empty);
        }

        private string GetOutputFilePath()
        {
            try
            {
                var inputFilePathClean = GetCleanInputFilePath(out _);

                var inputFile = new FileInfo(inputFilePathClean);

                DirectoryInfo outputDirectory;

                if (string.IsNullOrWhiteSpace(Options.OutputDirectoryPath))
                {
                    outputDirectory = inputFile.Directory;
                }
                else
                {
                    outputDirectory = new DirectoryInfo(Options.OutputDirectoryPath);
                }

                if (string.IsNullOrWhiteSpace(Options.OutputFilePath))
                {
                    var outputFileName = Path.GetFileNameWithoutExtension(inputFile.Name) + "_merged" + Path.GetExtension(inputFile.Name);

                    if (outputDirectory == null)
                    {
                        // This shouldn't normally happen; use the current working directory
                        return outputFileName;
                    }

                    return Path.Combine(outputDirectory.FullName, outputFileName);
                }

                var outputFile = new FileInfo(Options.OutputFilePath);
                if (Options.OutputFilePath.Contains(Path.DirectorySeparatorChar))
                {
                    // Relative or full path
                    return outputFile.FullName;
                }

                if (outputDirectory == null)
                {
                    // This shouldn't normally happen; use the current working directory
                    return outputFile.Name;
                }

                return Path.Combine(outputDirectory.FullName, outputFile.Name);
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error constructing the output file path", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Find and merge crosstab files
        /// </summary>
        /// <returns>True if successful, false if an error</returns>
        public bool StartProcessing()
        {
            try
            {
                OnStatusEvent("Finding input files");

                var inputFilePathClean = GetCleanInputFilePath(out var searchPattern);

                var inputFile = new FileInfo(inputFilePathClean);
                DirectoryInfo inputFileDirectory;

                if (inputFile.Directory == null)
                {
                    OnWarningEvent("Could not determine the parent directory of " + Options.InputFilePathSpec);
                    inputFileDirectory = new DirectoryInfo(".");

                    OnWarningEvent("Will use the current working directory: " + inputFileDirectory.FullName);
                }
                else
                {
                    inputFileDirectory = inputFile.Directory;
                }

                var outputFilePath = GetOutputFilePath();
                var outputFile = new FileInfo(outputFilePath);

                var foundFiles = inputFileDirectory.GetFiles(searchPattern).ToList();

                // Assure that foundFiles does not contain the output file
                for (var index = 0; index < foundFiles.Count; index++)
                {
                    if (foundFiles[index].FullName.Equals(outputFile.FullName))
                    {
                        OnDebugEvent(string.Format(
                            "{0} matched {1}; removing the merged file from the input file list",
                            searchPattern, outputFile.Name));

                        foundFiles.RemoveAt(index);
                        break;
                    }
                }

                if (foundFiles.Count == 0)
                {
                    OnWarningEvent(string.Format("Did not find any files matching " + Options.InputFilePathSpec));
                    OnWarningEvent(string.Format("Searched {0} using {1} ", inputFile.Directory, searchPattern));
                    return false;
                }

                if (foundFiles.Count == 1)
                {
                    OnWarningEvent(string.Format("Found only one file matching {0}\nThere is nothing to merge", Options.InputFilePathSpec));
                    OnWarningEvent(string.Format("Searched {0} using {1} ", inputFile.Directory, searchPattern));
                    return false;
                }

                StartProcessing(foundFiles, outputFile);

                return true;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error merging files", ex);
                return false;
            }
        }

        /// <summary>
        /// Merge the specified crosstab files
        /// </summary>
        /// <returns>True if successful, false if an error</returns>
        public bool StartProcessing(List<FileInfo> filesToMerge, FileInfo outputFile)
        {
            try
            {
                if (filesToMerge.Count == 0)
                {
                    OnWarningEvent("Method StartProcessing called with an empty file list; nothing to do");
                    return false;
                }

                if (filesToMerge.Count == 1)
                {
                    OnWarningEvent("Method StartProcessing called with single file; nothing to do");
                    return false;
                }

                if (outputFile.Exists && !Options.OverwriteExistingFile)
                {
                    OnWarningEvent(string.Format("The output file already exists at {0}", outputFile.FullName));
                    OnWarningEvent("To overwrite, start the program with /Y or use OverwriteExistingFile=True in a Key=Value parameter file (or use /Y:True)");
                    return false;
                }

                Console.WriteLine();
                OnStatusEvent(string.Format("Merging {0} files", filesToMerge.Count));

                // This dictionary will track all of the loaded data using the following schema
                //                     SortedDictionary<KeyColumn1, SortedDictionary<KeyColumn2, ValuesByDataset>>
                var storedValues = new SortedDictionary<string, SortedDictionary<string, DatasetValueContainer>>();

                var configuration = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);

                // This will be incremented when we finish processing each input file
                var columnNumberAddon = 0;
                var columnDelimiter = "\t";

                var keyColumnHeaders = new List<DatasetInfo>();
                var mergedHeaders = new List<DatasetInfo>();

                foreach (var inputFile in filesToMerge)
                {
                    if (Path.GetExtension(inputFile.Name).EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        configuration.Delimiter = ",";
                    else
                        configuration.Delimiter = "\t";

                    if (columnNumberAddon == 0)
                    {
                        // This is the first file; store the delimiter that will be used by the output file
                        columnDelimiter = configuration.Delimiter;
                    }

                    OnStatusEvent("Reading data from " + PRISM.PathUtils.CompactPathString(inputFile.FullName, 110));
                    using (var reader = new StreamReader(new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    using (var csv = new CsvHelper.CsvReader(reader, configuration))
                    {
                        var rowNumber = 0;
                        var headersRead = false;
                        var duplicateKeyCount = 0;
                        var fieldNameFirstInstance = new Dictionary<string, int>();

                        csv.Read();
                        csv.ReadHeader();
                        rowNumber++;

                        var headers = new Dictionary<int, DatasetInfo>();

                        var headerColumnIndex = 0;
                        foreach (var header in csv.Parser.Context.HeaderRecord)
                        {
                            var dataset = new DatasetInfo(headerColumnIndex + 1 + columnNumberAddon, header);
                            headers.Add(headerColumnIndex, dataset);

                            if (columnNumberAddon == 0 && headerColumnIndex < Options.KeyColumnCount)
                            {
                                // This is the first file; cache the key column headers
                                keyColumnHeaders.Add(dataset);
                            }

                            headerColumnIndex++;
                        }

                        if (Options.HeaderRowCount <= 1)
                            headersRead = true;

                        var keyColumnCount = Options.KeyColumnCount;
                        var headerCount = headers.Count;

                        while (csv.Read())
                        {
                            rowNumber++;

                            if (!headersRead)
                            {
                                for (var columnIndex = 0; columnIndex < headerCount; columnIndex++)
                                {
                                    if (!csv.TryGetField<string>(columnIndex, out var fieldValue))
                                        continue;

                                    headers[columnIndex].HeaderNames.Add(fieldValue);
                                }

                                if (Options.HeaderRowCount == rowNumber)
                                    headersRead = true;

                                continue;
                            }

                            var primaryFieldName = csv.GetField<string>(0);
                            var secondaryFieldName = keyColumnCount < 2 ? string.Empty : csv.GetField<string>(1);

                            for (var columnIndex = 2; columnIndex < keyColumnCount; columnIndex++)
                            {
                                secondaryFieldName += string.Format("\t{0}", csv.GetField<string>(columnIndex));
                            }

                            var currentLineKey = keyColumnCount < 2 ? primaryFieldName : primaryFieldName + ", " + secondaryFieldName;
                            if (fieldNameFirstInstance.TryGetValue(currentLineKey, out var rowNumFirstInstance))
                            {
                                duplicateKeyCount++;
                                if (duplicateKeyCount <= 10 || duplicateKeyCount % 100 == 0)
                                {
                                    if (duplicateKeyCount == 1)
                                    {
                                        OnWarningEvent(string.Format(
                                            "File {0} has a duplicate key on line {1}; in a crosstab, no two lines should have the same key columns",
                                            inputFile.Name, rowNumber));
                                    }

                                    OnWarningEvent(string.Format(
                                        "Skipped line {0} (duplicate of line {1}): {2}",
                                        rowNumber, rowNumFirstInstance, currentLineKey));
                                }

                                continue;
                            }

                            fieldNameFirstInstance.Add(currentLineKey, rowNumber);

                            DatasetValueContainer valuesByDataset;
                            if (storedValues.TryGetValue(primaryFieldName, out var primaryField))
                            {
                                if (!primaryField.TryGetValue(secondaryFieldName, out valuesByDataset))
                                {
                                    valuesByDataset = new DatasetValueContainer();
                                    primaryField.Add(secondaryFieldName, valuesByDataset);
                                }
                            }
                            else
                            {
                                valuesByDataset = new DatasetValueContainer();
                                primaryField = new SortedDictionary<string, DatasetValueContainer>
                                {
                                    { secondaryFieldName, valuesByDataset }
                                };
                                storedValues.Add(primaryFieldName, primaryField);
                            }

                            for (var columnIndex = keyColumnCount; columnIndex < headerCount; columnIndex++)
                            {
                                if (!csv.TryGetField<string>(columnIndex, out var fieldValue))
                                    continue;

                                var currentDataset = headers[columnIndex];
                                valuesByDataset.DatasetValues.Add(currentDataset, fieldValue);
                            }
                        }

                        // Append headers to the merged header list
                        foreach (var header in from item in headers.Skip(keyColumnCount) orderby item.Key select item.Value)
                        {
                            mergedHeaders.Add(header);
                        }

                        // Increment the column number add-on
                        columnNumberAddon += Options.KeyColumnCount + headers.Count;

                        if (duplicateKeyCount > 10)
                        {
                            OnWarningEvent(string.Format(
                                "Skipped {0} duplicate lines in {1}",
                                duplicateKeyCount, inputFile.FullName));
                            Console.WriteLine();
                        }
                    }
                }

                Console.WriteLine();

                if (Options.Preview)
                {
                    Console.WriteLine("Loaded data from {0} datasets", mergedHeaders.Count);

                    var rowsToWrite = 0;
                    foreach (var item in storedValues)
                    {
                        rowsToWrite += item.Value.Count;
                    }

                    Console.WriteLine("Would write {0} rows to the output file", rowsToWrite);
                    return true;
                }

                var success = WriteMergedFile(keyColumnHeaders, mergedHeaders, storedValues, outputFile, columnDelimiter);

                if (success)
                {
                    OnStatusEvent("Processing Complete");
                    return true;
                }

                OnWarningEvent("Error processing");
                return false;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error merging files", ex);
                return false;
            }
        }

        /// <summary>
        /// Display the current processing options at the console
        /// </summary>
        public void ShowCurrentProcessingOptions()
        {
            Console.WriteLine("Processing options");
            Console.WriteLine();
            Console.WriteLine("{0,-25} {1}", "Header rows:", Options.HeaderRowCount);
            Console.WriteLine("{0,-25} {1}", "Key columns:", Options.KeyColumnCount);

            Console.WriteLine("{0,-25} {1}", "Input file spec:", Options.InputFilePathSpec);

            var outputFilePath = GetOutputFilePath();
            Console.WriteLine("{0,-25} {1}", "Output file path:", outputFilePath);

            if (Options.Preview)
            {
                Console.WriteLine("{0,-25} {1}", "Preview mode:", Options.Preview);
            }
            Console.WriteLine();
        }

        private bool WriteMergedFile(
            IReadOnlyCollection<DatasetInfo> keyColumnHeaders,
            IReadOnlyList<DatasetInfo> mergedHeaders,
            SortedDictionary<string, SortedDictionary<string, DatasetValueContainer>> storedValues,
            FileInfo outputFile,
            string delimiter)
        {
            try
            {
                var dataValues = new List<string>();

                OnStatusEvent(string.Format("Creating the merged file at {0}", PRISM.PathUtils.CompactPathString(outputFile.FullName, 110)));

                if (outputFile.Directory?.Exists == false)
                {
                    OnDebugEvent(string.Format("Creating missing directory {0}", PRISM.PathUtils.CompactPathString(outputFile.Directory.FullName, 110)));
                    outputFile.Directory.Create();
                    Console.WriteLine();
                }

                using (var writer = new StreamWriter(new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    // Write the header row(s)
                    for (var rowIndex = 0; rowIndex < Options.HeaderRowCount; rowIndex++)
                    {
                        foreach (var keyColumn in keyColumnHeaders)
                        {
                            dataValues.Add(keyColumn.HeaderNames[rowIndex]);
                        }

                        foreach (var header in mergedHeaders)
                        {
                            dataValues.Add(header.HeaderNames[rowIndex]);
                        }

                        writer.WriteLine(string.Join(delimiter, dataValues));
                        dataValues.Clear();
                    }

                    foreach (var primaryField in storedValues)
                    {
                        foreach (var secondaryField in primaryField.Value)
                        {
                            dataValues.Add(primaryField.Key);
                            if (keyColumnHeaders.Count == 2)
                            {
                                dataValues.Add(secondaryField.Key);
                            }
                            else if (keyColumnHeaders.Count > 2)
                            {
                                var secondaryFieldParts = secondaryField.Key.Split('\t');
                                dataValues.AddRange(secondaryFieldParts);
                            }

                            var datasetValues = secondaryField.Value.DatasetValues;

                            foreach (var datasetInfo in mergedHeaders)
                            {
                                if (datasetValues.TryGetValue(datasetInfo, out var value))
                                {
                                    dataValues.Add(value);
                                }
                                else
                                {
                                    dataValues.Add(string.Empty);
                                }
                            }
                            writer.WriteLine(string.Join(delimiter, dataValues));
                            dataValues.Clear();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error writing the merged file", ex);
                return false;
            }
        }
    }
}