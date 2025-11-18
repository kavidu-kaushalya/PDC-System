using PDC_System.Models;
using Newtonsoft.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PDC_System.Payroll_Details
{
    /// <summary>
    /// Interaction logic for PayrollDetailsWindow.xaml
    /// </summary>
    public partial class PayrollDetailsWindow : UserControl
    {

        private string loanFile = "Savers/loan.json";
        private string deducationanFile = "Savers/Deducation.json";
        private string earningFile = "Savers/Earning.json";
        private string ETFFile = "Savers/ETF.json";
        private List<Deducation> dedecations = new List<Deducation>();
        private List<Earning> earnings = new List<Earning>();
        private List<Loan> loans = new List<Loan>();
        private List<ETF> etfs = new List<ETF>();
        public PayrollDetailsWindow()
        {
            InitializeComponent();
            LoadLoans();
        }

        private void LoadLoans()
        {
            if (File.Exists(loanFile))
            {
                string json = File.ReadAllText(loanFile);
                loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();
            }
            LoanGrid.ItemsSource = loans;

            if (File.Exists(deducationanFile))
            {
                string json = File.ReadAllText(deducationanFile);
                dedecations = JsonConvert.DeserializeObject<List<Deducation>>(json) ?? new List<Deducation>();
            }
            EDeductionGrid.ItemsSource = dedecations;

            if (File.Exists(earningFile))
            {
                string json = File.ReadAllText(earningFile);
                earnings = JsonConvert.DeserializeObject<List<Earning>>(json) ?? new List<Earning>();
                EEarningGrid.ItemsSource = earnings;
            }
            if (File.Exists(earningFile))
            {
                string json = File.ReadAllText(ETFFile);
                etfs = JsonConvert.DeserializeObject<List<ETF>>(json) ?? new List<ETF>();
                ETFGrid.ItemsSource = etfs;
            }
        }

        private void AddLoan_Click(object sender, RoutedEventArgs e)
        {
            AddLoanWindow addLoan = new AddLoanWindow();
            addLoan.LoanSaved += new Action<Loan>(AddLoan_LoanSaved);
            addLoan.ShowDialog();
        }
        private void AddErning(object sender, RoutedEventArgs e)
        {
            AddErningWindow addearining = new AddErningWindow();
            addearining.Earingsaved += new Action<Earning>(AddLoan_ErningSaved);
            addearining.ShowDialog();
        }
        private void AddDeducation(object sender, RoutedEventArgs e)
        {
            AddDeducationWinodw adddeducation = new AddDeducationWinodw();
            adddeducation.DeducationSaved += new Action<Deducation>(AddLoan_DeducationSaved);
            adddeducation.ShowDialog();
        }
        private void AddETF(object sender, RoutedEventArgs e)
        {
            AddETFWindow adddeducation = new AddETFWindow();
            adddeducation.ETFsaved += new Action<ETF>(AddLoan_ETFSaved);
            adddeducation.ShowDialog();
        }


        private void AddLoan_LoanSaved(Loan newLoan)
        {
            loans.Add(newLoan);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(loanFile));
            File.WriteAllText(loanFile, JsonConvert.SerializeObject(loans, Newtonsoft.Json.Formatting.Indented));
            LoanGrid.Items.Refresh();
        }
        private void AddLoan_ETFSaved(ETF newETF)
        {
            etfs.Add(newETF);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ETFFile));
            File.WriteAllText(ETFFile, JsonConvert.SerializeObject(etfs, Newtonsoft.Json.Formatting.Indented));
            ETFGrid.Items.Refresh();
        }
        private void AddLoan_ErningSaved(Earning newEarning)
        {
            earnings.Add(newEarning);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(earningFile));
            File.WriteAllText(earningFile, JsonConvert.SerializeObject(earnings, Newtonsoft.Json.Formatting.Indented));
            EEarningGrid.Items.Refresh();
        }
        private void AddLoan_DeducationSaved(Deducation newDeducation)
        {
            dedecations.Add(newDeducation);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(deducationanFile));
            File.WriteAllText(deducationanFile, JsonConvert.SerializeObject(dedecations, Newtonsoft.Json.Formatting.Indented));
            EDeductionGrid.Items.Refresh();
        }


        #region Paysheet

        #endregion
    }
}
