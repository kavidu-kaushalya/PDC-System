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

        private string oldLoanFile = "Savers/OldLoans.json";
        private string loanHistoryFile = "Savers/LoanHistory.json";
        private List<EPFHistory> epfHistoryList = new List<EPFHistory>();

        private string loanFile = "Savers/loan.json";
        private string deducationanFile = "Savers/Deducation.json";
        private string earningFile = "Savers/Earning.json";
        private string EPFFile = "Savers/EPF.json";
        private List<Deducation> dedecations = new List<Deducation>();
        private List<Earning> earnings = new List<Earning>();
        private List<Loan> loans = new List<Loan>();
        private List<EPF> etfs = new List<EPF>();
        public PayrollDetailsWindow()
        {
            InitializeComponent();
            LoadLoans();
            LoadOldLoans();


            if (File.Exists("Savers/EPFHistory.json"))
            {
                string json = File.ReadAllText("Savers/EPFHistory.json");
                epfHistoryList = JsonConvert.DeserializeObject<List<EPFHistory>>(json)
                                 ?? new List<EPFHistory>();

                LoadEPFGrid(epfHistoryList);
            }




        }

        private void LoadEPFGrid(List<EPFHistory> list)
        {
            var data = list.Select(h => new
            {
                h.EmployeeId,
                h.EmployeeName,
                h.Month,
                h.BasicSalary,
                EmployeeContribution = h.EmployeeAmount,
                EmployerContribution = h.EmployerAmount,
                TotalEPF = h.EmployeeAmount + h.EmployerAmount
            }).ToList();

            EPFCombinedGrid.ItemsSource = data;

            TotalEmployeeTxt.Text = list.Sum(x => x.EmployeeAmount).ToString("N2");
            TotalEmployerTxt.Text = list.Sum(x => x.EmployerAmount).ToString("N2");
            TotalEPFTxt.Text = list.Sum(x => x.EmployeeAmount + x.EmployerAmount).ToString("N2");
        }


        private void FilterEPF_Click(object sender, RoutedEventArgs e)
        {
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;
            string empId = SearchEmployeeId.Text.Trim();

            var filtered = epfHistoryList;

            // Filter by date range
            if (start != null && end != null)
            {
                filtered = filtered.Where(x =>
                {
                    DateTime monthDate = DateTime.Parse(x.Month); // Month = "2025-01"
                    return monthDate >= start && monthDate <= end;
                }).ToList();
            }

            // Filter by employee ID
            if (!string.IsNullOrEmpty(empId))
            {
                filtered = filtered.Where(x => x.EmployeeId.Contains(empId, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            LoadEPFGrid(filtered);
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            SearchEmployeeId.Clear();

            LoadEPFGrid(epfHistoryList);
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
                string json = File.ReadAllText(EPFFile);
                etfs = JsonConvert.DeserializeObject<List<EPF>>(json) ?? new List<EPF>();
                EPFGrid.ItemsSource = etfs;
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
        private void AddEPF(object sender, RoutedEventArgs e)
        {
            AddEPFWindow adddeducation = new AddEPFWindow();
            adddeducation.ETFsaved += new Action<EPF>(AddLoan_EPFSaved);
            adddeducation.ShowDialog();
        }


        private void AddLoan_LoanSaved(Loan newLoan)
        {
            loans.Add(newLoan);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(loanFile));
            File.WriteAllText(loanFile, JsonConvert.SerializeObject(loans, Newtonsoft.Json.Formatting.Indented));
            LoanGrid.Items.Refresh();
        }
        private void AddLoan_EPFSaved(EPF newEPF)
        {
            etfs.Add(newEPF);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(EPFFile));
            File.WriteAllText(EPFFile, JsonConvert.SerializeObject(etfs, Newtonsoft.Json.Formatting.Indented));
            EPFGrid.Items.Refresh();
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

        private void OpenLoanHistory(object sender, RoutedEventArgs e)
        {
            if (LoanGrid.SelectedItem is Loan ln)
            {
                // Pass both EmployeeId and Name to match the required constructor
                new LoanHistoryWindow(ln.EmployeeId, ln.Name).ShowDialog();
            }
            else
            {

                CustomMessageBox.Show("Select a loan first!");
            }
        }



        private void EndLoan_Click(object sender, RoutedEventArgs e)
        {
            if (LoanGrid.SelectedItem is not Loan selectedLoan)
            {
                CustomMessageBox.Show("Select a loan to end.");
                return;
            }

            // ✔ Allow ending only if status = Finished
            if (selectedLoan.Status != "Finished")
            {
                CustomMessageBox.Show("You can only end this loan after the status is set to 'Finished'.");
                return;
            }

            if (CustomMessageBox.Show(
                "Loan has no remaining amount. Do you want to finish this loan?",
                "Confirm Finish",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            // 🔥 CHANGE 2: Set Status to Finished
            selectedLoan.Status = "Finished";

            // 1. Load old loans
            List<Loan> oldLoans = new List<Loan>();
            if (File.Exists(oldLoanFile))
            {
                string jsonOld = File.ReadAllText(oldLoanFile);
                oldLoans = JsonConvert.DeserializeObject<List<Loan>>(jsonOld) ?? new List<Loan>();
            }

            // 2. Move loan to old loans
            oldLoans.Add(selectedLoan);
            File.WriteAllText(oldLoanFile, JsonConvert.SerializeObject(oldLoans, Formatting.Indented));

            // 3. Remove from active loan.json
            loans.Remove(selectedLoan);
            File.WriteAllText(loanFile, JsonConvert.SerializeObject(loans, Formatting.Indented));

            // 4. Remove loan history of this employee
            if (File.Exists(loanHistoryFile))
            {
                string jsonHistory = File.ReadAllText(loanHistoryFile);
                var historyList = JsonConvert.DeserializeObject<List<LoanHistoryService.LoanHistoryEntry>>(jsonHistory)
                                  ?? new List<LoanHistoryService.LoanHistoryEntry>();

                // 🔥 CHANGE 3: Remove only this employee's loan history
                historyList = historyList.Where(h => h.EmployeeId != selectedLoan.EmployeeId).ToList();

                File.WriteAllText(loanHistoryFile, JsonConvert.SerializeObject(historyList, Formatting.Indented));
            }

            LoanGrid.Items.Refresh();
            CustomMessageBox.Show("Loan successfully marked as FINISHED.");
        }

        private void DeleteEPF_Click(object sender, RoutedEventArgs e)
        {
            if (EPFGrid.SelectedItem is not EPF selectedEPF)
            {
                CustomMessageBox.Show("Select an EPF record to delete!");
                return;
            }

            if (CustomMessageBox.Show(
                "Are you sure you want to delete this EPF record?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            // Remove from list
            etfs.Remove(selectedEPF);

            // Save updated file
            File.WriteAllText(EPFFile, JsonConvert.SerializeObject(etfs, Formatting.Indented));

            // Refresh grid 
            EPFGrid.Items.Refresh();

            CustomMessageBox.Show("EPF record deleted successfully!");
        }




        private void LoadOldLoans()
        {
            if (File.Exists(oldLoanFile))
            {
                string json = File.ReadAllText(oldLoanFile);
                var oldLoans = JsonConvert.DeserializeObject<List<Loan>>(json);
                OldLoanGrid.ItemsSource = oldLoans;
            }
        }

        #region Paysheet

        #endregion
    }
}
