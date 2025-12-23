using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using PDC_System.Models;

public static class AttendancePdfGenerator
{
    public static void CreatePdf(List<AttendanceRecord> records, string outputPath)
    {

        QuestPDF.Settings.License = LicenseType.Community; // ✅ Add this line

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(25);
                page.Size(PageSizes.A4.Landscape());
                page.PageColor(Colors.White);

                page.Header().Text("PDC System - Attendance Report")
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Blue.Medium);

                page.Content().Table(table =>
                {
                    // ❇ Table Columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);  // Date
                        columns.ConstantColumn(70);  // ID
                        columns.RelativeColumn();     // Name
                        columns.ConstantColumn(70);  // In
                        columns.ConstantColumn(70);  // Out
                        columns.ConstantColumn(60);  // Late
                        columns.ConstantColumn(60);  // Early
                        columns.ConstantColumn(60);  // OT
                    });

                    // ❇ Table Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Date").Bold();
                        header.Cell().Element(CellStyle).Text("Emp ID").Bold();
                        header.Cell().Element(CellStyle).Text("Name").Bold();
                        header.Cell().Element(CellStyle).Text("In").Bold();
                        header.Cell().Element(CellStyle).Text("Out").Bold();
                        header.Cell().Element(CellStyle).Text("Late").Bold();
                        header.Cell().Element(CellStyle).Text("Early").Bold();
                        header.Cell().Element(CellStyle).Text("OT").Bold();
                    });

                    // ❇ Table Rows
                    // Replace r.InTime with r.CheckIn in the table row generation
                    foreach (var r in records)
                    {
                        table.Cell().Element(CellStyle).Text(r.Date.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(r.EmployeeId);
                        table.Cell().Element(CellStyle).Text(r.Name);
                        table.Cell().Element(CellStyle).Text(r.CheckIn); // <-- FIXED HERE
                        table.Cell().Element(CellStyle).Text(r.CheckOut);
                        table.Cell().Element(CellStyle).Text(r.EarlyLeave);
                        table.Cell().Element(CellStyle).Text(r.EarlyLeave);
                        table.Cell().Element(CellStyle).Text(r.EarlyLeave);
                    }
                });

                page.Footer()
                    .AlignRight()
                    .Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });

        document.GeneratePdf(outputPath);
    }

    // Reusable cell style
    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .PaddingVertical(4)
            .PaddingHorizontal(2);
    }
}
