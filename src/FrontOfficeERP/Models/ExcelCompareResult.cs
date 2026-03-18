namespace FrontOfficeERP.Models;

/// <summary>
/// Represents the type of change detected during cell-by-cell comparison.
/// </summary>
public enum ChangeType
{
    None,
    Addition,
    Deletion,
    ValueChange
}

public class ExcelCompareResult
{
    public int RowIndex { get; set; }
    public string Status { get; set; } = string.Empty; // Match, Mismatch, Missing
    public string ReferenceColumn { get; set; } = string.Empty;
    public string ReferenceValue { get; set; } = string.Empty;
    public string CompareValue { get; set; } = string.Empty;
    public Dictionary<string, string> MasterRowData { get; set; } = new();
    public Dictionary<string, string> CompareRowData { get; set; } = new();
}

/// <summary>
/// Represents a single cell-level difference found during workbook comparison.
/// </summary>
public class CellDifference
{
    public string SheetName { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
    public string CellAddress { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable summary of the change.
    /// </summary>
    public string Summary => ChangeType switch
    {
        ChangeType.Addition => $"Added: '{NewValue}'",
        ChangeType.Deletion => $"Removed: '{OldValue}'",
        ChangeType.ValueChange => $"Changed: '{OldValue}' -> '{NewValue}'",
        _ => "No change"
    };
}

/// <summary>
/// Aggregated results from a cell-by-cell workbook comparison.
/// </summary>
public class WorkbookCompareResult
{
    public List<CellDifference> Differences { get; set; } = new();
    public int TotalCellsCompared { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int ValueChanges { get; set; }
    public int MatchedCells { get; set; }
    public TimeSpan ElapsedTime { get; set; }

    public bool HasDifferences => Differences.Count > 0;
}

public class ColumnMapping
{
    public string MasterColumn { get; set; } = string.Empty;
    public string CompareColumn { get; set; } = string.Empty;
    public bool IsReferenceColumn { get; set; }
}

public class ExcelFileData
{
    public string FileName { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
}

public class MergedData
{
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public int TotalMatched { get; set; }
    public int TotalMismatched { get; set; }
    public int TotalMissing { get; set; }
}
