/* Title:           Warehouse Inventory Report
 * Date:            4-5-17
 * Author:          Terry Holmes
 * 
 * Description:     This form is used to see the current inventory within a selected warehouse */

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
using DataValidationDLL;
using Microsoft.Win32;
using CSVFileDLL;
using NewPartNumbersDLL;
using System.Printing;


namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for WarehouseInventoryReport.xaml
    /// </summary>
    public partial class WarehouseInventoryReport : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        ReadWirteCSV TheCSVFileClass = new ReadWirteCSV();
        PartNumberClass ThePartNumberClass = new PartNumberClass();

        //setting up the total inventory
        FindCompleteInventoryDataSet TheFindCompleteInventoryDataSet = new FindCompleteInventoryDataSet();
        WarehouseReport TheWarehouseReportDataSet = new WarehouseReport();
        FindWarehouseInventoryDataSet TheFindWarehouseInventoryDataSet = new FindWarehouseInventoryDataSet();
        FindWarehouseInventoryPartDataSet TheFindWarehouseInventoryPartDataSet = new FindWarehouseInventoryPartDataSet();
        FindPartByPartIDDataSet TheFindPartByPartIDDataSet = new FindPartByPartIDDataSet();
        FindPartByPartNumberDataSet TheFindPartByPartNumberDataSet = new FindPartByPartNumberDataSet();
        FindPartByJDEPartNumberDataSet TheFindPartByJDEPartNumberDataSet = new FindPartByJDEPartNumberDataSet();

        int gintWarehouseID;
        bool gblnFullWarehouse;
        
        public WarehouseInventoryReport()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            try
            {
                cboWarehouse.Items.Add("Select Warehouse");

                btnFindPart.IsEnabled = false;

                intNumberOfRecords = MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    cboWarehouse.Items.Add(MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses[intCounter].FirstName);
                }

                TheFindCompleteInventoryDataSet = TheInventoryClass.FindCompleteInventory();

                dgrInventory.ItemsSource = TheFindCompleteInventoryDataSet.FindCompleteInventory;

                cboWarehouse.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Warehouse Inventory Report // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
          
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }
       
        private void cboWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this will load up the information
            string strWarehouseName;
            int intSelectedIndex;
            int intCounter;
            int intNumberOfRecords;
            try
            {
                intSelectedIndex = cboWarehouse.SelectedIndex;

                if(intSelectedIndex > -1)
                {
                    strWarehouseName = cboWarehouse.SelectedItem.ToString();

                    if(strWarehouseName != "Select Warehouse")
                    {
                        intNumberOfRecords = MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses.Rows.Count - 1;

                        gblnFullWarehouse = true;

                        for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                        {
                            if(MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses[intCounter].FirstName == strWarehouseName)
                            {
                                gintWarehouseID = MainWindow.TheFindPartsWarehouseDataSet.FindPartsWarehouses[intCounter].EmployeeID;
                                break;
                            }
                        }

                        TheFindWarehouseInventoryDataSet = TheInventoryClass.FindWarehouseInventory(gintWarehouseID);

                        FillWarehouseDataSet();

                        btnFindPart.IsEnabled = true;
                    }
                    else
                    {
                        TheFindCompleteInventoryDataSet = TheInventoryClass.FindCompleteInventory();

                        dgrInventory.ItemsSource = TheFindCompleteInventoryDataSet.FindCompleteInventory;

                        btnFindPart.IsEnabled = false;
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Warehouse Inventory Report // cboWarehouse Selection Changed " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
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
                    intColumns = TheWarehouseReportDataSet.warehouseinventory.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        ProjectReportTable.Columns.Add(new TableColumn());
                    }
                    ProjectReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Warehouse Inventory Report for " + cboWarehouse.SelectedItem.ToString()))));
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
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("JDE Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Warehouse"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity"))));
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

                    intNumberOfRecords = TheWarehouseReportDataSet.warehouseinventory.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheWarehouseReportDataSet.warehouseinventory[intReportRowCounter][intColumnCounter].ToString()))));


                            newTableRow.Cells[intColumnCounter].FontSize = 10;
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
                    pdProjectReport.PrintDocument(((IDocumentPaginatorSource)fdProjectReport).DocumentPaginator, "Warehouse Inventory Report For " + cboWarehouse.SelectedItem.ToString());
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // MSR Report // Print Button " + Ex.Message);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            //try catch for exceptions
            try
            {
                SaveFileDialog file = new SaveFileDialog();
                file.ShowDialog();
                ReadWirteCSV.CsvFileWriter TheReconCSV = new ReadWirteCSV.CsvFileWriter(file.FileName + ".csv");

                intNumberOfRecords = TheWarehouseReportDataSet.warehouseinventory.Rows.Count - 1;

                //calling the writer
                using (TheReconCSV)
                {
                    ReadWirteCSV.CsvRow NewTitleRow = new ReadWirteCSV.CsvRow();

                    NewTitleRow.Add("Part Number");
                    NewTitleRow.Add("JDE Part Number");
                    NewTitleRow.Add("Description");
                    NewTitleRow.Add("Warehouse");
                    NewTitleRow.Add("Quantity");

                    //writing the new row
                    TheReconCSV.WriteRow(NewTitleRow);

                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        //creating a new row
                        ReadWirteCSV.CsvRow NewCSVRow = new ReadWirteCSV.CsvRow();

                        NewCSVRow.Add(TheWarehouseReportDataSet.warehouseinventory[intCounter].PartNumber);
                        NewCSVRow.Add(TheWarehouseReportDataSet.warehouseinventory[intCounter].JDEPartNumber);
                        NewCSVRow.Add(TheWarehouseReportDataSet.warehouseinventory[intCounter].PartDescription);
                        NewCSVRow.Add(TheWarehouseReportDataSet.warehouseinventory[intCounter].Warehouse);
                        NewCSVRow.Add(Convert.ToString(TheWarehouseReportDataSet.warehouseinventory[intCounter].Quantity));
                        

                        //writing the new row
                        TheReconCSV.WriteRow(NewCSVRow);
                    }

                }

                //output to user
                TheMessagesClass.InformationMessage("The File Has Been Saved to Your Selected location");

                //output to user
                TheMessagesClass.InformationMessage("The File Has Been Saved to Your Selected location");
            }
            catch (Exception Ex)
            {
                //message to user
                TheMessagesClass.ErrorMessage(Ex.ToString());

                //event log entry
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Warehouse Inventory Report // Export to CSV File // " + Ex.Message);
            }
        }

        private void btnFindPart_Click(object sender, RoutedEventArgs e)
        {
            //this will find the part information
            int intPartID;
            string strPartNumber;
            bool blnNotInteger;
            int intRecordsReturned = 0;
            
            try
            {
                strPartNumber = txtEnterPartNumber.Text;
                if(strPartNumber == "")
                {
                    TheMessagesClass.ErrorMessage("Part Information Was Not Entered");
                    return;
                }

                blnNotInteger = TheDataValidationClass.VerifyIntegerData(strPartNumber);

                gblnFullWarehouse = false;
               
                if(blnNotInteger == false)
                {
                    intPartID = Convert.ToInt32(strPartNumber);

                    TheFindPartByPartIDDataSet = ThePartNumberClass.FindPartByPartID(intPartID);

                    intRecordsReturned = TheFindPartByPartIDDataSet.FindPartByPartID.Rows.Count;

                    if(intRecordsReturned > 0)
                    {
                        TheFindWarehouseInventoryPartDataSet = TheInventoryClass.FindWarehouseInventoryPart(intPartID, gintWarehouseID);
                    }
                }

                if(intRecordsReturned == 0)
                {
                    TheFindPartByPartNumberDataSet = ThePartNumberClass.FindPartByPartNumber(strPartNumber);

                    intRecordsReturned = TheFindPartByPartNumberDataSet.FindPartByPartNumber.Rows.Count;

                    if(intRecordsReturned > 0)
                    {
                        intPartID = TheFindPartByPartNumberDataSet.FindPartByPartNumber[0].PartID;

                        TheFindWarehouseInventoryPartDataSet = TheInventoryClass.FindWarehouseInventoryPart(intPartID, gintWarehouseID);
                    }
                }

                if(intRecordsReturned == 0)
                {
                    TheFindPartByJDEPartNumberDataSet = ThePartNumberClass.FindPartByJDEPartNumber(strPartNumber);

                    intRecordsReturned = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber.Rows.Count;

                    if(intRecordsReturned > 0)
                    {
                        intPartID = TheFindPartByJDEPartNumberDataSet.FindPartByJDEPartNumber[0].PartID;

                        TheFindWarehouseInventoryPartDataSet = TheInventoryClass.FindWarehouseInventoryPart(intPartID, gintWarehouseID);
                    }
                }

                if(intRecordsReturned == 0)
                {
                    TheMessagesClass.InformationMessage("Part Not Found");
                    return;
                }

                FillWarehouseDataSet();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Warehouse Inventory Report // Find Part Button " + Ex.Message);
            }
        }
        private void FillWarehouseDataSet()
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            try
            {
                TheWarehouseReportDataSet.warehouseinventory.Rows.Clear();

                if (gblnFullWarehouse == true)
                {
                    intNumberOfRecords = TheFindWarehouseInventoryDataSet.FindWarehouseInventory.Rows.Count - 1;

                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        WarehouseReport.warehouseinventoryRow NewInventoryRow = TheWarehouseReportDataSet.warehouseinventory.NewwarehouseinventoryRow();

                        NewInventoryRow.JDEPartNumber = TheFindWarehouseInventoryDataSet.FindWarehouseInventory[intCounter].JDEPartNumber;
                        NewInventoryRow.PartNumber = TheFindWarehouseInventoryDataSet.FindWarehouseInventory[intCounter].PartNumber;
                        NewInventoryRow.PartDescription = TheFindWarehouseInventoryDataSet.FindWarehouseInventory[intCounter].PartDescription;
                        NewInventoryRow.Warehouse = TheFindWarehouseInventoryDataSet.FindWarehouseInventory[intCounter].FirstName;
                        NewInventoryRow.Quantity = TheFindWarehouseInventoryDataSet.FindWarehouseInventory[intCounter].Quantity;

                        TheWarehouseReportDataSet.warehouseinventory.Rows.Add(NewInventoryRow);
                    }
                }
                else if(gblnFullWarehouse == false)
                {
                    WarehouseReport.warehouseinventoryRow NewInventoryRow = TheWarehouseReportDataSet.warehouseinventory.NewwarehouseinventoryRow();

                    NewInventoryRow.JDEPartNumber = TheFindWarehouseInventoryPartDataSet.FindWarehouseInventoryPart[0].JDEPartNumber;
                    NewInventoryRow.PartNumber = TheFindWarehouseInventoryPartDataSet.FindWarehouseInventoryPart[0].PartNumber;
                    NewInventoryRow.PartDescription = TheFindWarehouseInventoryPartDataSet.FindWarehouseInventoryPart[0].PartDescription;
                    NewInventoryRow.Warehouse = TheFindWarehouseInventoryPartDataSet.FindWarehouseInventoryPart[0].FirstName;
                    NewInventoryRow.Quantity = TheFindWarehouseInventoryPartDataSet.FindWarehouseInventoryPart[0].Quantity;

                    TheWarehouseReportDataSet.warehouseinventory.Rows.Add(NewInventoryRow);
                }

                dgrInventory.ItemsSource = TheWarehouseReportDataSet.warehouseinventory;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Warehouse Inventory Report // Fill Warehouse Data Set" + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
            
        }
    }
}
