using System.Collections.Generic;
using System.Globalization;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PDC_System
{
    public class InvoicePrinter
    {
        public string CustomerDetails { get; set; }
        public string InvoiceDetails { get; set; }
        public string TotalAmount { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();




        public void Print()
        {
            DrawingVisual visual = CreateInvoiceDrawing();
            PrintPreviewWindow preview = new PrintPreviewWindow(visual);
            preview.ShowDialog();
        }


        private DrawingVisual CreateInvoiceDrawing()
        {
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                Typeface tf = new Typeface("Consolas");

                // COMPANY HEADER
                dc.DrawText(MakeText("PRIYANTHA DIE CUTTING", tf, 14, TextAlignment.Right),
                    new Point(Mm(230), Mm(5)));

                dc.DrawText(MakeText("INVOICE", tf, 20, TextAlignment.Right),
                    new Point(Mm(230), Mm(10)));

                // INVOICE DETAILS
                string invoiceDetails = $"Invoice No: #15246\nDate: 2023/11/07\nCash";
                dc.DrawText(MakeText(invoiceDetails, tf, 10, TextAlignment.Right),
                    new Point(Mm(230), Mm(18)));

                // CUSTOMER DETAILS
                dc.DrawText(MakeText("Customer Details", tf, 12, TextAlignment.Left),
                    new Point(Mm(10), Mm(5)));

                dc.DrawText(MakeText(CustomerDetails, tf, 10, TextAlignment.Left),
                    new Point(Mm(10), Mm(12)));

                Pen linePen = new Pen(Brushes.Black, 0.5);

                // TABLE HEADERS
                dc.DrawText(MakeText("Description", tf, 10), new Point(Mm(10), Mm(35)));
                dc.DrawText(MakeText("Qty.", tf, 10, TextAlignment.Center), new Point(Mm(175), Mm(35)));
                dc.DrawText(MakeText("Unit Price", tf, 10, TextAlignment.Center), new Point(Mm(195), Mm(35)));
                dc.DrawText(MakeText("Total", tf, 10, TextAlignment.Right), new Point(Mm(230), Mm(35)));

                dc.DrawLine(linePen, new Point(Mm(10), Mm(39)), new Point(Mm(230), Mm(39)));

                double y = 42;
                foreach (var item in Items)
                {
                    // Draw Description (always)
                    dc.DrawText(
                        MakeText(item.Description ?? "", tf, item.IsTitle ? 12 : 12, TextAlignment.Left),
                        new Point(Mm(10), Mm(y))
                    );

                    if (item.IsTitle)
                    {
                        // Title row: leave numeric columns empty
                        dc.DrawText(MakeText("", tf, 10, TextAlignment.Center), new Point(Mm(175), Mm(y))); // Qty
                        dc.DrawText(MakeText("", tf, 10, TextAlignment.Center), new Point(Mm(195), Mm(y))); // Unit Price
                        dc.DrawText(MakeText("", tf, 10, TextAlignment.Right), new Point(Mm(230), Mm(y)));  // Total
                    }
                    else
                    {
                        // Normal item row: show all values
                        dc.DrawText(MakeText(item.Qty.ToString(), tf, 10, TextAlignment.Center), new Point(Mm(175), Mm(y)));
                        dc.DrawText(MakeText(item.UnitPrice.ToString("N2"), tf, 10, TextAlignment.Center), new Point(Mm(195), Mm(y)));
                        dc.DrawText(MakeText(item.Total.ToString("N2"), tf, 10, TextAlignment.Right), new Point(Mm(230), Mm(y)));
                    }

                    y += 6; // move to next row
                }



                // TOTAL
                dc.DrawText(MakeText($"Total: {decimal.Parse(TotalAmount):N2}", tf, 15, TextAlignment.Right),
                    new Point(Mm(230), Mm(120)));

                // FOOTER
                string footer =
                    "No. 1630/4, S.M Perera Mawatha, Rajagiriya.\n" +
                    "Tel: 072-2978667 / 011-2864267 / 075-3222226" +
                    "Email: priyanthadiscutting@gmail.com" +
                    "WhatsApp: 075-7729225";

                dc.DrawText(MakeText(footer, tf, 9, TextAlignment.Center),
                    new Point(Mm(128.8), Mm(128)));
            }

            return visual;
        }


        // Helper method to create formatted text (FIXED VERSION)
        private FormattedText MakeText(string text, Typeface typeface, double fontSize, TextAlignment alignment = TextAlignment.Left)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                null, // Fixed: Using null instead of NumberSubstitution
                1.0)  // Fixed: Using fixed DPI value
            {
                TextAlignment = alignment
            };
        }

        // Helper method to convert mm to pixels (1mm = 3.78 pixels approx)
        private double Mm(double mm)
        {
            return mm * 3.78;
        }


        // OLD method kept to avoid errors from old calls
        private FormattedText MakeText(string text, Typeface typeface, double size)
        {
            return MakeText(text, typeface, size, TextAlignment.Left);
        }

    }

    public class InvoiceItem
    {
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Qty * UnitPrice;

        public bool IsTitle { get; set; }
    }
}
