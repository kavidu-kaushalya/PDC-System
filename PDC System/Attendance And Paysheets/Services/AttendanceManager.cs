using PDC_System.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PDC_System;

namespace PDC_System.Services
{
    public class AttendanceManager
    {
        private string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers");

        public AttendanceManager()
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            EnsureJsonFile("ivms.json");
            EnsureJsonFile("employee.json");
            EnsureJsonFile("holiday.json");
            EnsureJsonFile("attendance.json");
        }

        private void EnsureJsonFile(string fileName)
        {
            string path = Path.Combine(basePath, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, "[]");
        }

        // ✅ Load all attendance (read-only, NO auto-save)
        public List<AttendanceRecord> LoadAttendance()
        {
            var data = JsonConvert.DeserializeObject<List<FingerprintData>>(File.ReadAllText(Path.Combine(basePath, "ivms.json")));
            var employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(Path.Combine(basePath, "employee.json")));
            var holidays = JsonConvert.DeserializeObject<List<Holiday>>(File.ReadAllText(Path.Combine(basePath, "holiday.json")));

            var records = new List<AttendanceRecord>();
            var grouped = data.GroupBy(x => new { x.EmployeeID, Date = x.DateTime.Date });
            var savedRecords = LoadSavedAttendanceRecords();

            foreach (var emp in employees)
            {
                DateTime startDate = emp.ValidFrom;
                DateTime endDate = emp.ValidTo;

                for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
                {
                    var savedRecord = savedRecords.FirstOrDefault(r =>
                        r.EmployeeId == emp.EmployeeId &&
                        r.Date.Date == day.Date);

                    // ✅ Manual edit එකක් තිබුනොත්, එක විතරක් use කරන්න
                    if (savedRecord != null && savedRecord.IsManualEdit)
                    {
                        records.Add(savedRecord);
                        continue;
                    }

                    // IVMS data එකෙන් calculate කරන්න
                    var grp = grouped.FirstOrDefault(g => g.Key.EmployeeID == emp.EmployeeId && g.Key.Date == day);
                    var times = grp?.OrderBy(x => x.DateTime).Select(x => x.DateTime).ToList();

                    var calculatedRecord = CalculateAttendanceRecord(emp, day, times, holidays);
                    records.Add(calculatedRecord);
                }
            }

