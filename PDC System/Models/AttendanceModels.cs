using System;

namespace PDC_System.Models
{
    public class FingerprintData
    {
        public string EmployeeID { get; set; }
        public DateTime DateTime { get; set; }
        

      
    }

  

    public class Loan
    {
        public string LoanId { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public decimal Remeining { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal MonthlyPay { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime Date { get; set; }

        public DateTime LoanDate { get; set; }
        public Loan()
        {
            LoanId = Guid.NewGuid().ToString("N").Substring(0, 8);
            Date = DateTime.Now;
        }
    }

    public class AttendanceRecord
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string OverTime { get; set; }
        public string DoubleOT { get; set; }
        public string EarlyLeave { get; set; }

        public string LateHours { get; set; } = "0h";
        public string Status { get; set; }
        public bool IsManualEdit { get; set; } = false;
        public string Email { get; set; }
    }

    public class Deducation
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DeducationDescription { get; set; }
        public decimal DeducationAmount { get; set; }
        public string Status { get; set; }
        public DateOnly DeducationDate { get; set; }

    }
    public class Earning
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EarningDescription { get; set; }
        public decimal EarningAmount { get; set; }
        public string Status { get; set; }
        public DateOnly EarningDate { get; set; }

    }


    public class EPFHistory
    {
        public string PaysheetId { get; set; }

        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal EmployeeAmount { get; set; }
        public decimal EmployerAmount { get; set; }
        public string Month { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }


    public class EPF
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal EmployeeAmount { get; set; }
        public decimal EmployerAmount { get; set; }
        public decimal Total { get; set; }
        public decimal DeducationAmount { get; internal set; }
        public object DeducationDescription { get; internal set; }
        public object DeducationDate { get; internal set; }
    }


    public class Paysheet
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Month { get; set; }
        public decimal Overtime { get; set; }
        public decimal DoubleOT { get; set; }
        public decimal EarlyLeave { get; set; }

        public decimal Loan { get; set; }

        public decimal ETF { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
    }



}
