using ClosedXML.Excel;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Services;

public class ExcelCompareService
{
    public async Task<ExcelFileData> LoadExcelFileAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var data = new ExcelFileData { FileName = Path.GetFileName(filePath) };

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();

            if (range is null)
                return data;

            // Read headers from first row
            var headerRow = range.FirstRow();
            for (int col = 1; col <= range.ColumnCount(); col++)
            {
                var header = headerRow.Cell(col).GetString().Trim();
                if (!string.IsNullOrEmpty(header))
                    data.Headers.Add(header);
            }

            // Read data rows
            for (int rowIdx = 2; rowIdx <= range.RowCount(); rowIdx++)
            {
                var row = range.Row(rowIdx);
                var rowData = new Dictionary<string, string>();
                bool hasData = false;

                for (int colIdx = 0; colIdx < data.Headers.Count; colIdx++)
                {
                    var value = row.Cell(colIdx + 1).GetString().Trim();
                    rowData[data.Headers[colIdx]] = value;
                    if (!string.IsNullOrEmpty(value))
                        hasData = true;
                }

                if (hasData)
                    data.Rows.Add(rowData);
            }

            return data;
        });
    }

    public async Task<(List<ExcelCompareResult> Results, MergedData Merged)> CompareFilesAsync(
        ExcelFileData masterFile,
        ExcelFileData compareFile,
        List<ColumnMapping> mappings)
    {
        return await Task.Run(() =>
        {
            var results = new List<ExcelCompareResult>();
            var referenceMapping = mappings.FirstOrDefault(m => m.IsReferenceColumn);

            if (referenceMapping is null)
                throw new InvalidOperationException("A reference column must be selected for matching.");

            // Build lookup from compare file by reference column
            var compareLookup = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in compareFile.Rows)
            {
                var key = row.GetValueOrDefault(referenceMapping.CompareColumn, "");
                if (!string.IsNullOrEmpty(key) && !compareLookup.ContainsKey(key))
                    compareLookup[key] = row;
            }

            int rowIndex = 1;
            foreach (var masterRow in masterFile.Rows)
            {
                var refValue = masterRow.GetValueOrDefault(referenceMapping.MasterColumn, "");

                if (compareLookup.TryGetValue(refValue, out var compareRow))
                {
                    // Check all mapped columns for match/mismatch
                    bool allMatch = true;
                    foreach (var mapping in mappings.Where(m => !m.IsReferenceColumn))
                    {
                        var masterVal = masterRow.GetValueOrDefault(mapping.MasterColumn, "");
                        var compareVal = compareRow.GetValueOrDefault(mapping.CompareColumn, "");

                        if (!string.Equals(masterVal, compareVal, StringComparison.OrdinalIgnoreCase))
                        {
                            allMatch = false;
                            results.Add(new ExcelCompareResult
                            {
                                RowIndex = rowIndex,
                                Status = "Mismatch",
                                ReferenceColumn = referenceMapping.MasterColumn,
                                ReferenceValue = refValue,
                                CompareValue = $"{mapping.MasterColumn}: '{masterVal}' vs '{compareVal}'",
                                MasterRowData = masterRow,
                                CompareRowData = compareRow
                            });
                        }
                    }

                    if (allMatch)
                    {
                        results.Add(new ExcelCompareResult
                        {
                            RowIndex = rowIndex,
                            Status = "Match",
                            ReferenceColumn = referenceMapping.MasterColumn,
                            ReferenceValue = refValue,
                            CompareValue = refValue,
                            MasterRowData = masterRow,
                            CompareRowData = compareRow
                        });
                    }
                }
                else
                {
                    results.Add(new ExcelCompareResult
                    {
                        RowIndex = rowIndex,
                        Status = "Missing",
                        ReferenceColumn = referenceMapping.MasterColumn,
                        ReferenceValue = refValue,
                        CompareValue = "(Not found in compare file)",
                        MasterRowData = masterRow,
                        CompareRowData = new Dictionary<string, string>()
                    });
                }

                rowIndex++;
            }

            // Build merged data
            var merged = BuildMergedData(masterFile, compareFile, mappings, compareLookup, results);

            return (results, merged);
        });
    }

    private MergedData BuildMergedData(
        ExcelFileData masterFile,
        ExcelFileData compareFile,
        List<ColumnMapping> mappings,
        Dictionary<string, Dictionary<string, string>> compareLookup,
        List<ExcelCompareResult> results)
    {
        var merged = new MergedData();
        var referenceMapping = mappings.First(m => m.IsReferenceColumn);

        // Build merged headers: all master headers + non-overlapping compare headers prefixed
        merged.Headers.AddRange(masterFile.Headers);
        foreach (var header in compareFile.Headers)
        {
            var mapping = mappings.FirstOrDefault(m => m.CompareColumn == header);
            if (mapping is null)
            {
                var mergedHeader = $"[Compare] {header}";
                if (!merged.Headers.Contains(mergedHeader))
                    merged.Headers.Add(mergedHeader);
            }
        }
        merged.Headers.Add("_CompareStatus");

        // Build merged rows
        foreach (var masterRow in masterFile.Rows)
        {
            var mergedRow = new Dictionary<string, string>();
            foreach (var header in masterFile.Headers)
                mergedRow[header] = masterRow.GetValueOrDefault(header, "");

            var refValue = masterRow.GetValueOrDefault(referenceMapping.MasterColumn, "");
            if (compareLookup.TryGetValue(refValue, out var compareRow))
            {
                foreach (var header in compareFile.Headers)
                {
                    var mapping = mappings.FirstOrDefault(m => m.CompareColumn == header);
                    if (mapping is null)
                    {
                        var mergedHeader = $"[Compare] {header}";
                        mergedRow[mergedHeader] = compareRow.GetValueOrDefault(header, "");
                    }
                }
                mergedRow["_CompareStatus"] = "Matched";
            }
            else
            {
                mergedRow["_CompareStatus"] = "Missing";
            }

            merged.Rows.Add(mergedRow);
        }

        merged.TotalMatched = results.Count(r => r.Status == "Match");
        merged.TotalMismatched = results.Count(r => r.Status == "Mismatch");
        merged.TotalMissing = results.Count(r => r.Status == "Missing");

        return merged;
    }
}