            return records.OrderBy(r => r.Date).ThenBy(r => r.EmployeeId).ToList();
        }

        // ✅ Load with date range (read-only, NO auto-save)
        public List<AttendanceRecord> LoadAttendanceWithDateRange(DateTime startDate, DateTime endDate)
        {
            var data = JsonConvert.DeserializeObject<List<FingerprintData>>(File.ReadAllText(Path.Combine(basePath, "ivms.json")));
            var employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(Path.Combine(basePath, "employee.json")));
            var holidays = JsonConvert.DeserializeObject<List<Holiday>>(File.ReadAllText(Path.Combine(basePath, "holiday.json")));

            var records = new List<AttendanceRecord>();
            var dateRange = new List<DateTime>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                dateRange.Add(date);
            }

            var grouped = data.GroupBy(x => new { x.EmployeeID, Date = x.DateTime.Date });
            var savedRecords = LoadSavedAttendanceRecords();

            foreach (var emp in employees)
            {
                foreach (var day in dateRange)
                {
                    // Employee valid period එකෙන් එළියේ නම් skip කරන්න
                    if (day < emp.ValidFrom.Date || day > emp.ValidTo.Date)
                        continue;

                    var savedRecord = savedRecords.FirstOrDefault(r =>
                        r.EmployeeId == emp.EmployeeId &&
                        r.Date.Date == day.Date);

                    // ✅ Manual edit තිබුනොත් HIGHEST PRIORITY
                    if (savedRecord != null && savedRecord.IsManualEdit)
                    {
                        records.Add(savedRecord);
                        continue;
                    }

                    // IVMS data එකෙන් calculate කරන්න
                    var grp = grouped.FirstOrDefault(g => g.Key.EmployeeID == emp.EmployeeId && g.Key.Date == day);
                    var times = grp?.OrderBy(x => x.DateTime).Select(x => x.DateTime).ToList();

                    var calculatedRecord = CalculateAttendanceRecord(emp, day, times, holidays);
                    records.Add(calculatedRecord);
                }
            }

            // ⛔ AUTO-SAVE REMOVED - Load only, don't save
            return records.OrderBy(r => r.Date).ThenBy(r => r.EmployeeId).ToList();
        }

        // ✅ Save single attendance record (manual or calculated)
        public void SaveAttendanceRecord(AttendanceRecord record)
        {
            try
            {
                var savedRecords = LoadSavedAttendanceRecords();

                savedRecords.RemoveAll(r =>
                    r.EmployeeId == record.EmployeeId &&
                    r.Date.Date == record.Date.Date);

                savedRecords.Add(record);

                string attendancePath = Path.Combine(basePath, "attendance.json");
                var json = JsonConvert.SerializeObject(savedRecords, Formatting.Indented);
                File.WriteAllText(attendancePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving attendance record: {ex.Message}");
            }
        }

        // ✅ Save all records (use with caution - overwrites everything)
        public void SaveAllAttendanceRecords(List<AttendanceRecord> records)
        {
            try
            {
                string attendancePath = Path.Combine(basePath, "attendance.json");
                var json = JsonConvert.SerializeObject(records, Formatting.Indented);
                File.WriteAllText(attendancePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving all attendance records: {ex.Message}");
            }
        }

        private List<AttendanceRecord> LoadSavedAttendanceRecords()
        {
            try
            {
                string attendancePath = Path.Combine(basePath, "attendance.json");
                if (File.Exists(attendancePath))
                {
                    var json = File.ReadAllText(attendancePath);
                    return JsonConvert.DeserializeObject<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
                }
            }
            catch (Exception)
            {
            }

            return new List<AttendanceRecord>();
        }

        private AttendanceRecord CalculateAttendanceRecord(Employee emp, DateTime day, List<DateTime> times, List<Holiday> holidays)
        {
            string checkInStr = "-";
            string checkOutStr = "-";
            string overtime = "0h 0m";
            string doubleOtStr = "0h 0m";
            string earlyLeave = "0h 0m";
            string lateHours = "0h 0m";
            string status = "";

            bool isSunday = day.DayOfWeek == DayOfWeek.Sunday;
            bool isSaturday = day.DayOfWeek == DayOfWeek.Saturday;
            bool isWorkingDay = IsWorkingDay(emp, day);
            var holiday = holidays.FirstOrDefault(h => h.Date.Date == day);
            bool isHoliday = holiday != null;
            bool isNonWorkingDay = !isWorkingDay && !isHoliday;

            TimeSpan workStart = isSaturday ? emp.SaturdayCheckIn : emp.CheckIn;
            TimeSpan workEnd = isSaturday ? emp.SaturdayCheckOut : emp.CheckOut;

            if (isHoliday)
            {
                status = $"{holiday.Name} (Holiday)";

                if (times != null && times.Count >= 2)
                {
                    var first = times.First();
                    var last = times.Last();
                    checkInStr = first.ToString("HH:mm");
                    checkOutStr = last.ToString("HH:mm");

                    var totalWorked = last - first;
                    var doubleOt = TimeSpan.FromMinutes(totalWorked.TotalMinutes * 1);
                    doubleOtStr = $"{(int)doubleOt.TotalHours}h {doubleOt.Minutes}m";
                }
            }
            else if (isNonWorkingDay)
            {
                if (times != null && times.Count >= 2)
                {
                    var first = times.First();
                    var last = times.Last();
                    checkInStr = first.ToString("HH:mm");
                    checkOutStr = last.ToString("HH:mm");

                    var totalWorked = last - first;
                    var doubleOt = TimeSpan.FromMinutes(totalWorked.TotalMinutes * 1);
                    doubleOtStr = $"{(int)doubleOt.TotalHours}h {doubleOt.Minutes}m";

                    status = "Non-Working Day (Double OT)";
                }
                else if (times != null && times.Count == 1)
                {
                    checkInStr = times.First().ToString("HH:mm");
                    status = "Non-Working Day (Missing Out/In)";
                }
                else
                {
                    status = "Non-Working Day";
                }
            }
            else if (times == null || times.Count == 0)
            {
                status = "Absent";
            }
            else if (times.Count == 1)
            {
                checkInStr = times.First().ToString("HH:mm");
                status = "Missing Finger Print";
            }
            else
            {
                var first = times.First();
                var last = times.Last();
                checkInStr = first.ToString("HH:mm");
                checkOutStr = last.ToString("HH:mm");

                if (isSunday)
                {
                    var totalWorked = last - first;
                    var doubleOt = TimeSpan.FromMinutes(totalWorked.TotalMinutes * 1);
                    doubleOtStr = $"{(int)doubleOt.TotalHours}h {doubleOt.Minutes}m";
                    status = "Sunday (Double OT)";
                }
                else if (isSaturday)
                {
                    if (!emp.Saturday)
                    {
                        var totalWorked = last - first;
                        var doubleOt = TimeSpan.FromMinutes(totalWorked.TotalMinutes * 1);
                        doubleOtStr = $"{(int)doubleOt.TotalHours}h {doubleOt.Minutes}m";
                        status = "Saturday (Double OT)";
                    }
                    else
                    {
                        TimeSpan ot = TimeSpan.Zero;
                        if (first.TimeOfDay < workStart) ot += workStart - first.TimeOfDay;
                        if (last.TimeOfDay > workEnd)
                        {
                            var extra = last.TimeOfDay - workEnd;
                            if (extra.TotalMinutes >= 14) extra = RoundToSettingMinutes(extra);
                            ot += extra;
                        }
                        ot = RoundToSettingMinutes(ot);
                        overtime = $"{(int)ot.TotalHours}h {ot.Minutes}m";
                        status = "Saturday OK";
                    }
                }
                else
                {
                    // Normal working day
                    TimeSpan ot = TimeSpan.Zero;

                    if (first.TimeOfDay < workStart)
                        ot += workStart - first.TimeOfDay;

                    if (last.TimeOfDay > workEnd)
                    {
                        var extra = last.TimeOfDay - workEnd;
                        if (extra.TotalMinutes >= 14) extra = RoundToSettingMinutes(extra);
                        ot += extra;
                    }

                    ot = RoundToSettingMinutes(ot);
                    overtime = $"{(int)ot.TotalHours}h {ot.Minutes}m";

                    if (last.TimeOfDay < workEnd)
                    {
                        var early = workEnd - last.TimeOfDay;
                        earlyLeave = $"{(int)early.TotalHours}h {early.Minutes}m";
                    }

                    if (first.TimeOfDay > workStart)
                    {
                        var late = first.TimeOfDay - workStart;
                        lateHours = $"{(int)late.TotalHours}h {late.Minutes}m";
                    }

                    status = "OK";
                }
            }

            return new AttendanceRecord
            {
                EmployeeId = emp.EmployeeId,
                Name = emp.Name,
                Email = emp.EmployeeEmail,
                Date = day,
                CheckIn = checkInStr,
                CheckOut = checkOutStr,
                OverTime = overtime,
                DoubleOT = doubleOtStr,
                EarlyLeave = earlyLeave,
                LateHours = lateHours,
                Status = status,
                IsManualEdit = false // ✅ IVMS එකෙන් calculate කරපු එක
            };
        }

        private TimeSpan RoundToSettingMinutes(TimeSpan time)
        {
            int roundMinutes = Properties.Settings.Default.OT_RoundMinutes;

            if (roundMinutes <= 1)
                return time;

            double totalMinutes = time.TotalMinutes;
            double roundedMinutes = Math.Floor(totalMinutes / roundMinutes) * roundMinutes;

            return TimeSpan.FromMinutes(roundedMinutes);
        }

        private bool IsWorkingDay(Employee emp, DateTime day)
        {
            switch (day.DayOfWeek)
            {
                case DayOfWeek.Monday: return emp.Monday;
                case DayOfWeek.Tuesday: return emp.Tuesday;
                case DayOfWeek.Wednesday: return emp.Wednesday;
                case DayOfWeek.Thursday: return emp.Thursday;
                case DayOfWeek.Friday: return emp.Friday;
                case DayOfWeek.Saturday: return emp.Saturday;
                case DayOfWeek.Sunday: return emp.Sunday;
                default: return false;
            }
        }

        // ✅ Save manual attendance record with IsManualEdit = true
        public void SaveManualAttendanceRecord(AttendanceRecord record)
        {
            try
            {
                // ✅ Manual edit flag set කරන්න
                record.IsManualEdit = true;
                record.Status = "Manual Edit";

                var savedRecords = LoadSavedAttendanceRecords();

                savedRecords.RemoveAll(r =>
                    r.EmployeeId == record.EmployeeId &&
                    r.Date.Date == record.Date.Date);

                savedRecords.Add(record);

                string attendancePath = Path.Combine(basePath, "attendance.json");
                var json = JsonConvert.SerializeObject(savedRecords, Formatting.Indented);
                File.WriteAllText(attendancePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving manual attendance record: {ex.Message}");
            }
        }

        // ✅ Reset manual edits for date range
        public void ResetManualEdits(DateTime startDate, DateTime endDate)
        {
            try
            {
                var savedRecords = LoadSavedAttendanceRecords();

                // Manual edits remove කරන්න
                savedRecords.RemoveAll(r =>
                    r.Date.Date >= startDate.Date &&
                    r.Date.Date <= endDate.Date &&
                    r.IsManualEdit);

                string attendancePath = Path.Combine(basePath, "attendance.json");
                var json = JsonConvert.SerializeObject(savedRecords, Formatting.Indented);
                File.WriteAllText(attendancePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resetting manual edits: {ex.Message}");
            }
        }

        // ✅ Reset single manual edit
        public void ResetManualEditForRecord(AttendanceRecord record)
        {
            try
            {
                var savedRecords = LoadSavedAttendanceRecords();

                // Manual record එක remove කරන්න
                savedRecords.RemoveAll(r =>
                    r.EmployeeId == record.EmployeeId &&
                    r.Date.Date == record.Date.Date &&
                    r.IsManualEdit);

                string path = Path.Combine(basePath, "attendance.json");
                File.WriteAllText(path, JsonConvert.SerializeObject(savedRecords, Formatting.Indented));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error resetting manual edit for record: {ex.Message}");
            }
        }

        public class Holiday
        {
            public DateTime Date { get; set; }
            public string Name { get; set; }
        }
    }
}