/* Title:           Main Window - Inventory Reports
 * Date:            4-5-17
 * Author:          Terry Holme
 * 
 * Description:     This will be the reporting functions of the Inventory Program */

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataValidationDLL;
using NewEmployeeDLL;
using NewEventLogDLL;
using InventoryDLL;
using NewPartNumbersDLL;
using ProjectsDLL;

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();
        InventoryClass TheInventoryClass = new InventoryClass();

        //setting up the data set
        public static VerifyLogonDataSet TheVerifyLogonDataSet = new VerifyLogonDataSet();
        public static FindPartsWarehousesDataSet TheFindPartsWarehouseDataSet = new FindPartsWarehousesDataSet();
                        
        //setting up global variables
        int gintNoOfMisses;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                gintNoOfMisses = 0;

                pbxPassword.Focus();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Main  Window // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
           
        }
        private void LogonFailed()
        {
            gintNoOfMisses++;

            if(gintNoOfMisses == 3)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "There Have Been Three Attemps to Sign Into Inventory Reports");

                TheMessagesClass.ErrorMessage("You Have Tried To Sign In Three Times\nThe Program Will Now Close");

                Application.Current.Shutdown();
            }
            else
            {
                TheMessagesClass.InformationMessage("You Have Failed The Sign In Process");
                return;
            }
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            string strValueForValidation;
            int intEmployeeID = 0;
            string strLastName;
            bool blnFatalError = false;
            int intRecordsReturned;
            string strErrorMessage = "";
            bool blnLogonPassed = true;

            TheFindPartsWarehouseDataSet = TheEmployeeClass.FindPartsWarehouses();

            //beginning data validation
            strValueForValidation = pbxPassword.Password;
            strLastName = txtLastName.Text;
            blnFatalError = TheDataValidationClass.VerifyIntegerData(strValueForValidation);
            if(blnFatalError == true)
            {
                strErrorMessage = "The Employee ID is not an Integer\n";
            }
            else
            {
                intEmployeeID = Convert.ToInt32(strValueForValidation);
            }
            if(strLastName == "")
            {
                blnFatalError = true;
                strErrorMessage += "The Last Name Was Not Entered\n";
            }
            if(blnFatalError == true)
            {
                TheMessagesClass.ErrorMessage(strErrorMessage);
                return;
            }

            //filling the data set
            TheVerifyLogonDataSet = TheEmployeeClass.VerifyLogon(intEmployeeID, strLastName);

            intRecordsReturned = TheVerifyLogonDataSet.VerifyLogon.Rows.Count;

            if(intRecordsReturned == 0)
            {
                LogonFailed();
                blnLogonPassed = false;
            }
            else
            {
                if(TheVerifyLogonDataSet.VerifyLogon[0].EmployeeGroup != "ADMIN")
                {
                    if(TheVerifyLogonDataSet.VerifyLogon[0].EmployeeGroup != "MANAGERS")
                    {
                        if(TheVerifyLogonDataSet.VerifyLogon[0].EmployeeGroup != "WAREHOUSE")
                        {
                            if(TheVerifyLogonDataSet.VerifyLogon[0].EmployeeGroup != "OFFICE")
                            {
                                LogonFailed();
                                blnLogonPassed = false;
                            }
                        }
                    }
                }
            }
            
            if(blnLogonPassed == true)
            {
                MainMenu MainMenu = new MainMenu();
                MainMenu.Show();
                Hide();
            }
        }
    }
}
