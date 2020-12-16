..\bin\CrosstabMerger.exe PeptideQuantitation*.txt /O:PeptideQuantitation_Merged.txt /Conf:MergerOptions_OneHeaderRow_OneKeyColumn.txt /Preview
pause

..\bin\CrosstabMerger.exe PeptideQuantitation*.txt /O:PeptideQuantitation_Merged.txt /Conf:MergerOptions_OneHeaderRow_OneKeyColumn.txt
pause

..\bin\CrosstabMerger.exe *RelativeAbundanceRatios.csv /O:RelativeAbundanceRatios_merged.csv /P:MergerOptions_ThreeHeaderRows_TwoKeyColumns.txt /Preview
pause

..\bin\CrosstabMerger.exe *RelativeAbundanceRatios.csv /O:RelativeAbundanceRatios_merged.csv /P:MergerOptions_ThreeHeaderRows_TwoKeyColumns.txt /Y
pause
