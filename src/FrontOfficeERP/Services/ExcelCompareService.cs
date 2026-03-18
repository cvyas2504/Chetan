using System.Diagnostics;
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

    /// <summary>
    /// High-performance cell-by-cell comparison of two entire workbooks.
    /// Identifies additions, deletions, and value changes across all sheets.
    /// Uses parallel processing for large workbooks.
    /// </summary>
    public async Task<WorkbookCompareResult> CompareWorkbooksCellByCellAsync(
        string masterFilePath, string compareFilePath)
    {
        return await Task.Run(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new WorkbookCompareResult();
            var differences = new List<CellDifference>();

            using var masterWb = new XLWorkbook(masterFilePath);
            using var compareWb = new XLWorkbook(compareFilePath);

            var masterSheetNames = masterWb.Worksheets.Select(ws => ws.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var compareSheetNames = compareWb.Worksheets.Select(ws => ws.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // All unique sheet names from both workbooks
            var allSheetNames = masterSheetNames.Union(compareSheetNames, StringComparer.OrdinalIgnoreCase).ToList();

            int totalCells = 0;
            int matched = 0;
            int additions = 0;
            int deletions = 0;
            int valueChanges = 0;

            // Use a thread-safe collection for parallel sheet processing
            var allDiffs = new System.Collections.Concurrent.ConcurrentBag<CellDifference>();
            var cellCounters = new System.Collections.Concurrent.ConcurrentBag<(int total, int match, int add, int del, int change)>();

            Parallel.ForEach(allSheetNames, sheetName =>
            {
                int localTotal = 0, localMatch = 0, localAdd = 0, localDel = 0, localChange = 0;
                var localDiffs = new List<CellDifference>();

                bool masterHasSheet = masterSheetNames.Contains(sheetName);
                bool compareHasSheet = compareSheetNames.Contains(sheetName);

                if (masterHasSheet && compareHasSheet)
                {
                    // Both workbooks have this sheet -- compare cell-by-cell
                    var masterWs = masterWb.Worksheet(sheetName);
                    var compareWs = compareWb.Worksheet(sheetName);

                    var masterRange = masterWs.RangeUsed();
                    var compareRange = compareWs.RangeUsed();

                    int masterRows = masterRange?.RowCount() ?? 0;
                    int masterCols = masterRange?.ColumnCount() ?? 0;
                    int compareRows = compareRange?.RowCount() ?? 0;
                    int compareCols = compareRange?.ColumnCount() ?? 0;

                    int maxRows = Math.Max(masterRows, compareRows);
                    int maxCols = Math.Max(masterCols, compareCols);

                    for (int r = 1; r <= maxRows; r++)
                    {
                        for (int c = 1; c <= maxCols; c++)
                        {
                            localTotal++;
                            var cellAddress = $"{GetColumnLetter(c)}{r}";

                            bool inMaster = r <= masterRows && c <= masterCols;
                            bool inCompare = r <= compareRows && c <= compareCols;

                            string masterVal = inMaster ? masterWs.Cell(r, c).GetString().Trim() : string.Empty;
                            string compareVal = inCompare ? compareWs.Cell(r, c).GetString().Trim() : string.Empty;

                            bool masterEmpty = string.IsNullOrEmpty(masterVal);
                            bool compareEmpty = string.IsNullOrEmpty(compareVal);

                            if (masterEmpty && compareEmpty)
                            {
                                localMatch++;
                                continue;
                            }

                            if (masterEmpty && !compareEmpty)
                            {
                                // Cell added in compare file
                                localAdd++;
                                localDiffs.Add(new CellDifference
                                {
                                    SheetName = sheetName,
                                    Row = r,
                                    Column = c,
                                    CellAddress = cellAddress,
                                    ChangeType = ChangeType.Addition,
                                    OldValue = string.Empty,
                                    NewValue = compareVal
                                });
                            }
                            else if (!masterEmpty && compareEmpty)
                            {
                                // Cell deleted in compare file
                                localDel++;
                                localDiffs.Add(new CellDifference
                                {
                                    SheetName = sheetName,
                                    Row = r,
                                    Column = c,
                                    CellAddress = cellAddress,
                                    ChangeType = ChangeType.Deletion,
                                    OldValue = masterVal,
                                    NewValue = string.Empty
                                });
                            }
                            else if (!string.Equals(masterVal, compareVal, StringComparison.Ordinal))
                            {
                                // Value changed
                                localChange++;
                                localDiffs.Add(new CellDifference
                                {
                                    SheetName = sheetName,
                                    Row = r,
                                    Column = c,
                                    CellAddress = cellAddress,
                                    ChangeType = ChangeType.ValueChange,
                                    OldValue = masterVal,
                                    NewValue = compareVal
                                });
                            }
                            else
                            {
                                localMatch++;
                            }
                        }
                    }
                }
                else if (masterHasSheet && !compareHasSheet)
                {
                    // Entire sheet deleted in compare
                    var masterWs = masterWb.Worksheet(sheetName);
                    var range = masterWs.RangeUsed();
                    if (range is not null)
                    {
                        for (int r = 1; r <= range.RowCount(); r++)
                        {
                            for (int c = 1; c <= range.ColumnCount(); c++)
                            {
                                var val = masterWs.Cell(r, c).GetString().Trim();
                                if (!string.IsNullOrEmpty(val))
                                {
                                    localTotal++;
                                    localDel++;
                                    localDiffs.Add(new CellDifference
                                    {
                                        SheetName = sheetName,
                                        Row = r,
                                        Column = c,
                                        CellAddress = $"{GetColumnLetter(c)}{r}",
                                        ChangeType = ChangeType.Deletion,
                                        OldValue = val,
                                        NewValue = string.Empty
                                    });
                                }
                            }
                        }
                    }
                }
                else if (!masterHasSheet && compareHasSheet)
                {
                    // Entire sheet added in compare
                    var compareWs = compareWb.Worksheet(sheetName);
                    var range = compareWs.RangeUsed();
                    if (range is not null)
                    {
                        for (int r = 1; r <= range.RowCount(); r++)
                        {
                            for (int c = 1; c <= range.ColumnCount(); c++)
                            {
                                var val = compareWs.Cell(r, c).GetString().Trim();
                                if (!string.IsNullOrEmpty(val))
                                {
                                    localTotal++;
                                    localAdd++;
                                    localDiffs.Add(new CellDifference
                                    {
                                        SheetName = sheetName,
                                        Row = r,
                                        Column = c,
                                        CellAddress = $"{GetColumnLetter(c)}{r}",
                                        ChangeType = ChangeType.Addition,
                                        OldValue = string.Empty,
                                        NewValue = val
                                    });
                                }
                            }
                        }
                    }
                }

                foreach (var d in localDiffs)
                    allDiffs.Add(d);

                cellCounters.Add((localTotal, localMatch, localAdd, localDel, localChange));
            });

            // Aggregate counters
            foreach (var (t, m, a, d, c) in cellCounters)
            {
                totalCells += t;
                matched += m;
                additions += a;
                deletions += d;
                valueChanges += c;
            }

            stopwatch.Stop();

            result.Differences = allDiffs.OrderBy(d => d.SheetName).ThenBy(d => d.Row).ThenBy(d => d.Column).ToList();
            result.TotalCellsCompared = totalCells;
            result.MatchedCells = matched;
            result.Additions = additions;
            result.Deletions = deletions;
            result.ValueChanges = valueChanges;
            result.ElapsedTime = stopwatch.Elapsed;

            return result;
        });
    }

    /// <summary>
    /// Column-mapped comparison (original logic preserved).
    /// </summary>
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

    /// <summary>
    /// Converts a 1-based column index to an Excel column letter (A, B, ..., Z, AA, AB, ...).
    /// </summary>
    private static string GetColumnLetter(int columnNumber)
    {
        string result = string.Empty;
        while (columnNumber > 0)
        {
            columnNumber--;
            result = (char)('A' + columnNumber % 26) + result;
            columnNumber /= 26;
        }
        return result;
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
