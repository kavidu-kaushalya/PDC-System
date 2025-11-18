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
using System.Text.RegularExpressions;

namespace PDC_System
{
    public partial class EditAttendanceWindow : Window
    {
        private AttendanceRecord _record;
        private string employeeemail;

        public EditAttendanceWindow(AttendanceRecord record)
        {
            InitializeComponent();
            _record = record;

            TxtEmployeeId.Text = record.EmployeeId;
            TxtName.Text = record.Name;
            employeeemail = record.Email;
            TxtDate.Text = record.Date.ToString("yyyy-MM-dd");

            // Split CheckIn and CheckOut into hour/minute boxes
            if (TimeSpan.TryParse(record.CheckIn, out var checkInTime))
            {
                TxtCheckIn_Hour.Text = checkInTime.Hours.ToString("00");
                TxtCheckIn_Minute.Text = checkInTime.Minutes.ToString("00");
            }

            if (TimeSpan.TryParse(record.CheckOut, out var checkOutTime))
            {
                TxtCheckOut_Hour.Text = checkOutTime.Hours.ToString("00");
                TxtCheckOut_Minute.Text = checkOutTime.Minutes.ToString("00");
            }
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
                string checkInStr = $"{TxtCheckIn_Hour.Text}:{TxtCheckIn_Minute.Text}";
                string checkOutStr = $"{TxtCheckOut_Hour.Text}:{TxtCheckOut_Minute.Text}";

                if (TimeSpan.TryParse(checkInStr, out TimeSpan checkIn) &&
                    TimeSpan.TryParse(checkOutStr, out TimeSpan checkOut))
                {
                    _record.CheckIn = checkIn.ToString(@"hh\:mm");
                    _record.CheckOut = checkOut.ToString(@"hh\:mm");

                    RecalculateOT(_record, checkIn, checkOut);

                    var manager = new AttendanceManager();
                    manager.SaveManualAttendanceRecord(_record);

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
                                                MessageBox.Show("✅ Email sent successfully!", "Mail Service", MessageBoxButton.OK, MessageBoxImage.Information);
                                            else
                                                MessageBox.Show("❌ Failed to send email!", "Mail Service", MessageBoxButton.OK, MessageBoxImage.Error);
                                        });
                                    }).ConfigureAwait(false); 

                }
                else
                {
                    MessageBox.Show("Invalid time format. Please enter valid hours and minutes.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attendance record: {ex.Message}", "Error",
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

        private void TxtHour_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox targetBox = null;

                if (sender == TxtCheckIn_Hour)
                    targetBox = TxtCheckIn_Minute;
                else if (sender == TxtCheckOut_Hour)
                    targetBox = TxtCheckOut_Minute;

                if (targetBox != null)
                {
                    targetBox.Focus();
                    targetBox.SelectAll(); // ✅ highlight the minute text
                }

                e.Handled = true; // prevent the default "ding" sound
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

            bool isSaturdayWorking = emp.Saturday;
            bool isNonWorkingDay = !isWorkingDay && !isHoliday && !isSunday && (!isSaturdayWorking || isSaturday);

            // Define working hours
            TimeSpan workStart = isSaturdayWorking ? emp.SaturdayCheckIn : emp.CheckIn;
            TimeSpan workEnd = isSaturdayWorking ? emp.SaturdayCheckOut : emp.CheckOut;

            if (isHoliday)
            {
                // Holiday worked → double OT
                var totalWorked = checkOut - checkIn;
                record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                record.Status = $"{holiday.Name} (Holiday)";
            }
            else if (isSunday)
            {
                // Sunday → double OT
                var totalWorked = checkOut - checkIn;
                record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                record.Status = "Sunday (Double OT)";
            }
            else if (isSaturday && !isSaturdayWorking)
            {
                // Saturday marked non-working → double OT
                var totalWorked = checkOut - checkIn;
                record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                record.Status = "Saturday (Double OT)";
            }
            else if (isNonWorkingDay)
            {
                // Any other non-working day → double OT
                var totalWorked = checkOut - checkIn;
                record.DoubleOT = $"{(int)totalWorked.TotalHours}h {totalWorked.Minutes}m";
                record.Status = "Non-Working Day (Double OT)";
            }
            else
            {
                // Regular working day or Saturday that is working
                TimeSpan ot = TimeSpan.Zero;

                // Early check-in OT
                if (checkIn < workStart)
                    ot += workStart - checkIn;

                // Late check-out OT
                if (checkOut > workEnd)
                    ot += checkOut - workEnd;

                ot = RoundToSettingMinutes(ot);
                record.OverTime = $"{(int)ot.TotalHours}h {ot.Minutes}m";

                // Early leave
                if (checkOut < workEnd)
                {
                    var early = workEnd - checkOut;
                    record.EarlyLeave = $"{(int)early.TotalHours}h {early.Minutes}m";
                }

                // Late hours
                if (checkIn > workStart)
                {
                    var late = checkIn - workStart;
                    record.LateHours = $"{(int)late.TotalHours}h {late.Minutes}m";
                }

                // Status
                record.Status = isSaturdayWorking ? "Saturday OK" : "OK";
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
                MessageBox.Show($"Error loading holidays: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
