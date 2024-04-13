/* Title:           Part Number Warehouse Report
 * Date:            7-6-17
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
using NewEventLogDLL;
using InventoryDLL;
using ReceivePartsDLL;
using IssuedPartsDLL;
using NewPartNumbersDLL;
using DataValidationDLL;
using DateSearchDLL;

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for PartNumberWarehouseReport.xaml
    /// </summary>
    public partial class PartNumberWarehouseReport : Window
    {
        //setting the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        ReceivePartsClass TheReceivePartsClass = new ReceivePartsClass();
        IssuedPartsClass TheIssuedPartsClass = new IssuedPartsClass();
        PartNumberClass ThePartNumberClass = new PartNumberClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        DateSearchClass TheDataSearchClass = new DateSearchClass();

        //setting up the data
        FindPartByPartIDDataSet TheFindPartByPartIDDataSet = new FindPartByPartIDDataSet();
        FindPartByJDEPartNumberDataSet TheFindPartByJDEPartNumberDataSet = new FindPartByJDEPartNumberDataSet();
        FindPartByPartNumberDataSet TheFindPartByPartNumberDataSet = new FindPartByPartNumberDataSet();
        FindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet = new FindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet();
        FindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet = new FindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet();

        //setting created data sets
        WarehousePartNumberReportDataSet TheTotalWarehousePartNumberReportDataSet = new WarehousePartNumberReportDataSet();
        WarehousePartNumberReportDataSet TheWarehousePartnumberReportTransactionsDataSet = new WarehousePartNumberReportDataSet();

        int gintWarehouseID;

        public PartNumberWarehouseReport()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            try
            {
                cboSelectWarehouse.Items.Add("Select Warehouse");

                intNumberOfRecords = MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    cboSelectWarehouse.Items.Add(MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses[intCounter].FirstName);
                }

                cboSelectWarehouse.SelectedIndex = 0;
                btnPrint.IsEnabled = false;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Part Number Warehouse Report // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }

        private void btnFindTransactions_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            string strPartNumber;
            int intPartID = 0;
            string strValueForValidation;
            DateTime datStartDate = DateTime.Now;
            DateTime datEndDate = DateTime.Now;
            int intCounter;
            int intNumberOfRecords;
            int intRecordsReturned = 0;
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;
            string strErrorMessage = "";
            string strJDEPartNumber = "";
            string strPartDescription = "";

            try
            {
                TheTotalWarehousePartNumberReportDataSet.partnumberreport.Rows.Clear();
                TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.Rows.Clear();

                //beginning data validation
                strPartNumber = txtEnterPartNumber.Text;
                if (strPartNumber == "")
                {
                    blnFatalError = true;
                    strErrorMessage += "Part Number Was Not Entered\n";
                }
                else
                {
                    blnThereIsAProblem = TheDataValidationClass.VerifyIntegerData(strPartNumber);
                    if (blnThereIsAProblem == false)
                    {
                        intPartID = Convert.ToInt32(strPartNumber);

                        //checking for part if part ID
                        TheFindPartByPartIDDataSet = ThePartNumberClass.FindPartByPartID(intPartID);

                        intRecordsReturned = TheFindPartByPartIDDataSet.FindPartByPartID.Rows.Count;

                        if(intRecordsReturned > 0)
                        {
                            strPartDescription = TheFindPartByPartIDDataSet.FindPartByPartID[0].PartDescription;
                            strPartNumber = TheFindPartByPartIDDataSet.FindPartByPartID[0].PartNumber;
                            strJDEPartNumber = TheFindPartByPartIDDataSet.FindPartByPartID[0].JDEPartNumber;
                        }

                    }

                    if(intRecordsReturned == 0)
                    {
                        TheFindPartByPartNumberDataSet = ThePartNumberClass.FindPartByPartNumber(strPartNumber);

                        intRecordsReturned = TheFindPartByPartNumberDataSet.FindPartByPartNumber.Rows.Count;

                        if(intRecordsReturned == 0)
                        {
                            TheFindPartByJDEPartNumberDataSet = ThePartNumberClass.FindPartByJDEPartNumber(strPartNumber);

                            intRecordsReturned = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber.Rows.Count;

                            if(intRecordsReturned == 0)
                            {
                                blnFatalError = true;
                                strErrorMessage += "The Part Number Was Not Found\n";
                            }
                            else
                            {
                                intPartID = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber[0].PartID;
                                strPartDescription = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber[0].PartDescription;
                                strPartNumber = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber[0].PartNumber;
                                strJDEPartNumber = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber[0].JDEPartNumber;
                            }
                        }
                        else
                        {
                            intPartID = TheFindPartByPartNumberDataSet.FindPartByPartNumber[0].PartID;
                            strPartDescription = TheFindPartByPartNumberDataSet.FindPartByPartNumber[0].PartDescription;
                            strJDEPartNumber = TheFindPartByPartNumberDataSet.FindPartByPartNumber[0].JDEPartNumber;
                        }
                    }
                    
                }
                strValueForValidation = txtEnterStartDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Start Date Is Not a Date\n";
                }
                else
                {
                    datStartDate = Convert.ToDateTime(strValueForValidation);
                }
                strValueForValidation = txtEnterEndDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The End Date Is Not a Date\n";
                }
                else
                {
                    datEndDate = Convert.ToDateTime(strValueForValidation);
                }
                if(cboSelectWarehouse.SelectedIndex <= 0)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Warehouse Was Not Selected\n";
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
                        TheMessagesClass.ErrorMessage("The Starting Date Is After The Ending Date");
                        return;
                    }
                }

                TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet = TheIssuedPartsClass.FindIssuedPartsByPartIDWarehouseIDDateRange(intPartID, gintWarehouseID, datStartDate, datEndDate);

                TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet = TheReceivePartsClass.FindReceivedPartsByPartIDWarehouseIDDateRange(intPartID, gintWarehouseID, datStartDate, datEndDate);

                intRecordsReturned = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange.Rows.Count;

                if(intRecordsReturned == 0)
                {
                    intRecordsReturned = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange.Rows.Count;

                    if(intRecordsReturned == 0)
                    {
                        TheMessagesClass.InformationMessage("There Are No Transactions For This Part\nWarehouse, and Date Range");
                        btnPrint.IsEnabled = false;
                    }
                }

                //creating totals data set
                WarehousePartNumberReportDataSet.partnumberreportRow NewReportRow = TheTotalWarehousePartNumberReportDataSet.partnumberreport.NewpartnumberreportRow();

                NewReportRow.DIDNumber = "NOT NEEDED";
                NewReportRow.JDEPartNumber = strJDEPartNumber;
                NewReportRow.PartDescription = strPartDescription;
                NewReportRow.PartID = intPartID;
                NewReportRow.PartNumber = strPartNumber;
                NewReportRow.QTYIssued = 0;
                NewReportRow.QTYReceived = 0;
                NewReportRow.TransactionDate = DateTime.Now;

                TheTotalWarehousePartNumberReportDataSet.partnumberreport.Rows.Add(NewReportRow);

                intNumberOfRecords = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].QTYIssued += TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].Quantity;

                    WarehousePartNumberReportDataSet.partnumberreportRow NewPartRow = TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.NewpartnumberreportRow();

                    NewPartRow.DIDNumber = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].AssignedProjectID;
                    NewPartRow.JDEPartNumber = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].JDEPartNumber;
                    NewPartRow.PartDescription = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].PartDescription;
                    NewPartRow.PartNumber = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].PartNumber;
                    NewPartRow.PartID = intPartID;
                    NewPartRow.QTYIssued = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].Quantity;
                    NewPartRow.QTYReceived = 0;
                    NewPartRow.TransactionDate = TheFindIssuedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindIssuedPartsByPartIDWarehouseIDAndDateRange[intCounter].TransactionDate;

                    TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.Rows.Add(NewPartRow);
                }

                intNumberOfRecords = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange.Rows.Count - 1;

                for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].QTYReceived += TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].Quantity;

                    WarehousePartNumberReportDataSet.partnumberreportRow NewPartRow = TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.NewpartnumberreportRow();

                    NewPartRow.DIDNumber = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].AssignedProjectID;
                    NewPartRow.JDEPartNumber = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].JDEPartNumber;
                    NewPartRow.PartDescription = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].PartDescription;
                    NewPartRow.PartNumber = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].PartNumber;
                    NewPartRow.PartID = intPartID;
                    NewPartRow.QTYIssued = 0;
                    NewPartRow.QTYReceived = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].Quantity;
                    NewPartRow.TransactionDate = TheFindReceivedPartsByPartIDWarehouseIDAndDateRangeDataSet.FindReceivedPartsByPartIDWarehouseIDAndDateRange[intCounter].TransactionDate;

                    TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.Rows.Add(NewPartRow);
                }

                txtJDEPartNumber.Text = TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].JDEPartNumber;
                txtPartDescription.Text = TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartDescription;
                txtPartID.Text = Convert.ToString(intPartID);
                txtPartNumber.Text = TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartNumber;
                txtQTYIssued.Text = Convert.ToString(TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].QTYIssued);
                txtQTYReceived.Text = Convert.ToString(TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].QTYReceived);

                dgrResults.ItemsSource = TheWarehousePartnumberReportTransactionsDataSet.partnumberreport;
                btnPrint.IsEnabled = true;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Part Number Warehouse Report // Find Transactions Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
            
        }

        private void cboSelectWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //setting local variables
            int intSelectedIndex;

            intSelectedIndex = cboSelectWarehouse.SelectedIndex;

            if(intSelectedIndex > 0)
            {
                gintWarehouseID = MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses[intSelectedIndex - 1].EmployeeID;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //this will print the report
            int intCurrentRow = 0;
            int intCounter;
            int intColumns;
            int intNumberOfRecords;


            try
            {
                PrintDialog pdProjectReport = new PrintDialog();

                if (pdProjectReport.ShowDialog().Value)
                {
                    FlowDocument fdProjectReport = new FlowDocument();
                    Thickness thickness = new Thickness(100, 50, 50, 50);
                    fdProjectReport.PagePadding = thickness;

                    pdProjectReport.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                    //Set Up Table Columns
                    Table ProjectReportTable = new Table();
                    fdProjectReport.Blocks.Add(ProjectReportTable);
                    ProjectReportTable.CellSpacing = 0;
                    intColumns = TheTotalWarehousePartNumberReportDataSet.partnumberreport.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        ProjectReportTable.Columns.Add(new TableColumn());
                    }
                    ProjectReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number Report For " + TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartNumber + " In Warehouse " + cboSelectWarehouse.SelectedItem.ToString()))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Header Row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++;
                    newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Transaction ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("JDE Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("DID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("QTY Received"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("QTY Issued"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Date"))));
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

                    intNumberOfRecords = TheTotalWarehousePartNumberReportDataSet.partnumberreport.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheTotalWarehousePartNumberReportDataSet.partnumberreport[intReportRowCounter][intColumnCounter].ToString()))));


                            newTableRow.Cells[intColumnCounter].FontSize = 8;
                            newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                            newTableRow.Cells[intColumnCounter].BorderBrush = Brushes.LightSteelBlue;
                            newTableRow.Cells[intColumnCounter].BorderThickness = new Thickness(0, 0, 0, 1);
                            newTableRow.Cells[intColumnCounter].TextAlignment = TextAlignment.Center;
                        }
                    }



                    //Set up page and print
                    fdProjectReport.ColumnWidth = pdProjectReport.PrintableAreaWidth;
                    fdProjectReport.PageHeight = pdProjectReport.PrintableAreaHeight;
                    fdProjectReport.PageWidth = pdProjectReport.PrintableAreaWidth;
                    pdProjectReport.PrintDocument(((IDocumentPaginatorSource)fdProjectReport).DocumentPaginator, "Part Number Report For " + TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartNumber + " In Warehouse " + cboSelectWarehouse.SelectedItem.ToString());
                    intCurrentRow = 0;

                }

                PrintTransactions();
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // Part Number Warehouse Report // Print Button " + Ex.Message);
            }
        }

        private void PrintTransactions()
        {
            //this will print the report
            int intCurrentRow = 0;
            int intCounter;
            int intColumns;
            int intNumberOfRecords;


            try
            {
                PrintDialog pdProjectReport = new PrintDialog();

                if (pdProjectReport.ShowDialog().Value)
                {
                    FlowDocument fdProjectReport = new FlowDocument();
                    Thickness thickness = new Thickness(100, 50, 50, 50);
                    fdProjectReport.PagePadding = thickness;

                    pdProjectReport.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

                    //Set Up Table Columns
                    Table ProjectReportTable = new Table();
                    fdProjectReport.Blocks.Add(ProjectReportTable);
                    ProjectReportTable.CellSpacing = 0;
                    intColumns = TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        ProjectReportTable.Columns.Add(new TableColumn());
                    }
                    ProjectReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number Transaction Report For " + TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartNumber + " In Warehouse " + cboSelectWarehouse.SelectedItem.ToString()))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Header Row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++;
                    newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Transaction ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("JDE Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("DID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("QTY Received"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("QTY Issued"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Date"))));
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

                    intNumberOfRecords = TheWarehousePartnumberReportTransactionsDataSet.partnumberreport.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheWarehousePartnumberReportTransactionsDataSet.partnumberreport[intReportRowCounter][intColumnCounter].ToString()))));


                            newTableRow.Cells[intColumnCounter].FontSize = 8;
                            newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                            newTableRow.Cells[intColumnCounter].BorderBrush = Brushes.LightSteelBlue;
                            newTableRow.Cells[intColumnCounter].BorderThickness = new Thickness(0, 0, 0, 1);
                            newTableRow.Cells[intColumnCounter].TextAlignment = TextAlignment.Center;
                        }
                    }



                    //Set up page and print
                    fdProjectReport.ColumnWidth = pdProjectReport.PrintableAreaWidth;
                    fdProjectReport.PageHeight = pdProjectReport.PrintableAreaHeight;
                    fdProjectReport.PageWidth = pdProjectReport.PrintableAreaWidth;
                    pdProjectReport.PrintDocument(((IDocumentPaginatorSource)fdProjectReport).DocumentPaginator, "Part Number Transaction Report For " + TheTotalWarehousePartNumberReportDataSet.partnumberreport[0].PartNumber + " In Warehouse " + cboSelectWarehouse.SelectedItem.ToString());
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // Part Number Warehouse Report // Print Transactions " + Ex.Message);
            }
        }
    }
}
