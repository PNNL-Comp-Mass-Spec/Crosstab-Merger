# Overview

The Crosstab Merger combines crosstab files (aka PivotTables) that have a similar format,
creating a single merged crosstab file.

## Downloads

Download a .zip file with the program from:
https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/releases

### Example Data Files

Browse example input and output files at\
[https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs](https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs)

## Console Switches

The CrossTab Merger is a command line application.\
Syntax:

```
CrosstabMerger.exe
 /I:InputFilePathSpec [/O:OutputFileNameOrPath]
 [/D:OutputDirectory] [/Y]
 [/H:HeaderRowCount] [/K:KeyColumnCount] [/Preview]
 [/Conf:KeyValueParamFilePath] [/CreateParamFile] 
```

Use `/I` to specify the pattern to use to find files to merge
* The name must contain a wildcard character * so that multiple files can be found
* For example, use `Block*.csv` to match all CSV files that start with "Block
* Or, use `Block?_RelativeAbundanceRatios.csv` to match these four CSV files at https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs
  * Block1_RelativeAbundanceRatios.csv"
  * Block2_RelativeAbundanceRatios.csv"
  * Block3_RelativeAbundanceRatios.csv"
  * Block4_RelativeAbundanceRatios.csv"

The output file name is optional, but can be specified using `/O`
* For example, `/O:MergedAbundanceRatios.csv`
* If omitted, the output file will be auto-named, based on the first input file name

Use `/D` to specify the output directory

Use `/Y` to allow overwriting an existing output file

Use `/H` to specify the number of header rows
* By default, the program assumes just one header row
* For example, the PeptideQuantitation tab-delimited text files at https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/ have one header row (and one key column)
  * The RelativeAbundanceRatios CSV files at https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/ have three header rows (and two key columns)

Use `/K` to specify the number of key columns
* This is the number of columns that provide metadata about the crosstab data
  * If the crosstab has peptide quantitation values, and the data file lists protein name in the first column and peptide sequence in the second color, the file has two key columns
* As mentioned for `/H`, the PeptideQuantitation .txt files [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/) have one key column
  * The RelativeAbundanceRatios CSV files [on GitHub](https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/) have two key columns

Use `/Preview` to read the input files and see the of datasets found and rows that would be written

Use `/P` or `/Conf` to define a key/value parameter file with settings to load
* Example Key=Value parameter files
  * [MergerOptions_OneHeaderRow_OneKeyColumn.txt](https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/MergerOptions_OneHeaderRow_OneKeyColumn.txt)
  * [MergerOptions_ThreeHeaderRows_TwoKeyColumns.txt](https://github.com/PNNL-Comp-Mass-Spec/Crosstab-Merger/tree/master/Docs/MergerOptions_ThreeHeaderRows_TwoKeyColumns.txt)

## Contacts

Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) \
E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov \
Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/

## License

MS File Info Scanner is licensed under the 2-Clause BSD License; you may not use this file 
except in compliance with the License. You may obtain a copy of the License at 
https://opensource.org/licenses/BSD-2-Clause

Copyright 2020 Battelle Memorial Institute
