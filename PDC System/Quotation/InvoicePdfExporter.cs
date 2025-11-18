using System;
using System.Collections.Generic;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PDC_System
{
    public static class InvoicePdfExporter
    {
        public static void SaveInvoiceAsPdf(string filePath, string customerDetails, string invoiceDetails, string totalAmount, List<InvoiceItem> items)
        {
            // 9 x 5.5 inches PDF size
            float pageWidth = 9f * 72f;    // 1 inch = 72 points
            float pageHeight = 5.5f * 72f;

            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                Document doc = new Document(new iTextSharp.text.Rectangle(pageWidth, pageHeight), 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                PdfContentByte cb = writer.DirectContent;


                /* Draw side holes like dot-matrix paper
                cb.SetGrayFill(0.85f); // light gray color
                float holeRadius = 3f;
                float holeSpacing = 20f;
                float top = pageHeight - 20f;
                float bottom = 20f;

                // Left side holes
                for (float yHole = top; yHole > bottom; yHole -= holeSpacing)
                {
                    cb.Circle(10f, yHole, holeRadius);
                    cb.Fill();
                }

                // Right side holes
                for (float yHole = top; yHole > bottom; yHole -= holeSpacing)
                {
                    cb.Circle(pageWidth - 10f, yHole, holeRadius);
                    cb.Fill();
                }
                cb.SetGrayFill(0f); // reset to black for text */


                // Use Consolas font (embedded)
                string consolasPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "consola.ttf");
                BaseFont bf = BaseFont.CreateFont(consolasPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                float y = pageHeight - 30; // start from top
                float leftMargin = 30;
                float rightMargin = pageWidth - 30;

                cb.BeginText();

                // LEFT: Customer Details Header
                cb.SetFontAndSize(bf, 11);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Customer Details", leftMargin, y, 0);

                // RIGHT: Company Name
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "PRIYANTHA DIE CUTTING", rightMargin, y, 0);
                y -= 15;

                // RIGHT: INVOICE (larger)
                cb.SetFontAndSize(bf, 16);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "INVOICE", rightMargin, y, 0);
                y -= 18;

                // LEFT: Customer details content
                cb.SetFontAndSize(bf, 9);
                string[] custLines = customerDetails.Split('\n');
                float custY = y;
                foreach (var line in custLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, line.Trim(), leftMargin, custY, 0);
                        custY -= 11;
                    }
                }

                // RIGHT: Invoice details
                string[] invLines = invoiceDetails.Split('\n');
                foreach (var line in invLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, line.Trim(), rightMargin, y, 0);
                        y -= 11;
                    }
                }

                // Move to table position (use the lower y between customer and invoice details)
                y = Math.Min(y, custY) - 20;

                // TABLE HEADER with underline
                cb.SetFontAndSize(bf, 9);
                float col1 = leftMargin;        // Description
                float col2 = pageWidth - 200;   // Qty
                float col3 = pageWidth - 130;   // Unit Price  
                float col4 = rightMargin;       // Total

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Description", col1, y, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Qty.", col2, y, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Unit Price", col3, y, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Total", col4, y, 0);

                cb.EndText();

                // Draw underline for header
                y -= 3;
                cb.MoveTo(leftMargin, y);
                cb.LineTo(rightMargin, y);
                cb.Stroke();

                y -= 12;

                // TABLE ITEMS
                cb.BeginText();
                cb.SetFontAndSize(bf, 9);

                foreach (var item in items)
                {
                    if (item.IsTitle)
                    {
                        // Title row (bold effect by slight offset)
                        cb.SetFontAndSize(bf, 10);
                        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, item.Description ?? "", col1, y, 0);
                        y -= 13;
                        cb.SetFontAndSize(bf, 9);
                    }
                    else
                    {
                        // Regular item row
                        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, item.Description ?? "", col1, y, 0);
                        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, item.Qty.ToString("N0"), col2, y, 0);
                        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, item.UnitPrice.ToString("N2"), col3, y, 0);
                        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, item.Total.ToString("N2"), col4, y, 0);
                        y -= 12;
                    }
                }

                cb.EndText();

              

                y -= 15;
                cb.BeginText();
                cb.SetFontAndSize(bf, 12);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, $"Total: {decimal.Parse(totalAmount):N2}", rightMargin, y, 0);

                // FOOTER at bottom
                y = 50; // Fixed position from bottom
                cb.SetFontAndSize(bf, 8);
                string footer = "No. 1630/4, S.M Perera Mawatha, Rajagiriya.\n" +
                                "Tel: 072-2978667 / 011-2864267 / 075-3222226 Email: priyanthadiecutting@gmail.com WhatsApp: 075-7729225";

                string[] footerLines = footer.Split('\n');
                foreach (var line in footerLines)
                {
                    cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, line.Trim(), pageWidth / 2, y, 0);
                    y -= 10;
                }

                cb.EndText();
                doc.Close();
            }
        }
    }
}