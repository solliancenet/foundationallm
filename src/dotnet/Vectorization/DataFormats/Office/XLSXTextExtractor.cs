using ClosedXML.Excel;
using System.Text;

namespace FoundationaLLM.Vectorization.DataFormats.Office
{
    /// <summary>
    /// Extracts text from XLSX files.
    /// </summary>
    public class XLSXTextExtractor
    {
        private const string DefaultSheetNumberTemplate = "\n# Worksheet {number}\n";
        private const string DefaultEndOfSheetTemplate = "\n# End of worksheet {number}";
        private const string DefaultRowPrefix = "";
        private const string DefaultColumnSeparator = ", ";
        private const string DefaultRowSuffix = "";

        private readonly bool _withWorksheetNumber;
        private readonly bool _withEndOfWorksheetMarker;
        private readonly bool _withQuotes;
        private readonly string _worksheetNumberTemplate;
        private readonly string _endOfWorksheetMarkerTemplate;
        private readonly string _rowPrefix;
        private readonly string _columnSeparator;
        private readonly string _rowSuffix;

        /// <summary>
        /// Constructor for XLSXTextExtractor.
        /// </summary>
        /// <param name="withWorksheetNumber"></param>
        /// <param name="withEndOfWorksheetMarker"></param>
        /// <param name="withQuotes"></param>
        /// <param name="worksheetNumberTemplate"></param>
        /// <param name="endOfWorksheetMarkerTemplate"></param>
        /// <param name="rowPrefix"></param>
        /// <param name="columnSeparator"></param>
        /// <param name="rowSuffix"></param>
        public XLSXTextExtractor(
            bool withWorksheetNumber = true,
            bool withEndOfWorksheetMarker = false,
            bool withQuotes = true,
            string? worksheetNumberTemplate = null,
            string? endOfWorksheetMarkerTemplate = null,
            string? rowPrefix = null,
            string? columnSeparator = null,
            string? rowSuffix = null)
        {
            this._withWorksheetNumber = withWorksheetNumber;
            this._withEndOfWorksheetMarker = withEndOfWorksheetMarker;
            this._withQuotes = withQuotes;

            this._worksheetNumberTemplate = worksheetNumberTemplate ?? DefaultSheetNumberTemplate;
            this._endOfWorksheetMarkerTemplate = endOfWorksheetMarkerTemplate ?? DefaultEndOfSheetTemplate;

            this._rowPrefix = rowPrefix ?? DefaultRowPrefix;
            this._columnSeparator = columnSeparator ?? DefaultColumnSeparator;
            this._rowSuffix = rowSuffix ?? DefaultRowSuffix;
        }

        /// <summary>
        /// Extracts the text content from a PPTX document.
        /// </summary>
        /// <param name="binaryContent">The binary content of the PPTX document.</param>
        /// <returns>The text content of the PPTX document.</returns>
        public string GetText(BinaryData binaryContent)
        {
            var sb = new StringBuilder();

            using var stream = binaryContent.ToStream();
            using var workbook = new XLWorkbook(stream);

            var worksheetNumber = 0;
            foreach (var worksheet in workbook.Worksheets)
            {
                worksheetNumber++;
                if (this._withWorksheetNumber)
                {
                    sb.AppendLine(this._worksheetNumberTemplate.Replace("{number}", $"{worksheetNumber}", StringComparison.OrdinalIgnoreCase));
                }

                foreach (IXLRangeRow? row in worksheet.RangeUsed()!.RowsUsed())
                {
                    if (row == null) { continue; }

                    var cells = row.CellsUsed().ToList();

                    sb.Append(this._rowPrefix);
                    for (var i = 0; i < cells.Count; i++)
                    {
                        IXLCell? cell = cells[i];

                        if (this._withQuotes && cell is { Value.IsText: true })
                        {
                            sb.Append('"')
                                .Append(cell.Value.GetText().Replace("\"", "\"\"", StringComparison.Ordinal))
                                .Append('"');
                        }
                        else
                        {
                            sb.Append(cell.Value);
                        }

                        if (i < cells.Count - 1)
                        {
                            sb.Append(this._columnSeparator);
                        }
                    }

                    sb.AppendLine(this._rowSuffix);
                }

                if (this._withEndOfWorksheetMarker)
                {
                    sb.AppendLine(this._endOfWorksheetMarkerTemplate.Replace("{number}", $"{worksheetNumber}", StringComparison.OrdinalIgnoreCase));
                }
            }

            return sb.ToString().Trim();
        }
    }
}
