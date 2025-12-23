using Newtonsoft.Json;
using System.IO;

public static class LoanHistoryService
{
    private static readonly string FilePath = "Savers/LoanHistory.json";

    public class LoanHistoryEntry
    {
        public string PaysheetId { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal OriginalLoanAmount { get; set; }
        public DateTime Date { get; set; }
        public string Month { get; set; }
    }

    public static List<LoanHistoryEntry> Load()
    {
        if (!File.Exists(FilePath)) return new List<LoanHistoryEntry>();
        return JsonConvert.DeserializeObject<List<LoanHistoryEntry>>(File.ReadAllText(FilePath))
               ?? new List<LoanHistoryEntry>();
    }

    public static void Save(List<LoanHistoryEntry> list)
    {
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(list, Formatting.Indented));
    }

    // 🔥 Save One Entry
    public static void AddEntry(LoanHistoryEntry entry)
    {
        var all = Load();
        all.Add(entry);
        Save(all);
    }

    public static LoanHistoryEntry? GetByPaysheetId(string paysheetId)
    {
        return Load().FirstOrDefault(h => h.PaysheetId == paysheetId);
    }


    // 🔥 Delete all history related to deleted paysheet
    public static void DeleteByPaysheetId(string paysheetId)
    {
        var all = Load();
        all.RemoveAll(h => h.PaysheetId == paysheetId);
        Save(all);
    }
}
