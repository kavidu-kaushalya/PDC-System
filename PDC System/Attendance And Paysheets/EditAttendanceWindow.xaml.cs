using Newtonsoft.Json;
using PDC_System.Models;

using MimeKit;
using MailKit.Net.Smtp;
using PDC_System.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PDC_System
{
    public partial class EditAttendanceWindow : Window
    {
        private AttendanceRecord _record;
        private string employeeemail;

        public EditAttendanceWindow(AttendanceRecord record)
        {
            InitializeComponent();

            LoadTimeDropdowns();

            _record = record;

            TxtEmployeeId.Text = record.EmployeeId;
            TxtName.Text = record.Name;
            employeeemail = record.Email;
            TxtDate.Text = record.Date.ToString("yyyy-MM-dd");

            if (TimeSpan.TryParse(record.CheckIn, out var checkInTime))
            {
                CmbCheckIn_Hour.SelectedItem = checkInTime.Hours.ToString("00");
                CmbCheckIn_Minute.SelectedItem = checkInTime.Minutes.ToString("00");
            }

            if (TimeSpan.TryParse(record.CheckOut, out var checkOutTime))
            {
                CmbCheckOut_Hour.SelectedItem = checkOutTime.Hours.ToString("00");
                CmbCheckOut_Minute.SelectedItem = checkOutTime.Minutes.ToString("00");
            }
        }


        private void LoadTimeDropdowns()
        {
            // Hours 00–23
            for (int i = 0; i < 24; i++)
            {
                string hour = i.ToString("00");

                CmbCheckIn_Hour.Items.Add(hour);
                CmbCheckOut_Hour.Items.Add(hour);
            }

            // Minutes 00–59
            for (int i = 0; i < 60; i++)
            {
                string minute = i.ToString("00");

                CmbCheckIn_Minute.Items.Add(minute);
                CmbCheckOut_Minute.Items.Add(minute);
            }

            // Default values
            CmbCheckIn_Hour.SelectedIndex = 0;
            CmbCheckIn_Minute.SelectedIndex = 0;
            CmbCheckOut_Hour.SelectedIndex = 0;
            CmbCheckOut_Minute.SelectedIndex = 0;
        }




        // Change the method signature to async and return Task
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string employeeId = TxtEmployeeId.Text;
                string name = TxtName.Text;
                string date = TxtDate.Text;
                // Combine hour + minute fields
                string checkInStr = $"{CmbCheckIn_Hour.SelectedItem}:{CmbCheckIn_Minute.SelectedItem}";
                string checkOutStr = $"{CmbCheckOut_Hour.SelectedItem}:{CmbCheckOut_Minute.SelectedItem}";

                if (TimeSpan.TryParse(checkInStr, out TimeSpan checkIn) &&
                    TimeSpan.TryParse(checkOutStr, out TimeSpan checkOut))
                {
                    _record.CheckIn = checkIn.ToString(@"hh\:mm");
                    _record.CheckOut = checkOut.ToString(@"hh\:mm");

                    RecalculateOT(_record, checkIn, checkOut);

                    var manager = new AttendanceManager();
                    manager.SaveManualAttendanceRecord(_record);

                    // ✅ 3. NOW get FINAL calculated values
                    string ot = _record.OverTime;
                    string doubleOt = _record.DoubleOT;
                    string earlyLeave = _record.EarlyLeave;
                    string lateHours = _record.LateHours;
                    string status = _record.Status;


                    this.DialogResult = true;
                    this.Close();

                    NotificationHelper.ShowNotification(
                                        "PDC System!",
                                        $"{name} ({employeeId})\n" +
                                        $"Date: {date}\n" +
                                        $"Check-in: {checkInStr}\n" +
                                        $"Check-out: {checkOutStr}\n" +
                                        $"Edited Successfully"
                                    );

                    var mailService = new MailService();

                    string email = Properties.Settings.Default.UserEmail;
                    string recipientEmail = email;


                    var ccList = new List<string> { $"{employeeemail}" };
                    string subject = $"Edited Attendance {name}";
                    string body = $@"
<html>
<head>
<style>
  * {{
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }}
  body {{
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 40px 20px;
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
  }}
  .container {{
    background: #ffffff;
    border-radius: 20px;
    padding: 40px 35px;
    box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    max-width: 500px;
    width: 100%;
    position: relative;
    overflow: hidden;
  }}
  .container::before {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 6px;
    background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
  }}
  .header {{
    text-align: center;
    margin-bottom: 30px;
  }}
  .icon {{
    font-size: 50px;
    margin-bottom: 15px;
    animation: bounce 1s ease infinite;
  }}
  @keyframes bounce {{
    0%, 100% {{ transform: translateY(0); }}
    50% {{ transform: translateY(-10px); }}
  }}
  h2 {{
    color: #667eea;
    font-size: 26px;
    font-weight: 600;
    margin-bottom: 5px;
  }}
  .subtitle {{
    color: #999;
    font-size: 14px;
  }}
  .content {{
    margin: 25px 0;
  }}
  .info-row {{
    display: flex;
    padding: 15px 0;
    border-bottom: 1px solid #f0f0f0;
    align-items: center;
  }}
  .info-row:last-child {{
    border-bottom: none;
  }}
  .info-label {{
    font-weight: 600;
    color: #666;
    min-width: 120px;
    font-size: 14px;
  }}
  .info-value {{
    color: #333;
    font-size: 15px;
    flex: 1;
  }}
  .employee-name {{
    font-size: 18px;
    font-weight: 700;
    color: #667eea;
  }}
  .success-banner {{
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 20px;
    border-radius: 12px;
    text-align: center;
    margin-top: 30px;
    font-weight: 600;
    font-size: 16px;
    box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
  }}
  .checkmark {{
    font-size: 24px;
    margin-right: 8px;
    animation: scaleIn 0.5s ease;
  }}
  @keyframes scaleIn {{
    0% {{ transform: scale(0); }}
    50% {{ transform: scale(1.2); }}
    100% {{ transform: scale(1); }}
  }}
  .time-badge {{
    display: inline-block;
    background: #f8f9fa;
    padding: 4px 12px;
    border-radius: 20px;
    font-weight: 600;
    color: #667eea;
  }}
</style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <div class='icon'>📘</div>
      <h2>PDC System</h2>
      <div class='subtitle'>Attendance Record Updated</div>
    </div>
    
    <div class='content'>
      <div class='info-row'>
        <div class='info-label'>Employee</div>
        <div class='info-value employee-name'>{name}</div>
      </div>
      <div class='info-row'>
        <div class='info-label'>Employee ID</div>
        <div class='info-value'>{employeeId}</div>
      </div>
      <div class='info-row'>
        <div class='info-label'>Date</div>
        <div class='info-value'>{date}</div>
      </div>
      <div class='info-row'>
        <div class='info-label'>Check-in Time</div>
        <div class='info-value'><span class='time-badge'>{checkInStr}</span></div>
      </div>
      <div class='info-row'>
        <div class='info-label'>Check-out Time</div>
        <div class='info-value'><span class='time-badge'>{checkOutStr}</span></div>
      </div>
<div class='info-row'>
  <div class='info-label'>Over Time</div>
  <div class='info-value'><span class='time-badge'>{ot}</span></div>
</div>

<div class='info-row'>
  <div class='info-label'>Double OT</div>
  <div class='info-value'><span class='time-badge'>{doubleOt}</span></div>
</div>

<div class='info-row'>
  <div class='info-label'>Early Leave</div>
  <div class='info-value'><span class='time-badge'>{earlyLeave}</span></div>
</div>

<div class='info-row'>
  <div class='info-label'>Late Hours</div>
  <div class='info-value'><span class='time-badge'>{lateHours}</span></div>
</div>

<div class='info-row'>
  <div class='info-label'>Status</div>
  <div class='info-value'><strong>{status}</strong></div>
</div>


    </div>
    
    <div class='success-banner'>
      <span class='checkmark'>✓</span>
      Record Updated Successfully!
    </div>
  </div>
</body>
</html>";


                    await mailService.SendEmailAsync(recipientEmail, ccList, subject, body)
                                    .ContinueWith(task =>
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            if (task.Result)
                                                NotificationHelper.ShowNotification("PDC System!","✅ Email sent successfully!");
                                            else
                                                NotificationHelper.ShowNotification("PDC System!","❌ Failed to send email!");
                                        });
                                    }).ConfigureAwait(false);

                }
                else
                {
                    CustomMessageBox.Show("Invalid time format. Please enter valid hours and minutes.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error saving attendance record: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers (0-9)
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        // Validate hour range (0–23)
        private void TxtHour_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (int.TryParse(box.Text, out int value))
            {
                if (value < 0) value = 0;
                else if (value > 23) value = 23;

                box.Text = value.ToString("00");
            }
            else
            {
                box.Text = "00";
            }
        }

        // Validate minute range (0–59)
        private void TxtMinute_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (int.TryParse(box.Text, out int value))
            {
                if (value < 0) value = 0;
                else if (value > 59) value = 59;

                box.Text = value.ToString("00");
            }
            else
            {
                box.Text = "00";
            }
        }

  



        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RecalculateOT(AttendanceRecord record, TimeSpan checkIn, TimeSpan checkOut)
        {
            // Reset values
            record.OverTime = "0h 0m";
            record.DoubleOT = "0h 0m";
            record.EarlyLeave = "0h 0m";
            record.LateHours = "0h 0m";

            // Load holidays
            var holidays = LoadHolidays();
            var holiday = holidays.FirstOrDefault(h => h.Date.Date == record.Date.Date);
            bool isHoliday = holiday != null;
            bool isSunday = record.Date.DayOfWeek == DayOfWeek.Sunday;
            bool isSaturday = record.Date.DayOfWeek == DayOfWeek.Saturday;

            // Load employee info
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers");
            var employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(Path.Combine(basePath, "employee.json")));
            var emp = employees.FirstOrDefault(e => e.EmployeeId == record.EmployeeId);
            if (emp == null) return;

            // Determine working day flags
            bool isWorkingDay = record.Date.DayOfWeek switch
            {
                DayOfWeek.Monday => emp.Monday,
                DayOfWeek.Tuesday => emp.Tuesday,
                DayOfWeek.Wednesday => emp.Wednesday,
                DayOfWeek.Thursday => emp.Thursday,
                DayOfWeek.Friday => emp.Friday,
                DayOfWeek.Saturday => emp.Saturday,
                DayOfWeek.Sunday => emp.Sunday,
                _ => false
            };

            // Define working hours
            TimeSpan workStart = isSaturday ? emp.SaturdayCheckIn : emp.CheckIn;
            TimeSpan workEnd = isSaturday ? emp.SaturdayCheckOut : emp.CheckOut;

            if (isHoliday)
            {
                // ✅ Holiday worked → double OT (with 14min threshold)
                var totalWorked = checkOut - checkIn;
                if (totalWorked.TotalMinutes >= 14)
                {
                    totalWorked = RoundToSettingMinutes(totalWorked);
                    record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                }
                record.Status = $"{holiday.Name} (Holiday)";
            }
            else if (isSunday)
            {
                // ✅ Sunday → double OT (with 14min threshold)
                var totalWorked = checkOut - checkIn;
                if (totalWorked.TotalMinutes >= 14)
                {
                    totalWorked = RoundToSettingMinutes(totalWorked);
                    record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                }
                record.Status = "Sunday (Double OT)";
            }
            else if (isSaturday && !emp.Saturday)
            {
                // ✅ Saturday marked non-working → double OT
                var totalWorked = checkOut - checkIn;
                if (totalWorked.TotalMinutes >= 14)
                {
                    totalWorked = RoundToSettingMinutes(totalWorked);
                    record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                }
                record.Status = "Saturday (Double OT)";
            }
            else if (!isWorkingDay && !isHoliday)
            {
                // ✅ Any other non-working day → double OT
                var totalWorked = checkOut - checkIn;
                if (totalWorked.TotalMinutes >= 14)
                {
                    totalWorked = RoundToSettingMinutes(totalWorked);
                    record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                }
                record.Status = "Non-Working Day (Double OT)";
            }
            else
            {
                // ✅ Regular working day or Saturday that is working
                TimeSpan ot = TimeSpan.Zero;

                // Early check-in OT (before work start)
                if (checkIn < workStart)
                {
                    var earlyOt = workStart - checkIn;

                    int minOtMinutes = Properties.Settings.Default.OT_RoundMinutes;
                    // or OT_MinMinutes if you have a separate setting

                    if (earlyOt.TotalMinutes >= minOtMinutes)
                    {
                        ot += earlyOt;
                    }
                }



                // Late check-out OT (after work end)
                if (checkOut > workEnd)
                {
                    var lateOt = checkOut - workEnd;
                    if (lateOt.TotalMinutes >= 14) // Only count significant overtime
                    {
                        ot += lateOt;
                    }
                }

                if (ot.TotalMinutes > 0)
                {
                    ot = RoundToSettingMinutes(ot);
                    record.OverTime = $"{(int)ot.TotalHours}h {ot.Minutes}m";
                }

                // ✅ Early leave (count any early departure)
                if (checkOut < workEnd)
                {
                    var early = workEnd - checkOut;
                    if (early.TotalMinutes >= 1)
                    {
                        record.EarlyLeave = $"{(int)early.TotalHours}h {early.Minutes}m";
                    }
                }

                // ✅ Late hours with allowable minutes
                int allowedLate = Properties.Settings.Default.Late_Allow_Minutes;
                if (checkIn > workStart)
                {
                    var late = checkIn - workStart;
                    if (late.TotalMinutes <= allowedLate)
                    {
                        record.LateHours = "0h 0m"; // No late
                    }
                    else
                    {
                        var actualLate = late - TimeSpan.FromMinutes(allowedLate);
                        record.LateHours = $"{(int)actualLate.TotalHours}h {actualLate.Minutes}m";
                    }
                }

                // Status
                record.Status = isSaturday && emp.Saturday ? "Saturday OK" : "OK";
            }
        }

        private TimeSpan RoundToSettingMinutes(TimeSpan time)
        {
            int roundMinutes = Properties.Settings.Default.OT_RoundMinutes; // e.g., 15

            if (roundMinutes <= 1)
                return time;

            double totalMinutes = time.TotalMinutes;

            // Floor to nearest multiple of roundMinutes
            double roundedMinutes = Math.Floor(totalMinutes / roundMinutes) * roundMinutes;

            return TimeSpan.FromMinutes(roundedMinutes);
        }


        private List<Holiday> LoadHolidays()
        {
            try
            {
                // 🔧 Fixed folder name to match AttendanceManager
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savers");
                string holidayPath = Path.Combine(basePath, "holiday.json");

                if (File.Exists(holidayPath))
                {
                    var json = File.ReadAllText(holidayPath);
                    return JsonConvert.DeserializeObject<List<Holiday>>(json) ?? new List<Holiday>();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error loading holidays: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return new List<Holiday>();
        }
    }

    public class Holiday
    {
        internal bool IsChecked;

        public DateTime Date { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; internal set; }
    }
}