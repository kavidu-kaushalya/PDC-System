using System;
using System.IO;
using System.Text.Json;

namespace PDC_System
{
    public class InvoiceNumberManager
    {
        // ✅ Save inside "Savers" folder
        private static readonly string SaversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
        private static readonly string FilePath = Path.Combine(SaversFolder, "invoice_data.json");

        public static int GetLastInvoiceNumber()
        {
            if (!Directory.Exists(SaversFolder))
                Directory.CreateDirectory(SaversFolder);

            if (!File.Exists(FilePath))
                return 0;

            try
            {
                var json = File.ReadAllText(FilePath);
                var data = JsonSerializer.Deserialize<InvoiceData>(json);
                return data?.LastInvoiceNumber ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        // 🔹 Only generate next number (no saving)
        public static int GetNextInvoiceNumber()
        {
            int lastNumber = GetLastInvoiceNumber();
            return lastNumber + 1;
        }

        // 🔹 Save the given number manually
        public static void SaveInvoiceNumber(int number)
        {
            if (!Directory.Exists(SaversFolder))
                Directory.CreateDirectory(SaversFolder);

            var data = new InvoiceData { LastInvoiceNumber = number };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        private class InvoiceData
        {
            public int LastInvoiceNumber { get; set; }
        }
    }
}
