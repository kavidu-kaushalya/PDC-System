using Newtonsoft.Json;
using PDC_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

public static class LoanHistoryPdfService
{
    public static void ExportLoanHistory(
        string employeeId,
        string employeeName,
        List<LoanHistoryService.LoanHistoryEntry> history,
        string outputPath,
        BitmapSource pieChartImage)
    {
        var latest = history.OrderByDescending(h => h.Date).First();

        decimal totalPaid = history.Sum(h => h.PaidAmount);
        decimal remaining = latest.RemainingAmount;
        decimal totalLoan = latest.OriginalLoanAmount;

        int remainingMonths = (int)Math.Ceiling(remaining / latest.MonthlyInstallment);
        DateTime endDate = latest.Date.AddMonths(remainingMonths);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(25);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Header().Text($"Loan History Report - {employeeName}")
                    .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Summary Section
                    col.Item().Text($@"
Employee ID: {employeeId}
Employee Name: {employeeName}

Original Loan Amount: {totalLoan:N2}
Total Paid: {totalPaid:N2}
Remaining Amount: {remaining:N2}

Monthly Installment: {latest.MonthlyInstallment:N2}
Ends In: {remainingMonths} months ({endDate:MMMM yyyy})
").FontSize(12);

                    // Pie Chart Image
                    if (pieChartImage != null)
                    {
                        // Convert BitmapSource to PNG byte array for QuestPDF
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(pieChartImage));
                        using (var ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            col.Item()
                                .PaddingVertical(10)
                                .AlignCenter()
                                .Image(ms.ToArray());
                        }
                    }

                    // Table of History
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(90);   // Date
                            cols.ConstantColumn(80);   // Month
                            cols.RelativeColumn(1);    // Paid
                            cols.RelativeColumn(1);    // Remaining
                            cols.RelativeColumn(1);    // Type
                        });

                        // TABLE HEADER
                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Month").Bold();
                            header.Cell().Text("Paid").Bold();
                            header.Cell().Text("Remaining").Bold();
                            header.Cell().Text("Entry Type").Bold();
                        });

                        foreach (var h in history)
                        {
                            table.Cell().Text(h.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(h.Month);
                            table.Cell().Text($"{h.PaidAmount:N2}");
                            table.Cell().Text($"{h.RemainingAmount:N2}");

                            string type = h.PaysheetId.StartsWith("PS-") ? "Auto" : "Manual";
                            table.Cell().Text(type);
                        }
                    });
                });
            });
        });

        document.GeneratePdf(outputPath);
    }

    public static BitmapSource ConvertChartToImage(System.Windows.Controls.UserControl chart)
    {
        // 🔥 FIX 1 — chart is null
        if (chart == null)
            return null;

        // 🔥 FIX 2 — force layout update
        chart.UpdateLayout();

        // 🔥 FIX 3 — check size BEFORE rendering
        if (chart.ActualWidth <= 0 || chart.ActualHeight <= 0)
            return null;

        var size = new System.Windows.Size(chart.ActualWidth, chart.ActualHeight);

        chart.Measure(size);
        chart.Arrange(new System.Windows.Rect(size));

        int pxWidth = (int)size.Width;
        int pxHeight = (int)size.Height;

        // 🔥 FIX 4 — if size still invalid return null
        if (pxWidth <= 0 || pxHeight <= 0)
            return null;

        var bmp = new RenderTargetBitmap(
            pxWidth,
            pxHeight,
            96,
            96,
            System.Windows.Media.PixelFormats.Pbgra32);

        bmp.Render(chart);
        return bmp;
    }

}
