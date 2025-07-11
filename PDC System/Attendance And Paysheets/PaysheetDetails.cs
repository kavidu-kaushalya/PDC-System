using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDC_System.Attendance_And_Paysheets
{
    class PaysheetDetails
    {
        public decimal pamount { get; set; }
        public string? Name { get; set; }
        public string? ptotalEarnings { get; set; }
        public string? totalDeductions { get; set; }
        public string? pfilepath { get; set; }
        public DateTime timestamp { get; set; }
        public DateTime DateOfJoining { get; internal set; }
    }
}
