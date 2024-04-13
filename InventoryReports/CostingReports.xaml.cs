/* Title:           Costing Reports
 * Date:            8-4-17
 * Author:          Terry Holmes */

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
using NewEmployeeDLL;
using NewEventLogDLL;
using InventoryCostingDLL;
using DataValidationDLL;
using CSVFileDLL;
using Microsoft.Win32;

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for CostingReports.xaml
    /// </summary>
    public partial class CostingReports : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        InventoryCostingClass TheInventoryCostingClass = new InventoryCostingClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();

        //setting up the data
        FindReceiveJHMaterialOverDateRangeDataSet TheFindReceiveJHMaterialOverADateRange = new FindReceiveJHMaterialOverDateRangeDataSet();
        FindIssuedJHMaterialOverDateRangeDataSet TheFindIssuedJHMaterialOverDateRangeDataSet = new FindIssuedJHMaterialOverDateRangeDataSet();
        FindEmployeeByEmployeeIDDataSet TheFindEmployeeByEmployeeIDDataSet = new FindEmployeeByEmployeeIDDataSet();
        FindReceivedJHMaterialsFromVendorDateRangeDataSet TheFindReceivedJHMaterialsFromVendorDataSet = new FindReceivedJHMaterialsFromVendorDateRangeDataSet();
        FindIssuedJHMaterialCostByProjectDataSet TheFindIssuedJHMaterialCostbyProjectDataSet = new FindIssuedJHMaterialCostByProjectDataSet();

        //setting up the created data set
        ReceivePartsCostingDataSet TheReceivePartsCostingDataSet = new ReceivePartsCostingDataSet();
        IssuePartsCostingDataSet TheIssuePartsCostingDataSet = new IssuePartsCostingDataSet();
        IssuedDateRangeCostingDataSet TheIssuedDateRangeCostingDataSet = new IssuedDateRangeCostingDataSet();

        int gintReceiveCounter;
        int gintReceiveNumberOfRecords;
        int gintIssueCounter;
        int gintIssueNumberOfRecords;
        string gstrReportSelection;
        string gstrReportCategory;
        decimal gdecReportCost;

        private void FindJHMaterialReceivedOverADateRange()
        {
            //setting up variables
            DateTime datStartDate = DateTime.Now;
            DateTime datEndDate = DateTime.Now;
            string strValueForValidation;
            string strErrorMessage = "";
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;
            int intCounter;
            int intNumberOfRecords;
            int intPartID;
            string strAssignedPartID;
            int intReceiveCounter;
            bool blnItemNotFound;
            int intQuantity;
            decimal decPartPrice;
            decimal decTotalPrice;
            
            try
            {
                //data validation
                strValueForValidation = txtStartDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "Start Date is not a Date\n";
                }
                else
                {
                    datStartDate = Convert.ToDateTime(strValueForValidation);
                }
                strValueForValidation = txtEndDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The End Date is not a Date\n";
                }
                else
                {
                    datEndDate = Convert.ToDateTime(strValueForValidation);
                }
                if(blnFatalError == false)
                {
                    blnFatalError = TheDataValidationClass.verifyDateRange(datStartDate, datEndDate);
                    if(blnFatalError == true)
                    {
                        strErrorMessage += "The Start Date is after the End Date";
                    }
                }
                if(blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage(strErrorMessage);
                }
                else
                {
                    TheReceivePartsCostingDataSet.receiveparts.Rows.Clear();

                    gintReceiveCounter = 0;
                    gintReceiveNumberOfRecords = 0;

                    TheFindReceiveJHMaterialOverADateRange = TheInventoryCostingClass.FindReceiveJHMaterialOverDateRange(datStartDate, datEndDate);

                    intNumberOfRecords = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange.Rows.Count - 1;

                    if(intNumberOfRecords > -1)
                    {
                        for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                        {
                            intPartID = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].PartID;
                            strAssignedPartID = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].AssignedProjectID;
                            decPartPrice = Convert.ToDecimal(TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].Price);
                            decPartPrice = Math.Round(decPartPrice, 2);
                            decPartPrice = Decimal.Parse(decPartPrice.ToString("0.00"));
                            intQuantity = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].Quantity;
                            decTotalPrice = intQuantity * decPartPrice;
                            blnItemNotFound = true;
                            gdecReportCost += decTotalPrice;
                            
                            if(gintReceiveCounter > 0)
                            {
                                for(intReceiveCounter = 0; intReceiveCounter <= gintReceiveNumberOfRecords; intReceiveCounter++)
                                {
                                    if(intPartID == TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].PartID)
                                    {
                                        if(strAssignedPartID == TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].AssignedProjectID)
                                        {
                                            blnItemNotFound = false;
                                            TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].Quantity += intQuantity;
                                            TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].TotalCost += decTotalPrice;
                                            
                                        }
                                    }
                                }
                            }

                            if(blnItemNotFound == true)
                            {
                                ReceivePartsCostingDataSet.receivepartsRow NewPartRow = TheReceivePartsCostingDataSet.receiveparts.NewreceivepartsRow();

                                NewPartRow.AssignedProjectID = strAssignedPartID;
                                NewPartRow.PartDescription = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].PartDescription;
                                NewPartRow.PartID = intPartID;
                                NewPartRow.PartNumber = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].PartNumber;
                                NewPartRow.PartPrice = decPartPrice;
                                NewPartRow.ProjectName = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].ProjectName;
                                NewPartRow.Quantity = intQuantity;
                                NewPartRow.TotalCost = (decTotalPrice);
                                NewPartRow.Warehouse = TheFindReceiveJHMaterialOverADateRange.FindReceiveJHMaterialOverDateRange[intCounter].FirstName;

                                TheReceivePartsCostingDataSet.receiveparts.Rows.Add(NewPartRow);
                                gintReceiveNumberOfRecords = gintReceiveCounter;
                                gintReceiveCounter++;
                            }
                        }
                    }

                    dgrResults.ItemsSource = TheReceivePartsCostingDataSet.receiveparts;
                    txtTotalCost.Text = Convert.ToString(gdecReportCost);
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Costing Reports // Find JH Material Received Over A Date Range " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
            
        }
        private void FindJHMaterialIssuedOverADateRange()
        {
            //setting local variable
            string strValueForValidation;
            DateTime datStartDate = DateTime.Now;
            DateTime datEndDate = DateTime.Now;
            string strErrorMessage = "";
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;
            int intPartID;
            string strWarehouse;
            bool blnIteNotFound;
            int intIssueCounter;
            int intNumberOfRecords;
            int intCounter;
            int intQuantity;
            decimal decPartPrice;
            decimal decTotalPrice;

            try
            {
                //data validation
                strValueForValidation = txtStartDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Start Date is not a Date\n";
                }
                else
                {
                    datStartDate = Convert.ToDateTime(strValueForValidation);
                }
                strValueForValidation = txtEndDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The End Date is not a Date\n";
                }
                else
                {
                    datEndDate = Convert.ToDateTime(strValueForValidation);
                }
                if(blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage(strErrorMessage);
                    return;
                }
                else
                {
                    if(datStartDate > datEndDate)
                    {
                        TheMessagesClass.ErrorMessage("The Start Date is after the End Datae");
                        return;
                    }
                }

                TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Clear();

                TheFindIssuedJHMaterialOverDateRangeDataSet = TheInventoryCostingClass.FindIssuedJHMaterialOverDateRange(datStartDate, datEndDate);

                intNumberOfRecords = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange.Rows.Count - 1;
                gintIssueCounter = 0;
                gintIssueNumberOfRecords = 0;
                gdecReportCost = 0;

                if(intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        //getting ready to check the record
                        intPartID = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].PartID;                        
                        intQuantity = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].Quantity;
                        strWarehouse = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].FirstName;
                        decPartPrice = Convert.ToDecimal(TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].Price);
                        decPartPrice = Math.Round(decPartPrice, 2);
                        decPartPrice = Decimal.Parse(decPartPrice.ToString("0.00"));
                        decTotalPrice = intQuantity * decPartPrice;
                        blnIteNotFound = true;
                        gdecReportCost += decTotalPrice;
                                                
                        if(gintIssueCounter > 0)
                        {
                            for(intIssueCounter = 0; intIssueCounter <= gintIssueNumberOfRecords; intIssueCounter++)
                            {
                                if(intPartID == TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].PartID)
                                {
                                    if(strWarehouse == TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].Warehouse)
                                    {
                                        blnIteNotFound = false;
                                        TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].Quantity += intQuantity;
                                        TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].TotalPrice += decTotalPrice;
                                    }
                                }
                            }
                        }

                        if(blnIteNotFound == true)
                        {
                            IssuedDateRangeCostingDataSet.issuedpartsRow NewIssuedPart = TheIssuedDateRangeCostingDataSet.issuedparts.NewissuedpartsRow();

                            NewIssuedPart.PartID = intPartID;
                            NewIssuedPart.PartDescription = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].PartDescription;
                            NewIssuedPart.PartNumber = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].PartNumber;
                            NewIssuedPart.PartPriced = decPartPrice;
                            NewIssuedPart.Quantity = intQuantity;
                            NewIssuedPart.TotalPrice = decTotalPrice;
                            NewIssuedPart.Warehouse = TheFindIssuedJHMaterialOverDateRangeDataSet.FindIssuedJHMaterialOverDateRange[intCounter].FirstName;

                            TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Add(NewIssuedPart);
                            gintIssueNumberOfRecords = gintIssueCounter;
                            gintIssueCounter++;
                        }

                    }
                }

                dgrResults.ItemsSource = TheIssuedDateRangeCostingDataSet.issuedparts;
                txtTotalCost.Text = Convert.ToString(gdecReportCost);
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Costing Reports // Find JH Material Issued Over a Date Range " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
        public CostingReports()
        {
            InitializeComponent();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            gdecReportCost = 0;

            if (gstrReportSelection == "Find Material Received By Date Range")
                FindJHMaterialReceivedOverADateRange();
            else if (gstrReportSelection == "Find Material Issued By Date Range")
                FindJHMaterialIssuedOverADateRange();
            else if (gstrReportSelection == "Find Material Received From Vendor")
                FindMaterialFromVendor();
            else if (gstrReportSelection == "Find Material Issued For a Project")
                FindMaterialIssuedForProject();
        }
        private void FindMaterialIssuedForProject()
        {
            //settig local variables
            string strAssignedProjectID;
            int intNumberOfRecords;
            int intCounter;
            int intIssueCounter;
            int intPartID;
            int intQuantity;
            decimal decPrice;
            decimal decTotalPrice;
            bool blnItemNotFound = true;

            try
            {
                strAssignedProjectID = txtEnterInformation.Text;
                if(strAssignedProjectID == "")
                {
                    TheMessagesClass.ErrorMessage("The Project ID Was Not Entered");
                    return;
                }

                TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Clear();

                TheFindIssuedJHMaterialCostbyProjectDataSet = TheInventoryCostingClass.FindIssuedJHMaterialCostByProject(strAssignedProjectID);

                intNumberOfRecords = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject.Rows.Count - 1;
                gintIssueCounter = 0;
                gintIssueNumberOfRecords = 0;

                if(intNumberOfRecords == -1)
                {
                    TheMessagesClass.InformationMessage("Parts Were Not Issued For This Project");
                    return;
                }

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intPartID = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].PartID;
                    intQuantity = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].Quantity;
                    decPrice = Convert.ToDecimal(TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].Price);
                    decPrice = Math.Round(decPrice, 2);
                    decPrice = Decimal.Parse(decPrice.ToString("0.00"));
                    decTotalPrice = intQuantity * decPrice;
                    gdecReportCost += decTotalPrice;
                    blnItemNotFound = true;

                    if(gintIssueCounter > 0)
                    {
                        for(intIssueCounter = 0; intIssueCounter <= gintIssueNumberOfRecords; intIssueCounter++)
                        {
                            if(intPartID == TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].PartID)
                            {
                                TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].Quantity += intQuantity;
                                TheIssuedDateRangeCostingDataSet.issuedparts[intIssueCounter].TotalPrice += decTotalPrice;
                                blnItemNotFound = false;
                            }
                        }
                    }

                    if(blnItemNotFound == true)
                    {
                        IssuedDateRangeCostingDataSet.issuedpartsRow NewIssuedPart = TheIssuedDateRangeCostingDataSet.issuedparts.NewissuedpartsRow();

                        NewIssuedPart.PartID = intPartID;
                        NewIssuedPart.PartDescription = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].PartDescription;
                        NewIssuedPart.PartNumber = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].PartNumber;
                        NewIssuedPart.PartPriced = decPrice;
                        NewIssuedPart.Quantity = intQuantity;
                        NewIssuedPart.TotalPrice = decTotalPrice;
                        NewIssuedPart.Warehouse = TheFindIssuedJHMaterialCostbyProjectDataSet.FindIssuedJHMaterialCostByProject[intCounter].FirstName;

                        TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Add(NewIssuedPart);
                        gintIssueNumberOfRecords = gintIssueCounter;
                        gintIssueCounter++;
                    }
                }

                dgrResults.ItemsSource = TheIssuedDateRangeCostingDataSet.issuedparts;
                txtTotalCost.Text = Convert.ToString(gdecReportCost);
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Costing Reports // Find Material Issued For Project " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
        private void FindMaterialFromVendor()
        {
            //setting local variables
            string strValueForValidation;
            string strErrorMessage = "";
            DateTime datStartDate = DateTime.Now;
            DateTime datEndDate = DateTime.Now;
            string strVendor;
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;
            int intNumberOfRecords;
            int intCounter;
            int intPartID;
            decimal decPartPrice;
            decimal decTotalPrice;
            int intQuantity;
            int intReceiveCounter;
            bool blnItemNotFound = true;

            try
            {
                //data validation
                strVendor = txtEnterInformation.Text;
                if(strVendor == "")
                {
                    blnFatalError = true;
                    strErrorMessage += "The Vendor Was Not Entered\n";
                }
                strValueForValidation = txtStartDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Start Date is not a Date\n";
                }
                else
                {
                    DateTime.TryParse(strValueForValidation, out datStartDate);
                }
                strValueForValidation = txtEndDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The End Date is not a Date\n";
                }
                else
                {
                    DateTime.TryParse(strValueForValidation, out datEndDate);
                }
                if(blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage(strErrorMessage);
                    return;
                }
                else
                {
                    blnFatalError = TheDataValidationClass.verifyDateRange(datStartDate, datEndDate);

                    if(blnFatalError == true)
                    {
                        TheMessagesClass.ErrorMessage("The Start Date is After the End Date");
                        return;
                    }
                }

                TheFindReceivedJHMaterialsFromVendorDataSet = TheInventoryCostingClass.FindReceivedJHMaterialsFromVendorDateRange(strVendor, datStartDate, datEndDate);

                intNumberOfRecords = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange.Rows.Count - 1;
                gintReceiveCounter = 0;
                gintReceiveNumberOfRecords = 0;
                TheReceivePartsCostingDataSet.receiveparts.Rows.Clear();

                if(intNumberOfRecords == -1)
                {
                    TheMessagesClass.InformationMessage("No Records Found For Vendor During Date Range");
                    return;
                }
                else
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intPartID = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].PartID;
                        intQuantity = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].Quantity;
                        decPartPrice = Convert.ToDecimal(TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].Price);
                        decPartPrice = Math.Round(decPartPrice, 2);
                        decPartPrice = Decimal.Parse(decPartPrice.ToString("0.00"));
                        decTotalPrice = intQuantity * decPartPrice;
                        blnItemNotFound = true;
                        gdecReportCost += decTotalPrice;

                        if(gintReceiveCounter > 0)
                        {
                            for(intReceiveCounter = 0; intReceiveCounter <= gintReceiveNumberOfRecords; intReceiveCounter++)
                            {
                                if (intPartID == TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].PartID)
                                {
                                    blnItemNotFound = false;
                                    TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].Quantity += intQuantity;
                                    TheReceivePartsCostingDataSet.receiveparts[intReceiveCounter].TotalCost += decTotalPrice;
                                }
                            }
                        }

                        if(blnItemNotFound == true)
                        {
                            ReceivePartsCostingDataSet.receivepartsRow NewPartRow = TheReceivePartsCostingDataSet.receiveparts.NewreceivepartsRow();

                            NewPartRow.AssignedProjectID = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].AssignedProjectID;
                            NewPartRow.PartDescription = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].PartDescription;
                            NewPartRow.PartID = intPartID;
                            NewPartRow.PartNumber = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].PartNumber;
                            NewPartRow.PartPrice = decPartPrice;
                            NewPartRow.ProjectName = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].ProjectName;
                            NewPartRow.Quantity = intQuantity;
                            NewPartRow.TotalCost = (decTotalPrice);
                            NewPartRow.Warehouse = TheFindReceivedJHMaterialsFromVendorDataSet.FindReceivedJHMasterialsFromVendorDateRange[intCounter].FirstName;

                            TheReceivePartsCostingDataSet.receiveparts.Rows.Add(NewPartRow);
                            gintReceiveNumberOfRecords = gintReceiveCounter;
                            gintReceiveCounter++;
                        }
                    }
                }

                dgrResults.ItemsSource = TheReceivePartsCostingDataSet.receiveparts;
                txtTotalCost.Text = Convert.ToString(gdecReportCost);
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Costing Reports // Find Material From Vendor " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this will load the combo box
            cboReportSelection.Items.Add("Select Report");
            cboReportSelection.Items.Add("Find Material Received By Date Range");
            cboReportSelection.Items.Add("Find Material Received From Vendor");
            cboReportSelection.Items.Add("Find Material Issued By Date Range");
            cboReportSelection.Items.Add("Find Material Issued For a Project");

            cboReportSelection.SelectedIndex = 0;

            HideControls();
        }

        private void cboReportSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int intSelectedIndex;
            string strComboBoxSelection;

            intSelectedIndex = cboReportSelection.SelectedIndex;
            txtEnterInformation.Text = "";
            txtTotalCost.Text = "";
            txtEndDate.Visibility = Visibility.Visible;
            txtStartDate.Visibility = Visibility.Visible;

            if(intSelectedIndex > 0)
            {
                strComboBoxSelection = cboReportSelection.SelectedItem.ToString();

                if(strComboBoxSelection == "Find Material Received By Date Range")
                {
                    gstrReportSelection = "Find Material Received By Date Range";
                    gstrReportCategory = "Receive Only";
                    HideControls();
                }
                else if(strComboBoxSelection == "Find Material Issued By Date Range")
                {
                    gstrReportSelection = "Find Material Issued By Date Range";
                    gstrReportCategory = "Issue Date Range Only";
                    HideControls();
                }
                else if (strComboBoxSelection == "Find Material Received From Vendor")
                {
                    gstrReportSelection = "Find Material Received From Vendor";
                    gstrReportCategory = "Receive Only";
                    lblEnterInformation.Visibility = Visibility.Visible;
                    lblEnterInformation.Content = "Enter Vendor";
                    txtEnterInformation.Visibility = Visibility.Visible;
                }
                else if (strComboBoxSelection == "Find Material Issued For a Project")
                {
                    gstrReportSelection = "Find Material Issued For a Project";
                    gstrReportCategory = "Issue Only";
                    lblEnterInformation.Visibility = Visibility.Visible;
                    txtEnterInformation.Visibility = Visibility.Visible;
                    lblEnterInformation.Content = "Enter Project ID";
                    txtStartDate.Visibility = Visibility.Hidden;
                    txtEndDate.Visibility = Visibility.Hidden;
                }
            }
        }
        private void HideControls()
        {
            txtEnterInformation.Visibility = Visibility.Hidden;
            cboSelectEmployee.Visibility = Visibility.Hidden;
            lblEnterInformation.Visibility = Visibility.Hidden;
            lblSelectEmployee.Visibility = Visibility.Hidden;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            string strComboBoxSelection;

            strComboBoxSelection = cboReportSelection.SelectedItem.ToString();

            if (strComboBoxSelection == "Find Material Received By Date Range")
            {
                PrintReceivedParts();
            }
            else if (strComboBoxSelection == "Find Material Issued By Date Range")
            {
                txtEnterInformation.Text = "";
                PrintIssuedDateRange();
            }
            else if (strComboBoxSelection == "Find Material Received From Vendor")
            {
                PrintReceivedParts();
            }
            else if (strComboBoxSelection == "Find Material Issued For a Project")
            {
                PrintIssuedDateRange();
            }
        }
        
        private void PrintIssuedDateRange()
        {
            //this will print the report
            int intCurrentRow = 0;
            int intCounter;
            int intColumns;
            int intNumberOfRecords;
            string strComboBoxSelection;

            try
            {
                PrintDialog pdIssuedDateReport = new PrintDialog();
                strComboBoxSelection = cboReportSelection.SelectedItem.ToString();

                if (pdIssuedDateReport.ShowDialog().Value)
                {
                    FlowDocument fdIssuedDateReport = new FlowDocument();
                    Thickness thickness = new Thickness(100, 50, 50, 50);
                    fdIssuedDateReport.PagePadding = thickness;

                    pdIssuedDateReport.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                    //Set Up Table Columns
                    Table IssuedDateReportTable = new Table();
                    fdIssuedDateReport.Blocks.Add(IssuedDateReportTable);
                    IssuedDateReportTable.CellSpacing = 0;
                    intColumns = TheIssuedDateRangeCostingDataSet.issuedparts.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        IssuedDateReportTable.Columns.Add(new TableColumn());
                    }

                    IssuedDateReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    IssuedDateReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = IssuedDateReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Blue Jay Communications Issued Inventory Costing Report"))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    if(strComboBoxSelection == "Find Material Issued For a Project")
                    {
                        IssuedDateReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = IssuedDateReportTable.RowGroups[0].Rows[intCurrentRow];
                        newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("For Project: " + txtEnterInformation.Text))));
                        newTableRow.Cells[0].FontSize = 16;
                        newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                        newTableRow.Cells[0].ColumnSpan = intColumns;
                        newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                        newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);
                    }
                    
                    IssuedDateReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++; 
                    newTableRow = IssuedDateReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Total Cost: " + txtTotalCost.Text))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Header Row
                    IssuedDateReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++;
                    newTableRow = IssuedDateReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("TransactionID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Total Cost"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Warehouse"))));
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Format Header Row
                    for (intCounter = 0; intCounter < intColumns; intCounter++)
                    {
                        newTableRow.Cells[intCounter].FontSize = 11;
                        newTableRow.Cells[intCounter].FontFamily = new FontFamily("Times New Roman");
                        newTableRow.Cells[intCounter].BorderBrush = Brushes.Black;
                        newTableRow.Cells[intCounter].TextAlignment = TextAlignment.Center;
                        newTableRow.Cells[intCounter].BorderThickness = new Thickness();
                    }

                    intNumberOfRecords = TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        IssuedDateReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = IssuedDateReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheIssuedDateRangeCostingDataSet.issuedparts[intReportRowCounter][intColumnCounter].ToString()))));


                            newTableRow.Cells[intColumnCounter].FontSize = 8;
                            newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                            newTableRow.Cells[intColumnCounter].BorderBrush = Brushes.LightSteelBlue;
                            newTableRow.Cells[intColumnCounter].BorderThickness = new Thickness(0, 0, 0, 1);
                            newTableRow.Cells[intColumnCounter].TextAlignment = TextAlignment.Center;
                        }
                    }

                    //Set up page and print
                    fdIssuedDateReport.ColumnWidth = pdIssuedDateReport.PrintableAreaWidth;
                    fdIssuedDateReport.PageHeight = pdIssuedDateReport.PrintableAreaHeight;
                    fdIssuedDateReport.PageWidth = pdIssuedDateReport.PrintableAreaWidth;
                    pdIssuedDateReport.PrintDocument(((IDocumentPaginatorSource)fdIssuedDateReport).DocumentPaginator, "Blue Jay Communications Issued Inventory Costing Report");
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // Costing Reports // Print Received Report " + Ex.Message);
            }
        }
        private void PrintReceivedParts()
        {
            //this will print the report
            int intCurrentRow = 0;
            int intCounter;
            int intColumns;
            int intNumberOfRecords;


            try
            {
                PrintDialog pdReceivedReport = new PrintDialog();

                if (pdReceivedReport.ShowDialog().Value)
                {
                    FlowDocument fdReceivedReport = new FlowDocument();
                    Thickness thickness = new Thickness(100, 50, 50, 50);
                    fdReceivedReport.PagePadding = thickness;

                    pdReceivedReport.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                    //Set Up Table Columns
                    Table ReceivedReportTable = new Table();
                    fdReceivedReport.Blocks.Add(ReceivedReportTable);
                    ReceivedReportTable.CellSpacing = 0;
                    intColumns = TheReceivePartsCostingDataSet.receiveparts.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                       ReceivedReportTable.Columns.Add(new TableColumn());
                    }
                    ReceivedReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ReceivedReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ReceivedReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Blue Jay Communications Received Inventory Costing Report"))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Header Row
                    ReceivedReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++;
                    newTableRow = ReceivedReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("TransactionID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Project ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Project Name"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Warehouse"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Total Cost"))));
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Format Header Row
                    for (intCounter = 0; intCounter < intColumns; intCounter++)
                    {
                        newTableRow.Cells[intCounter].FontSize = 11;
                        newTableRow.Cells[intCounter].FontFamily = new FontFamily("Times New Roman");
                        newTableRow.Cells[intCounter].BorderBrush = Brushes.Black;
                        newTableRow.Cells[intCounter].TextAlignment = TextAlignment.Center;
                        newTableRow.Cells[intCounter].BorderThickness = new Thickness();
                    }

                    intNumberOfRecords = TheReceivePartsCostingDataSet.receiveparts.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ReceivedReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ReceivedReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheReceivePartsCostingDataSet.receiveparts[intReportRowCounter][intColumnCounter].ToString()))));


                            newTableRow.Cells[intColumnCounter].FontSize = 8;
                            newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                            newTableRow.Cells[intColumnCounter].BorderBrush = Brushes.LightSteelBlue;
                            newTableRow.Cells[intColumnCounter].BorderThickness = new Thickness(0, 0, 0, 1);
                            newTableRow.Cells[intColumnCounter].TextAlignment = TextAlignment.Center;
                        }
                    }

                    //Set up page and print
                    fdReceivedReport.ColumnWidth = pdReceivedReport.PrintableAreaWidth;
                    fdReceivedReport.PageHeight = pdReceivedReport.PrintableAreaHeight;
                    fdReceivedReport.PageWidth = pdReceivedReport.PrintableAreaWidth;
                    pdReceivedReport.PrintDocument(((IDocumentPaginatorSource)fdReceivedReport).DocumentPaginator, "Blue Jay Communications Received Inventory Costing Report");
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // Costing Reports // Print Received Report " + Ex.Message);
            }
        }

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            string strComboBoxSelection;

            strComboBoxSelection = cboReportSelection.SelectedItem.ToString();

            if (strComboBoxSelection == "Find Material Received By Date Range")
            {
                ExportReceivedParts();
            }
            else if (strComboBoxSelection == "Find Material Issued By Date Range")
            {
                ExportIssuedParts();
            }
            else if (strComboBoxSelection == "Find Material Received From Vendor")
            {
                ExportReceivedParts();
            }
            else if (strComboBoxSelection == "Find Material Issued For a Project")
            {
                ExportIssuedParts();
            }
        }
        private void ExportIssuedParts()
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            //try catch for exceptions
            try
            {
                //creating the file writer
                SaveFileDialog Excelfile = new SaveFileDialog();
                Excelfile.ShowDialog();
                ReadWirteCSV.CsvFileWriter TheReconCSV = new ReadWirteCSV.CsvFileWriter(Excelfile.FileName + ".csv");

                intNumberOfRecords = TheIssuedDateRangeCostingDataSet.issuedparts.Rows.Count - 1;

                //calling the writer
                using (TheReconCSV)
                {
                    ReadWirteCSV.CsvRow NewTitleRow = new ReadWirteCSV.CsvRow();

                    NewTitleRow.Add("TransactionID");
                    NewTitleRow.Add("PartID");
                    NewTitleRow.Add("PartNumber");
                    NewTitleRow.Add("Description");
                    NewTitleRow.Add("Warehouse");
                    NewTitleRow.Add("Quantity");
                    NewTitleRow.Add("PartPrice");
                    NewTitleRow.Add("Total");

                    //writing the new row
                    TheReconCSV.WriteRow(NewTitleRow);

                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        //creating a new row
                        ReadWirteCSV.CsvRow NewCSVRow = new ReadWirteCSV.CsvRow();

                        NewCSVRow.Add(Convert.ToString(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].TransactionID));
                        NewCSVRow.Add(Convert.ToString(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].PartID));
                        NewCSVRow.Add(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].PartNumber);
                        NewCSVRow.Add(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].PartDescription);
                        NewCSVRow.Add(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].Warehouse);
                        NewCSVRow.Add(Convert.ToString(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].Quantity));
                        NewCSVRow.Add(Convert.ToString(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].PartPriced));
                        NewCSVRow.Add(Convert.ToString(TheIssuedDateRangeCostingDataSet.issuedparts[intCounter].TotalPrice));

                        //writing the new row
                        TheReconCSV.WriteRow(NewCSVRow);
                    }

                }

                //output to user
                TheMessagesClass.InformationMessage("The File Has Been Saved to Your Selected location");
            }
            catch (Exception Ex)
            {
                //message to user
                TheMessagesClass.ErrorMessage(Ex.ToString());

                //event log entry
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Export Received Parts // Export to CSV File // " + Ex.Message);
            }
        }
        private void ExportReceivedParts()
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            //try catch for exceptions
            try
            {
                //creating the file writer
                SaveFileDialog Excelfile = new SaveFileDialog();
                Excelfile.ShowDialog();
                ReadWirteCSV.CsvFileWriter TheReconCSV = new ReadWirteCSV.CsvFileWriter(Excelfile.FileName + ".csv");

                intNumberOfRecords = TheReceivePartsCostingDataSet.receiveparts.Rows.Count - 1;

                //calling the writer
                using (TheReconCSV)
                {
                    ReadWirteCSV.CsvRow NewTitleRow = new ReadWirteCSV.CsvRow();

                    NewTitleRow.Add("TransactionID");
                    NewTitleRow.Add("PartID");
                    NewTitleRow.Add("PartNumber");
                    NewTitleRow.Add("Description");
                    NewTitleRow.Add("ProjectID");
                    NewTitleRow.Add("ProjectName");
                    NewTitleRow.Add("Warehouse");
                    NewTitleRow.Add("Quantity");
                    NewTitleRow.Add("PartPrice");
                    NewTitleRow.Add("Total");

                    //writing the new row
                    TheReconCSV.WriteRow(NewTitleRow);

                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        //creating a new row
                        ReadWirteCSV.CsvRow NewCSVRow = new ReadWirteCSV.CsvRow();

                        NewCSVRow.Add(Convert.ToString(TheReceivePartsCostingDataSet.receiveparts[intCounter].TransactionID));
                        NewCSVRow.Add(Convert.ToString(TheReceivePartsCostingDataSet.receiveparts[intCounter].PartID));
                        NewCSVRow.Add(TheReceivePartsCostingDataSet.receiveparts[intCounter].PartNumber);
                        NewCSVRow.Add(TheReceivePartsCostingDataSet.receiveparts[intCounter].PartDescription);
                        NewCSVRow.Add(TheReceivePartsCostingDataSet.receiveparts[intCounter].AssignedProjectID);
                        NewCSVRow.Add(TheReceivePartsCostingDataSet.receiveparts[intCounter].ProjectName);
                        NewCSVRow.Add(TheReceivePartsCostingDataSet.receiveparts[intCounter].Warehouse);
                        NewCSVRow.Add(Convert.ToString(TheReceivePartsCostingDataSet.receiveparts[intCounter].Quantity));
                        NewCSVRow.Add(Convert.ToString(TheReceivePartsCostingDataSet.receiveparts[intCounter].PartPrice));
                        NewCSVRow.Add(Convert.ToString(TheReceivePartsCostingDataSet.receiveparts[intCounter].TotalCost));

                        //writing the new row
                        TheReconCSV.WriteRow(NewCSVRow);
                    }

                }

                //output to user
                TheMessagesClass.InformationMessage("The File Has Been Saved to Your Selected location");
            }
            catch (Exception Ex)
            {
                //message to user
                TheMessagesClass.ErrorMessage(Ex.ToString());

                //event log entry
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Export Received Parts // Export to CSV File // " + Ex.Message);
            }
        }
    }
}
