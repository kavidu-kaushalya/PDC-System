namespace PDC_System
{
    public class PaySheet
    {
        public string EmployeeName { get; set; }
        public decimal OTMin { get; set; }
        public decimal DOTMin { get; set; }
        public decimal Late { get; set; }
        public decimal Early { get; set; }

        public decimal Salary { get; set; }
        public decimal OT { get; set; }
        public decimal DOT { get; set; }

        public decimal Loans { get; set; }
        public decimal CollectedMoney { get; set; }
        public decimal ETF { get; set; }

        public decimal ActualOvertime => CalculateActualOvertime();
        public decimal DoubleOvertime => CalculateDoubleOvertime();
        public decimal TotalEarnings => CalculateTotalEarnings();
        public decimal TotalDeductions => CalculateTotalDeductions();
        public decimal NetSalary => CalculateNetSalary();

        // New property TotalAmount (NetSalary + extra earnings - extra deductions)
        public decimal TotalAmount => CalculateTotalAmount();

        // Constructor for initializing the PaySheet
        public PaySheet(string employeeName, decimal otMin, decimal dotMin, decimal late, decimal early,
                        decimal salary, decimal ot, decimal dot, decimal loans, decimal collectedMoney, decimal etf)
        {
            EmployeeName = employeeName;
            OTMin = otMin;
            DOTMin = dotMin;
            Late = late;
            Early = early;
            Salary = salary;
            OT = ot;
            DOT = dot;
            Loans = loans;
            CollectedMoney = collectedMoney;
            ETF = etf;
        }

        // Calculate Actual Overtime (AOT in hours * OT LKR)
        private decimal CalculateActualOvertime()
        {
            decimal AOT = OTMin - (Late + Early) / 60; // AOT = OT - (Late + Early)/60
            return AOT * OT; // Actual Overtime = AOT * OT
        }

        // Calculate Double Overtime (DOT LKR * (DOT minutes / 60))
        private decimal CalculateDoubleOvertime()
        {
            return DOT * (DOTMin / 60);
        }

        // Calculate Total Earnings (Salary + Actual Overtime + Double Overtime)
        private decimal CalculateTotalEarnings()
        {
            return Salary + ActualOvertime + DoubleOvertime;
        }

        // Calculate Total Deductions (Loans + Collected Money + ETF)
        private decimal CalculateTotalDeductions()
        {
            return Loans + CollectedMoney + ETF;
        }

        // Calculate Net Salary (Total Earnings - Total Deductions)
        private decimal CalculateNetSalary()
        {
            return TotalEarnings - TotalDeductions;
        }

        // Calculate Total Amount (Net Salary + extra earnings - extra deductions)
        private decimal CalculateTotalAmount()
        {
            return NetSalary; // Total Amount can also include extra earnings/deductions if desired
        }

        // Optional: Methods for handling extra earnings and deductions
        public decimal AddExtraEarnings(decimal extraEarnings)
        {
            return TotalEarnings + extraEarnings;
        }

        public decimal AddExtraDeductions(decimal extraDeductions)
        {
            return TotalDeductions + extraDeductions;
        }
    }
}
