using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Services;

public class ExportService
{
    static ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> ExportDutyRosterToExcelAsync(
        List<DutyRoster> rosters,
        string title,
        string fileName)
    {
        return await Task.Run(() =>
        {
            var filePath = GetExportPath(fileName, "xlsx");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Duty Roster");

            // Title
            worksheet.Cell(1, 1).Value = title;
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers
            var headers = new[] { "Date", "Employee", "Duty Code", "Duty Name", "Shift", "Status", "Remarks" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(3, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Data rows
            int row = 4;
            foreach (var roster in rosters.OrderBy(r => r.DutyDate).ThenBy(r => r.EmployeeName))
            {
                worksheet.Cell(row, 1).Value = roster.DutyDate.ToString("dd-MMM-yyyy");
                worksheet.Cell(row, 2).Value = roster.EmployeeName;
                worksheet.Cell(row, 3).Value = roster.DutyCode;
                worksheet.Cell(row, 4).Value = roster.DutyName;
                worksheet.Cell(row, 5).Value = roster.ShiftType;
                worksheet.Cell(row, 6).Value = roster.Status;
                worksheet.Cell(row, 7).Value = roster.Remarks;

                for (int c = 1; c <= 7; c++)
                    worksheet.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            return filePath;
        });
    }

    public async Task<string> ExportDutyRosterToPdfAsync(
        List<DutyRoster> rosters,
        string title,
        string fileName)
    {
        return await Task.Run(() =>
        {
            var filePath = GetExportPath(fileName, "pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(title).Bold().FontSize(18);
                        col.Item().AlignCenter().Text($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}").FontSize(9);
                        col.Item().PaddingBottom(10);
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);  // Date
                            columns.RelativeColumn(3);  // Employee
                            columns.RelativeColumn(1);  // Code
                            columns.RelativeColumn(2);  // Duty
                            columns.RelativeColumn(1);  // Shift
                            columns.RelativeColumn(1);  // Status
                            columns.RelativeColumn(2);  // Remarks
                        });

                        // Header
                        var headerTexts = new[] { "Date", "Employee", "Code", "Duty", "Shift", "Status", "Remarks" };
                        foreach (var h in headerTexts)
                        {
                            table.Cell().Background("#1a237e").Padding(4)
                                .Text(h).FontColor("#ffffff").Bold().FontSize(9);
                        }

                        // Data
                        bool alternate = false;
                        foreach (var roster in rosters.OrderBy(r => r.DutyDate).ThenBy(r => r.EmployeeName))
                        {
                            var bg = alternate ? "#f5f5f5" : "#ffffff";
                            table.Cell().Background(bg).Padding(3).Text(roster.DutyDate.ToString("dd-MMM-yyyy")).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.EmployeeName).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.DutyCode).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.DutyName).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.ShiftType).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.Status).FontSize(9);
                            table.Cell().Background(bg).Padding(3).Text(roster.Remarks).FontSize(9);
                            alternate = !alternate;
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            return filePath;
        });
    }

    public async Task<string> ExportCompareResultToExcelAsync(
        MergedData mergedData,
        List<ExcelCompareResult> results,
        string fileName)
    {
        return await Task.Run(() =>
        {
            var filePath = GetExportPath(fileName, "xlsx");

            using var workbook = new XLWorkbook();

            // Merged Data sheet
            var mergedSheet = workbook.Worksheets.Add("Merged Data");
            for (int i = 0; i < mergedData.Headers.Count; i++)
            {
                var cell = mergedSheet.Cell(1, i + 1);
                cell.Value = mergedData.Headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var dataRow in mergedData.Rows)
            {
                for (int i = 0; i < mergedData.Headers.Count; i++)
                {
                    var header = mergedData.Headers[i];
                    mergedSheet.Cell(row, i + 1).Value = dataRow.GetValueOrDefault(header, "");
                }
                row++;
            }
            mergedSheet.Columns().AdjustToContents();

            // Comparison Results sheet
            var compareSheet = workbook.Worksheets.Add("Comparison Results");
            var compareHeaders = new[] { "Row", "Status", "Reference Column", "Reference Value", "Compare Value" };
            for (int i = 0; i < compareHeaders.Length; i++)
            {
                var cell = compareSheet.Cell(1, i + 1);
                cell.Value = compareHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
            }

            row = 2;
            foreach (var result in results)
            {
                compareSheet.Cell(row, 1).Value = result.RowIndex;
                compareSheet.Cell(row, 2).Value = result.Status;
                compareSheet.Cell(row, 3).Value = result.ReferenceColumn;
                compareSheet.Cell(row, 4).Value = result.ReferenceValue;
                compareSheet.Cell(row, 5).Value = result.CompareValue;

                var statusColor = result.Status switch
                {
                    "Match" => XLColor.LightGreen,
                    "Mismatch" => XLColor.LightSalmon,
                    "Missing" => XLColor.LightYellow,
                    _ => XLColor.White
                };

                for (int c = 1; c <= 5; c++)
                    compareSheet.Cell(row, c).Style.Fill.BackgroundColor = statusColor;

                row++;
            }
            compareSheet.Columns().AdjustToContents();

            // Summary sheet
            var summarySheet = workbook.Worksheets.Add("Summary");
            summarySheet.Cell(1, 1).Value = "Excel Comparison Summary";
            summarySheet.Cell(1, 1).Style.Font.Bold = true;
            summarySheet.Cell(1, 1).Style.Font.FontSize = 14;
            summarySheet.Cell(3, 1).Value = "Total Matched:";
            summarySheet.Cell(3, 2).Value = mergedData.TotalMatched;
            summarySheet.Cell(4, 1).Value = "Total Mismatched:";
            summarySheet.Cell(4, 2).Value = mergedData.TotalMismatched;
            summarySheet.Cell(5, 1).Value = "Total Missing:";
            summarySheet.Cell(5, 2).Value = mergedData.TotalMissing;
            summarySheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
            return filePath;
        });
    }

    public async Task<string> ExportCellDifferencesToExcelAsync(
        WorkbookCompareResult result,
        string fileName)
    {
        return await Task.Run(() =>
        {
            var filePath = GetExportPath(fileName, "xlsx");

            using var workbook = new XLWorkbook();

            // Differences sheet
            var diffSheet = workbook.Worksheets.Add("Cell Differences");
            var headers = new[] { "Sheet", "Cell", "Change Type", "Old Value", "New Value", "Summary" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = diffSheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
            }

            int row = 2;
            foreach (var diff in result.Differences)
            {
                diffSheet.Cell(row, 1).Value = diff.SheetName;
                diffSheet.Cell(row, 2).Value = diff.CellAddress;
                diffSheet.Cell(row, 3).Value = diff.ChangeType.ToString();
                diffSheet.Cell(row, 4).Value = diff.OldValue;
                diffSheet.Cell(row, 5).Value = diff.NewValue;
                diffSheet.Cell(row, 6).Value = diff.Summary;

                var bgColor = diff.ChangeType switch
                {
                    ChangeType.Addition => XLColor.LightGreen,
                    ChangeType.Deletion => XLColor.LightSalmon,
                    ChangeType.ValueChange => XLColor.LightYellow,
                    _ => XLColor.White
                };

                for (int c = 1; c <= 6; c++)
                    diffSheet.Cell(row, c).Style.Fill.BackgroundColor = bgColor;

                row++;
            }
            diffSheet.Columns().AdjustToContents();

            // Summary sheet
            var summarySheet = workbook.Worksheets.Add("Summary");
            summarySheet.Cell(1, 1).Value = "Cell-by-Cell Comparison Summary";
            summarySheet.Cell(1, 1).Style.Font.Bold = true;
            summarySheet.Cell(1, 1).Style.Font.FontSize = 14;
            summarySheet.Cell(3, 1).Value = "Total Cells Compared:";
            summarySheet.Cell(3, 2).Value = result.TotalCellsCompared;
            summarySheet.Cell(4, 1).Value = "Matched Cells:";
            summarySheet.Cell(4, 2).Value = result.MatchedCells;
            summarySheet.Cell(5, 1).Value = "Additions:";
            summarySheet.Cell(5, 2).Value = result.Additions;
            summarySheet.Cell(6, 1).Value = "Deletions:";
            summarySheet.Cell(6, 2).Value = result.Deletions;
            summarySheet.Cell(7, 1).Value = "Value Changes:";
            summarySheet.Cell(7, 2).Value = result.ValueChanges;
            summarySheet.Cell(8, 1).Value = "Elapsed Time:";
            summarySheet.Cell(8, 2).Value = $"{result.ElapsedTime.TotalMilliseconds:F0}ms";
            summarySheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
            return filePath;
        });
    }

    private static string GetExportPath(string fileName, string extension)
    {
        var exportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FrontOfficeERP",
            "Exports");

        if (!Directory.Exists(exportDir))
            Directory.CreateDirectory(exportDir);

        var safeName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
        return Path.Combine(exportDir, safeName);
    }
}
