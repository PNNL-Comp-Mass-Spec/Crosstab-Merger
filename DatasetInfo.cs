using System.Collections.Generic;

namespace CrosstabMerger
{
    internal class DatasetInfo
    {
        public int ColumnNumber { get; }

        /// <summary>
        /// Header name or names
        /// </summary>
        /// <remarks>
        /// If Options.HeaderRowCount is 1, there is just one header name
        /// If Options.HeaderRowCount is 2, the first header name is the from the first row for this column, while the second name is from the second row
        /// </remarks>
        public List<string> HeaderNames { get; }

        public DatasetInfo(int columnNumber, string headerName)
        {
            ColumnNumber = columnNumber;
            HeaderNames = new List<string>
            {
                headerName
            };
        }

        public override string ToString()
        {
            if (HeaderNames.Count == 0)
                return "DatasetInfo with empty header name";

            return string.Join("; ", HeaderNames);
        }
    }
}
