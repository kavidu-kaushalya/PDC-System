using System;
using System.Collections.Generic;
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

namespace PDC_System.Job_Card
{
    /// <summary>
    /// Interaction logic for JobCardView.xaml
    /// </summary>
    public partial class JobCardView : Window
    {
        public JobCardView(JobCard data)
        {
            InitializeComponent();

            Name.Text = $"{data.Customer_Name}";
            Date.Text = $"{data.JobCardDate}";
            Description.Text = $"{data.Description}";
            PaperSize.Text = $"{data.Paper_Size}";
            GSM.Text = $"{data.GSM}";
            Paper_Type.Text = $"{data.Paper_Type}";
            Duplex.Text = $"{data.Duplex}";
            Laminate.Text = $"{data.Laminate}";
            OrderQ.Text = $"{data.Quantity}";
            PaperQ.Text = $"{data.Printed}";
            Notes.Text = $"{data.Special_Note}";
        }
    }
}
