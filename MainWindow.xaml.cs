using System.CodeDom;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace A2PatriciaGariando
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // fields
        LawFirmDBDataSetTableAdapters.AttorneysTableAdapter adpAttorneys = new LawFirmDBDataSetTableAdapters.AttorneysTableAdapter();
        LawFirmDBDataSetTableAdapters.BillWithAttTableAdapter adpBillWithAtt = new LawFirmDBDataSetTableAdapters.BillWithAttTableAdapter();

        LawFirmDBDataSetTableAdapters.FindBillByDateTableAdapter adpFindBillByDate = new LawFirmDBDataSetTableAdapters.FindBillByDateTableAdapter();
        LawFirmDBDataSetTableAdapters.FindClientByNameTableAdapter adpFindClientByName = new LawFirmDBDataSetTableAdapters.FindClientByNameTableAdapter();
        LawFirmDBDataSetTableAdapters.FindClientByIdTableAdapter adpFindByClientId = new LawFirmDBDataSetTableAdapters.FindClientByIdTableAdapter();
        LawFirmDBDataSetTableAdapters.AllBillingsTableAdapter adpAllBillings = new LawFirmDBDataSetTableAdapters.AllBillingsTableAdapter();

        // data table (I don't think I need all these tables but just in case) 
        LawFirmDBDataSet.AttorneysDataTable tblAttorneys = new LawFirmDBDataSet.AttorneysDataTable();
        LawFirmDBDataSet.BillWithAttDataTable tblBillWithAtt = new LawFirmDBDataSet.BillWithAttDataTable();

        LawFirmDBDataSet.FindBillByDateDataTable tblFindBillByDate = new LawFirmDBDataSet.FindBillByDateDataTable();
        LawFirmDBDataSet.FindClientByNameDataTable tblFindClientByName = new LawFirmDBDataSet.FindClientByNameDataTable();
        LawFirmDBDataSet.FindClientByIdDataTable tblFindClientById = new LawFirmDBDataSet.FindClientByIdDataTable();
        LawFirmDBDataSet.AllBillingsDataTable tblAllBills = new LawFirmDBDataSet.AllBillingsDataTable();

        public MainWindow()
        {
            InitializeComponent();

            // load our available dates
            sctStartDate.DisplayDateStart = new DateTime(2018, 6, 1);
            sctStartDate.DisplayDateEnd = new DateTime(2018, 6, 15);

            sctEndDate.DisplayDateStart = new DateTime(2018, 6, 1);
            sctEndDate.DisplayDateEnd = new DateTime(2018, 6, 15);

            /* DatePicker Class
             * 1. learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/datepicker?view=netframeworkdesktop-4.8
             * 2. learn.microsoft.com/en-us/dotnet/api/system.windows.controls.datepicker?view=windowsdesktop-9.0
             * 
             * The first website was an overview whereas the second website provided more insight on what the DatePicker can do. 
             */
        }

        public void LoadAllBills()
        {
            // Get() Example 
            // tblAllBills = adpAllBillings.GetAllBillings();

            grdBills.ItemsSource = adpAllBillings.GetAllBillings();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbAttorneysLastName.ItemsSource = adpAttorneys.GetAttorneys();
            cmbAttorneysLastName.DisplayMemberPath = "LastName";
            cmbAttorneysLastName.SelectedValuePath = "AttorneyID";
        }

        private void btnGetAllBills_Click(object sender, RoutedEventArgs e)
        {
            LoadAllBills();

            /* Format SQL Server Dates with FORMAT Function 
             * www.mssqltips.com/sqlservertip/2655/format-sql-server-dates-with-format-function/
             * 
             * FORMAT (getdate(), 'dd-MM-yy')
             * Found in the SQL Query for AllBillings TableAdapter 
             */
        }

        private void cmbAttorneys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // (int) did not work hence using Convert.ToInt32
            int attId = Convert.ToInt32(cmbAttorneysLastName.SelectedValue);

            tblBillWithAtt = adpBillWithAtt.GetBillingWithAttorney(attId);
            grdBills.ItemsSource = tblBillWithAtt;
        }

        private void btnClearFields_Click(object sender, RoutedEventArgs e)
        {
            cmbAttorneysLastName.SelectedIndex = -1;
            sctStartDate.Text = sctEndDate.Text = txtFindClientName.Text = "";
            txtBillID.Text = txtClientFullName.Text = txtCategoryName.Text = txtAttorneyFullName.Text = txtClientFee.Text = "";
            grdBills.ItemsSource = null;
            MessageBox.Show("All fields have been cleared!");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // FindByClientName
            grdBills.ItemsSource = adpFindClientByName.GetBillingByClientName(txtFindClientName.Text);
        }

        private void sctStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            sctEndDate.DisplayDateStart = sctStartDate.SelectedDate;
        }

        private void btnBillByDate_Click(object sender, RoutedEventArgs e)
        {
            // .xsd query is almost similar to FindClientByName

            // FindBillByDate
            grdBills.ItemsSource = adpFindBillByDate.GetBillByDate(sctStartDate.Text, sctEndDate.Text);

            /* MySQL BETWEEN Operator
             * www.w3schools.com/mysql/mysql_between.asp
             * 
             * WHERE column_name BETWEEN value1 AND value2 
             * Found in the SQL Query for FindBillByDate TableAdapter 
             */
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            tblFindClientById = adpFindByClientId.GetClientById();

            // FindByClientID
            int id = int.Parse(txtBillID.Text);
            var row = tblFindClientById.FindByBillingID(id);

            if (row != null)
            {
                // concatenate client name 
                txtClientFullName.Text = row.Client_First_Name + " " + row.Client_Last_Name;
                txtCategoryName.Text = row.Category;
                // concatenate attorney name 
                txtAttorneyFullName.Text = row.Attorney_First_Name + " " + row.Attorney_Last_Name;

                // do the math 
                double hours = double.Parse(row.Hours.ToString());
                double rate = double.Parse(row.Rate.ToString());
                double fee = hours * rate;
                txtClientFee.Text = fee.ToString("C");
            }
            else
            {
                // if id is not found, clear all fields and display error message 
                cmbAttorneysLastName.SelectedIndex = -1;
                sctStartDate.Text = sctEndDate.Text = txtFindClientName.Text = "";
                txtBillID.Text = txtClientFullName.Text = txtCategoryName.Text = txtAttorneyFullName.Text = txtClientFee.Text = "";
                grdBills.ItemsSource = null;
                MessageBox.Show("Client ID not found. Please try again.");
            }
        }
    }
}