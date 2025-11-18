using Google.Apis.PeopleService.v1.Data;
using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PDC_System
{
    public class EmployeeDailyAttendance
    {
        public static async Task CheckTodayAttendanceAsync()
        {

            // Check the setting first
            if (!Properties.Settings.Default.SendAttendanceEmails)
            {
                // Setting is false, so skip execution
                return;
            }

            string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
            string saversFolder = Path.Combine(baseFolder, "Savers");

            string attendanceFile = Path.Combine(saversFolder, "attendance.json");
            string sentLogFile = Path.Combine(saversFolder, "SentDates.txt");

            if (!File.Exists(attendanceFile))
                return;

            // Read all attendance records
            var attendances = JsonConvert.DeserializeObject<List<AttendanceRecord>>(File.ReadAllText(attendanceFile));
            if (attendances == null || !attendances.Any())
                return;

            // Load already sent records
            HashSet<string> sentRecords = new HashSet<string>();
            if (File.Exists(sentLogFile))
            {
                var lines = File.ReadAllLines(sentLogFile);
                foreach (var line in lines)
                    sentRecords.Add(line.Trim());
            }

            DateTime today = DateTime.Now.Date;
            DateTime yesterday = today.AddDays(-1);

            // Only process yesterday's attendances
            var recentAttendances = attendances
                .Where(a => a.Date.Date == yesterday)
                .ToList();

            if (!recentAttendances.Any())
                return;

            var mailService = new MailService();

            foreach (var a in recentAttendances)
            {
                string recordKey = $"{a.EmployeeId}|{a.Date:yyyy-MM-dd}".Trim();

                // Skip if already sent
                if (sentRecords.Contains(recordKey))
                    continue;

                // Prepare email
                string recipientEmail = a.Email; // Make sure AttendanceRecord has Email
                string subject = $"Daily Attendance {a.Name}";
                string body = $@"
<html>
<head>
  <style>
    .card {{
      font-family: Arial, sans-serif;
      border-radius: 10px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      padding: 20px;
      max-width: 400px;
      background-color: #ffffff;
    }}
    .header {{
      font-size: 18px;
      font-weight: bold;
      color: #1e1e9c;
      margin-bottom: 15px;
    }}
    .employee {{
      border: 1px solid #d6d9f3;
      border-radius: 8px;
      padding: 10px;
      margin-bottom: 15px;
      background-color: #f8f9ff;
    }}
    .label {{
      font-size: 12px;
      color: #888888;
    }}
    .check-in {{
      background-color: #e6f9e6;
      padding: 10px;
      border-radius: 6px;
      display: inline-block;
      margin-right: 10px;
      font-weight: bold;
    }}
    .check-out {{
      background-color: #fff4e6;
      padding: 10px;
      border-radius: 6px;
      display: inline-block;
      font-weight: bold;
    }}
    .status {{
      margin-top: 15px;
      display: inline-block;
      padding: 6px 12px;
      border-radius: 20px;
      background-color: #d4f1d4;
      font-weight: bold;
      color: #2b7a2b;
    }}
  </style>
</head>
<body>
  <div class='card'>
    <div class='header'>🕒 PDC System</div>

    <div class='employee'>
      <div>{a.Name}</div>
      <div class='label'>ID: {a.EmployeeId}</div>
    </div>

    <div class='label'>Date</div>
    <div>{a.Date:MMMM dd, yyyy}</div>

    <div style='margin-top:10px;'>
      <span class='check-in'>Check-in: {a.CheckIn}</span>
      <span class='check-out'>Check-out: {a.CheckOut}</span>
    </div>

    <div class='status'>{a.Status}</div>
  </div>
</body>
</html>";

                // Send email
                bool emailSent = await mailService.SendEmailAsync(recipientEmail, new List<string>(), subject, body);

                if (emailSent)
                {
                    // Log the sent record to prevent future duplicates
                    File.AppendAllText(sentLogFile, recordKey + Environment.NewLine);
                    sentRecords.Add(recordKey);
                }
            }
        }
    }
}
