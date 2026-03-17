namespace FrontOfficeERP.Models;

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
