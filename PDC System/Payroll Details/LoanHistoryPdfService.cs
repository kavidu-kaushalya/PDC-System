using Newtonsoft.Json;
using PDC_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class LoanHistoryPdfService
{
    /// <summary>
    /// Reads the original loan amount from loan.json for the given employee.
    /// </summary>
    private static decimal GetOriginalLoanAmountFromJson(string employeeId)
    {
        try
        {
            string path = "Savers/loan.json";
            if (!File.Exists(path)) return 0;

            string json = File.ReadAllText(path);
            var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();
            var loan = loans.FirstOrDefault(l => l.EmployeeId == employeeId);
            return loan?.LoanAmount ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public static void ExportLoanHistory(
        string employeeId,
        string employeeName,
        List<LoanHistoryService.LoanHistoryEntry> history,
        string outputPath)
    {
        var latest = history.OrderByDescending(h => h.Date).First();

        decimal totalPaid = history.Sum(h => h.PaidAmount);

        decimal originalLoan = GetOriginalLoanAmountFromJson(employeeId);
        decimal remaining = Math.Max(0, originalLoan - totalPaid);

        var orderedHistory = history.OrderBy(h => h.Date).ToList();
        decimal cumulativePaid = 0;
        foreach (var entry in orderedHistory)
        {
            cumulativePaid += entry.PaidAmount;
            entry.RemainingAmount = Math.Max(0, originalLoan - cumulativePaid);
        }

        var displayHistory = orderedHistory.OrderByDescending(h => h.Date).ToList();

        var headerColor = Colors.Blue.Darken2;
        var tableHeaderBg = Colors.Blue.Darken2;
        var tableHeaderText = Colors.White;
        var altRowBg = Colors.Grey.Lighten4;
        var borderColor = Colors.Grey.Lighten1;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                // ── HEADER ──
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().BorderBottom(2).BorderColor(headerColor).PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Text("Loan History Report")
                            .FontSize(22).Bold().FontColor(headerColor);
                        row.ConstantItem(150).AlignRight().AlignMiddle()
                            .Text(DateTime.Now.ToString("yyyy-MM-dd"))
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                // ── CONTENT ──
                page.Content().PaddingVertical(15).Column(col =>
                {
                    // ── Employee Info Card ──
                    col.Item().Background(Colors.Grey.Lighten4).Border(1).BorderColor(borderColor)
                        .Padding(12).Column(infoCol =>
                        {
                            infoCol.Item().Text("Employee Information")
                                .FontSize(13).Bold().FontColor(headerColor);
                            infoCol.Item().PaddingTop(6).Row(row =>
                            {
                                row.RelativeItem().Column(left =>
                                {
                                    left.Item().Text(t =>
                                    {
                                        t.Span("Employee ID: ").Bold().FontSize(11);
                                        t.Span(employeeId).FontSize(11);
                                    });
                                    left.Item().PaddingTop(3).Text(t =>
                                    {
                                        t.Span("Employee Name: ").Bold().FontSize(11);
                                        t.Span(employeeName).FontSize(11);
                                    });
                                });
                                row.RelativeItem().AlignRight().Column(right =>
                                {
                                    right.Item().Text(t =>
                                    {
                                        t.Span("Monthly Installment: ").Bold().FontSize(11);
                                        t.Span($"{latest.MonthlyInstallment:N2}").FontSize(11);
                                    });
                                });
                            });
                        });

                    col.Item().PaddingTop(12);

                    // ── Loan Summary Card ──
                    col.Item().Row(summaryRow =>
                    {
                        // Original Loan
                        summaryRow.RelativeItem().Border(1).BorderColor(borderColor)
                            .Background(Colors.White).Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Original Loan").FontSize(10).FontColor(Colors.Grey.Darken1);
                                c.Item().AlignCenter().PaddingTop(4).Text($"{originalLoan:N2}")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Darken1);
                            });

                        summaryRow.ConstantItem(10); // spacer

                        // Total Paid
                        summaryRow.RelativeItem().Border(1).BorderColor(borderColor)
                            .Background(Colors.White).Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Total Paid").FontSize(10).FontColor(Colors.Grey.Darken1);
                                c.Item().AlignCenter().PaddingTop(4).Text($"{totalPaid:N2}")
                                    .FontSize(16).Bold().FontColor(Colors.Green.Darken2);
                            });

                        summaryRow.ConstantItem(10); // spacer

                        // Remaining
                        summaryRow.RelativeItem().Border(1).BorderColor(borderColor)
                            .Background(Colors.White).Padding(10).Column(c =>
                            {
                                c.Item().AlignCenter().Text("Remaining").FontSize(10).FontColor(Colors.Grey.Darken1);
                                c.Item().AlignCenter().PaddingTop(4).Text($"{remaining:N2}")
                                    .FontSize(16).Bold().FontColor(remaining > 0 ? Colors.Red.Darken1 : Colors.Green.Darken2);
                            });
                    });

                    col.Item().PaddingTop(10);

                    // ── Payment History Title ──
                    col.Item().PaddingBottom(6).Text("Payment History")
                        .FontSize(14).Bold().FontColor(headerColor);

                    // ── Table of History ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);   // #
                            cols.RelativeColumn(2);    // Date
                            cols.RelativeColumn(2);    // Month
                            cols.RelativeColumn(2);    // Paid
                            cols.RelativeColumn(2);    // Remaining
                            cols.RelativeColumn(1.5f); // Type
                        });

                        // TABLE HEADER
                        table.Header(header =>
                        {
                            foreach (var headerText in new[] { "#", "Date", "Month", "Paid", "Remaining", "Type" })
                            {
                                header.Cell()
                                    .Background(tableHeaderBg)
                                    .Padding(6)
                                    .AlignCenter()
                                    .Text(headerText)
                                    .Bold()
                                    .FontSize(10)
                                    .FontColor(tableHeaderText);
                            }
                        });

                        int rowIndex = 0;
                        foreach (var h in displayHistory)
                        {
                            var bgColor = rowIndex % 2 == 0 ? Colors.White : altRowBg;
                            string type = h.PaysheetId.StartsWith("PS-") ? "Auto" : "Manual";

                            // # column
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignCenter()
                                .Text($"{rowIndex + 1}").FontSize(10);

                            // Date
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignCenter()
                                .Text(h.Date.ToString("yyyy-MM-dd")).FontSize(10);

                            // Month
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignCenter()
                                .Text(h.Month).FontSize(10);

                            // Paid
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignRight()
                                .Text($"{h.PaidAmount:N2}").FontSize(10).FontColor(Colors.Green.Darken2);

                            // Remaining
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignRight()
                                .Text($"{h.RemainingAmount:N2}").FontSize(10)
                                .FontColor(h.RemainingAmount > 0 ? Colors.Red.Darken1 : Colors.Green.Darken2);

                            // Type
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(borderColor)
                                .Padding(5).AlignCenter()
                                .Text(type).FontSize(10);

                            rowIndex++;
                        }
                    });
                });

                // ── FOOTER ──
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                    t.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        document.GeneratePdf(outputPath);
    }
}