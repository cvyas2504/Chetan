using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class ExcelCompareViewModel : ObservableObject
{
    private readonly ExcelCompareService _compareService;
    private readonly ExportService _exportService;

    [ObservableProperty]
    private string _masterFilePath = string.Empty;

    [ObservableProperty]
    private string _compareFilePath = string.Empty;

    [ObservableProperty]
    private ExcelFileData? _masterFileData;

    [ObservableProperty]
    private ExcelFileData? _compareFileData;

    [ObservableProperty]
    private ObservableCollection<ColumnMapping> _columnMappings = new();

    [ObservableProperty]
    private ObservableCollection<ExcelCompareResult> _compareResults = new();

    [ObservableProperty]
    private MergedData? _mergedData;

    [ObservableProperty]
    private int _totalMatched;

    [ObservableProperty]
    private int _totalMismatched;

    [ObservableProperty]
    private int _totalMissing;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private bool _hasMappings;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string? _selectedMasterHeader;

    [ObservableProperty]
    private string? _selectedCompareHeader;

    [ObservableProperty]
    private bool _isReferenceColumn;

    [ObservableProperty]
    private ObservableCollection<string> _masterHeaders = new();

    [ObservableProperty]
    private ObservableCollection<string> _compareHeaders = new();

    // Cell-by-cell comparison properties
    [ObservableProperty]
    private WorkbookCompareResult? _workbookResult;

    [ObservableProperty]
    private ObservableCollection<CellDifference> _cellDifferences = new();

    [ObservableProperty]
    private bool _hasCellResults;

    [ObservableProperty]
    private int _cellAdditions;

    [ObservableProperty]
    private int _cellDeletions;

    [ObservableProperty]
    private int _cellValueChanges;

    [ObservableProperty]
    private int _cellMatched;

    [ObservableProperty]
    private string _cellCompareElapsed = string.Empty;

    [ObservableProperty]
    private string _selectedCompareMode = "Column Mapping";

    public List<string> CompareModes { get; } = new() { "Column Mapping", "Cell-by-Cell" };

    public ExcelCompareViewModel(ExcelCompareService compareService, ExportService exportService)
    {
        _compareService = compareService;
        _exportService = exportService;
    }

    [RelayCommand]
    private async Task BrowseMasterFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Master Excel File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xlsx", ".xls" } }
                })
            });

            if (result is not null)
            {
                MasterFilePath = result.FullPath;
                await LoadMasterFileAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task BrowseCompareFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Compare Excel File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xlsx", ".xls" } }
                })
            });

            if (result is not null)
            {
                CompareFilePath = result.FullPath;
                await LoadCompareFileAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadMasterFileAsync()
    {
        IsBusy = true;
        try
        {
            MasterFileData = await _compareService.LoadExcelFileAsync(MasterFilePath);
            MasterHeaders = new ObservableCollection<string>(MasterFileData.Headers);
            StatusMessage = $"Master file loaded: {MasterFileData.Rows.Count} rows, {MasterFileData.Headers.Count} columns";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load master file: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCompareFileAsync()
    {
        IsBusy = true;
        try
        {
            CompareFileData = await _compareService.LoadExcelFileAsync(CompareFilePath);
            CompareHeaders = new ObservableCollection<string>(CompareFileData.Headers);
            StatusMessage = $"Compare file loaded: {CompareFileData.Rows.Count} rows, {CompareFileData.Headers.Count} columns";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load compare file: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddColumnMapping()
    {
        if (SelectedMasterHeader is null || SelectedCompareHeader is null)
        {
            Shell.Current.DisplayAlert("Validation", "Select both master and compare columns.", "OK");
            return;
        }

        // If setting as reference, clear existing reference
        if (IsReferenceColumn)
        {
            foreach (var m in ColumnMappings)
                m.IsReferenceColumn = false;
        }

        var mapping = new ColumnMapping
        {
            MasterColumn = SelectedMasterHeader,
            CompareColumn = SelectedCompareHeader,
            IsReferenceColumn = IsReferenceColumn
        };

        ColumnMappings.Add(mapping);
        HasMappings = ColumnMappings.Count > 0;
    }

    [RelayCommand]
    private void RemoveColumnMapping(ColumnMapping mapping)
    {
        ColumnMappings.Remove(mapping);
        HasMappings = ColumnMappings.Count > 0;
    }

    [RelayCommand]
    private void ClearMappings()
    {
        ColumnMappings.Clear();
        HasMappings = false;
    }

    [RelayCommand]
    private async Task RunComparisonAsync()
    {
        if (SelectedCompareMode == "Cell-by-Cell")
        {
            await RunCellByCellComparisonAsync();
            return;
        }

        // Column Mapping mode
        if (MasterFileData is null || CompareFileData is null)
        {
            await Shell.Current.DisplayAlert("Validation", "Please load both master and compare files.", "OK");
            return;
        }

        if (!ColumnMappings.Any(m => m.IsReferenceColumn))
        {
            await Shell.Current.DisplayAlert("Validation", "Please set a reference column for matching.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var (results, merged) = await _compareService.CompareFilesAsync(
                MasterFileData, CompareFileData, ColumnMappings.ToList());

            CompareResults = new ObservableCollection<ExcelCompareResult>(results);
            MergedData = merged;

            TotalMatched = merged.TotalMatched;
            TotalMismatched = merged.TotalMismatched;
            TotalMissing = merged.TotalMissing;
            HasResults = true;
            HasCellResults = false;

            StatusMessage = $"Comparison complete: {TotalMatched} matched, {TotalMismatched} mismatched, {TotalMissing} missing";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RunCellByCellComparisonAsync()
    {
        if (string.IsNullOrEmpty(MasterFilePath) || string.IsNullOrEmpty(CompareFilePath))
        {
            await Shell.Current.DisplayAlert("Validation", "Please select both master and compare files.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _compareService.CompareWorkbooksCellByCellAsync(MasterFilePath, CompareFilePath);

            WorkbookResult = result;
            CellDifferences = new ObservableCollection<CellDifference>(result.Differences);
            CellAdditions = result.Additions;
            CellDeletions = result.Deletions;
            CellValueChanges = result.ValueChanges;
            CellMatched = result.MatchedCells;
            CellCompareElapsed = $"{result.ElapsedTime.TotalMilliseconds:F0}ms ({result.TotalCellsCompared:N0} cells)";
            HasCellResults = true;
            HasResults = false;

            StatusMessage = $"Cell comparison done in {result.ElapsedTime.TotalMilliseconds:F0}ms: " +
                            $"{result.Additions} additions, {result.Deletions} deletions, " +
                            $"{result.ValueChanges} changes, {result.MatchedCells} matched";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        if (HasCellResults && WorkbookResult is not null)
        {
            await ExportCellResultsAsync();
            return;
        }

        if (MergedData is null || CompareResults.Count == 0)
        {
            await Shell.Current.DisplayAlert("Info", "No results to export.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var filePath = await _exportService.ExportCompareResultToExcelAsync(
                MergedData, CompareResults.ToList(), "ExcelCompare");
            PrintService.OpenFile(filePath);
            await Shell.Current.DisplayAlert("Success", $"Exported to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportCellResultsAsync()
    {
        IsBusy = true;
        try
        {
            var filePath = await _exportService.ExportCellDifferencesToExcelAsync(
                WorkbookResult!, "CellCompare");
            PrintService.OpenFile(filePath);
            await Shell.Current.DisplayAlert("Success", $"Exported to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
