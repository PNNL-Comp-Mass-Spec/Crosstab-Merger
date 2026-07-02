using System.Collections.Generic;

namespace CrosstabMerger
{
    internal class DatasetValueContainer
    {
        // Ignore Spelling: crosstab

        public Dictionary<DatasetInfo, string> DatasetValues { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DatasetValueContainer()
        {
            DatasetValues = new Dictionary<DatasetInfo, string>();
        }
    }
}
