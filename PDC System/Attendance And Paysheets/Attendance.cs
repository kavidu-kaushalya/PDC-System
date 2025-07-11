namespace PDC_System
{
    public class Attendance
    {
        // The month and year for the attendance record
        public string? Month { get; set; }

        // The name of the employee
        public string? Employee_Name { get; set; }

        // Overtime minutes worked
        public decimal OTMin { get; set; }

        // Double overtime minutes worked
        public decimal DOTMin { get; set; }

        // Total number of working days in the month
        public int WorkingDays { get; set; }

        // Number of days the employee was absent
        public int AbsentDays { get; set; }

        // Early departure time in minutes
        public decimal Early { get; set; }

        // Late arrival time in minutes
        public decimal Late { get; set; }

        // Adjusted overtime calculated as (OT - (Late + Early)/60)
        public decimal AOT { get; set; }

        // Any loans deducted from the employee's salary
        public decimal Loans { get; set; }

        // Money collected by the employee (if any)
        public decimal CollectedMoney { get; set; }

        // Allowances given to the employee
        public decimal Allowance { get; set; }

        // Employee Trust Fund (ETF) deduction or contribution
        public decimal ETF { get; set; }

        // Number of days the employee does not receive pay due to absenteeism
        public int No_PAY { get; set; }

        // Employee's basic salary
        public decimal E_Salary { get; set; }

        // Rate of money deducted for each day of absence (LKR)
        public int absentlkr { get; set; }

        // The overtime hourly rate for the employee
        public decimal E_OT { get; set; }

        // The double overtime hourly rate for the employee
        public decimal E_DOT { get; set; }

        // Employee's job designation or role
        public string? jobr { get; set; }

        // Employee's first address line
        public string? address1 { get; set; }

        // Employee's second address line
        public string? address2 { get; set; }

        // Employee's city of residence
        public string? city { get; set; }

        // Employee's contact number
        public string? contact { get; set; }

        // Overtime worked due to early arrival
        public decimal eot { get; set; }

        // Overtime worked due to late arrival
        public decimal elate { get; set; }

        // Early departure time in hours
        public decimal eerly { get; set; }

        // Double overtime worked in hours
        public decimal edot { get; set; }

        




    }
}
