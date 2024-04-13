/* Title:           Project Report
 * Date:            5-2-17
 * Author:          Terry Holmes
 * 
 * Description:     This form is the project report */

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
using ProjectsDLL;
using InventoryDLL;
using IssuedPartsDLL;
using ReceivePartsDLL;
using BOMPartsDLL;
using CSVFileDLL;
using DataValidationDLL;
using NewEmployeeDLL;
using Microsoft.Win32;

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for ProjectReport.xaml
    /// </summary>
    public partial class ProjectReport : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        BOMPartsClass TheBOMPartsClass = new BOMPartsClass();
        ReadWirteCSV TheCSVClass = new ReadWirteCSV();
        EventLogClass TheEventLogClass = new EventLogClass();
        ProjectClass TheProjectClass = new ProjectClass();
        InventoryClass TheInventoryClass = new InventoryClass();
        IssuedPartsClass TheIssuePartsClass = new IssuedPartsClass();
        ReceivePartsClass TheReceivePartsClass = new ReceivePartsClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();

        //setting up the data sets
        ProjectReportDataSet TheProjectReportDataSet = new ProjectReportDataSet();
        FindProjectByAssignedProjectIDDataSet TheFindProjectByAssignedProjectIDDataSet = new FindProjectByAssignedProjectIDDataSet();
        FindProjectByProjectNameDataSet TheFindProjectByProjectNameDataSet = new FindProjectByProjectNameDataSet();
        FindReceivedPartsByProjectIDDataSet TheFindReceivedPartsByProjectIDDataSet = new FindReceivedPartsByProjectIDDataSet();
        FindIssuedPartsByProjectIDDataSet TheFindIssuedPartsByProjectIDDataSet = new FindIssuedPartsByProjectIDDataSet();
        FindBOMPartsByProjectIDDataSet TheFindBOMPartsByProjectIDDataSet = new FindBOMPartsByProjectIDDataSet();
        FindEmployeeByEmployeeIDDataSet TheFindEmployeeByEmployeeIDDataSet = new FindEmployeeByEmployeeIDDataSet();

        //setting global variables
        int gintReportCounter;
        int gintReportUpperLimit;
        
        public ProjectReport()
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

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtEnterProjectID.Focus();
            btnPrint.IsEnabled = false;
            btnExportCSVFile.IsEnabled = false;
            dgrResults.ItemsSource = TheProjectReportDataSet.projectreport;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            string strProjectName;
            int intProjectID = 0;
            int intRecordsReturned;
            bool blnItemNotFound = true;
            int intCounter;
            int intNumberOfRecords;
            int intReportCounter;
            bool blnItemFound;
            int intPartID;
            int intQuantity;
            int intWarehouseID;

            try
            {
                TheProjectReportDataSet.projectreport.Rows.Clear();

                gintReportCounter = 0;
                gintReportUpperLimit = 0;

                strProjectName = txtEnterProjectID.Text;
                if(strProjectName == "")
                {
                    TheMessagesClass.ErrorMessage("Project Information Was Not Entered");
                    return;
                }

                //checking for the project
                TheFindProjectByAssignedProjectIDDataSet = TheProjectClass.FindProjectByAssignedProjectID(strProjectName);

                intRecordsReturned = TheFindProjectByAssignedProjectIDDataSet.FindProjectByAssignedProjectID.Rows.Count;

                if(intRecordsReturned > 0)
                {
                    intProjectID = TheFindProjectByAssignedProjectIDDataSet.FindProjectByAssignedProjectID[0].ProjectID;
                    blnItemNotFound = false;
                }
                else if (intRecordsReturned == 0)
                {
                    TheFindProjectByProjectNameDataSet = TheProjectClass.FindProjectByProjectName(strProjectName);

                    intRecordsReturned = TheFindProjectByProjectNameDataSet.FindProjectByProjectName.Rows.Count;

                    if(intRecordsReturned > 0)
                    {
                        intProjectID = TheFindProjectByProjectNameDataSet.FindProjectByProjectName[0].ProjectID;
                        blnItemNotFound = false;
                    }
                }

                if(blnItemNotFound == true)
                {
                    TheMessagesClass.InformationMessage("The Project Was Not Found");
                    return;
                }

                TheFindReceivedPartsByProjectIDDataSet = TheReceivePartsClass.FindReceivedPartsByProjectID(intProjectID);

                intNumberOfRecords = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        blnItemFound = false;
                        intPartID = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].PartID;
                        intQuantity = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].Quantity;
                       
                        if(gintReportCounter > 0)
                        {
                            for(intReportCounter = 0; intReportCounter <= gintReportUpperLimit; intReportCounter++)
                            {
                                if(intPartID == TheProjectReportDataSet.projectreport[intReportCounter].PartID)
                                {
                                    TheProjectReportDataSet.projectreport[intReportCounter].Received += intQuantity;
                                    blnItemFound = true;
                                }
                            }
                        }

                        if(blnItemFound == false)
                        {
                            ProjectReportDataSet.projectreportRow NewPartRow = TheProjectReportDataSet.projectreport.NewprojectreportRow();

                            NewPartRow.Description = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].PartDescription;
                            NewPartRow.Issued = 0;
                            NewPartRow.JDEPartNumber = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].JDEPartNumber;
                            NewPartRow.PartID = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].PartID;
                            NewPartRow.PartNumber = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].PartNumber;
                            NewPartRow.Received = intQuantity;
                            NewPartRow.Reported = 0;
                            intWarehouseID = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].WarehouseID;
                            TheFindEmployeeByEmployeeIDDataSet = TheEmployeeClass.FindEmployeeByEmployeeID(intWarehouseID);
                            NewPartRow.Warehouse = TheFindEmployeeByEmployeeIDDataSet.FindEmployeeByEmployeeID[0].FirstName;

                            TheProjectReportDataSet.projectreport.Rows.Add(NewPartRow);
                            gintReportUpperLimit = gintReportCounter;
                            gintReportCounter++;
                        }
                    }
                }

                TheFindIssuedPartsByProjectIDDataSet = TheIssuePartsClass.FindIssuedPartsByProjectID(intProjectID);

                intNumberOfRecords = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        blnItemFound = false;
                        intPartID = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].PartID;
                        intQuantity = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].Quantity;

                        if (gintReportCounter > 0)
                        {
                            for (intReportCounter = 0; intReportCounter <= gintReportUpperLimit; intReportCounter++)
                            {
                                if (intPartID == TheProjectReportDataSet.projectreport[intReportCounter].PartID)
                                {
                                    TheProjectReportDataSet.projectreport[intReportCounter].Issued += intQuantity;
                                    blnItemFound = true;
                                }
                            }
                        }

                        if (blnItemFound == false)
                        {
                            ProjectReportDataSet.projectreportRow NewPartRow = TheProjectReportDataSet.projectreport.NewprojectreportRow();

                            NewPartRow.Description = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].PartDescription;
                            NewPartRow.Issued = intQuantity;
                            NewPartRow.JDEPartNumber = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].JDEPartNumber;
                            NewPartRow.PartID = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].PartID;
                            NewPartRow.PartNumber = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].PartNumber;
                            NewPartRow.Received = 0;
                            NewPartRow.Reported = 0;
                            intWarehouseID = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].WarehouseID;
                            TheFindEmployeeByEmployeeIDDataSet = TheEmployeeClass.FindEmployeeByEmployeeID(intWarehouseID);
                            NewPartRow.Warehouse = TheFindEmployeeByEmployeeIDDataSet.FindEmployeeByEmployeeID[0].FirstName;

                            TheProjectReportDataSet.projectreport.Rows.Add(NewPartRow);
                            gintReportUpperLimit = gintReportCounter;
                            gintReportCounter++;
                        }
                    }
                }

                TheFindBOMPartsByProjectIDDataSet = TheBOMPartsClass.FindBOMPartsByProjectID(intProjectID);

                intNumberOfRecords = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        blnItemFound = false;
                        intPartID = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].PartID;
                        intQuantity = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].Quantity;

                        if (gintReportCounter > 0)
                        {
                            for (intReportCounter = 0; intReportCounter <= gintReportUpperLimit; intReportCounter++)
                            {
                                if (intPartID == TheProjectReportDataSet.projectreport[intReportCounter].PartID)
                                {
                                    TheProjectReportDataSet.projectreport[intReportCounter].Issued += intQuantity;
                                    blnItemFound = true;
                                }
                            }
                        }

                        if (blnItemFound == false)
                        {
                            ProjectReportDataSet.projectreportRow NewPartRow = TheProjectReportDataSet.projectreport.NewprojectreportRow();

                            NewPartRow.Description = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].PartDescription;
                            NewPartRow.Issued = 0;
                            NewPartRow.JDEPartNumber = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].JDEPartNumber;
                            NewPartRow.PartID = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].PartID;
                            NewPartRow.PartNumber = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].PartNumber;
                            NewPartRow.Received = 0;
                            NewPartRow.Reported = intQuantity;
                            intWarehouseID = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].WarehouseID;
                            TheFindEmployeeByEmployeeIDDataSet = TheEmployeeClass.FindEmployeeByEmployeeID(intWarehouseID);
                            NewPartRow.Warehouse = TheFindEmployeeByEmployeeIDDataSet.FindEmployeeByEmployeeID[0].FirstName;

                            TheProjectReportDataSet.projectreport.Rows.Add(NewPartRow);
                            gintReportUpperLimit = gintReportCounter;
                            gintReportCounter++;
                        }
                    }
                }

                dgrResults.ItemsSource = TheProjectReportDataSet.projectreport;
                btnPrint.IsEnabled = true;
                btnExportCSVFile.IsEnabled = true;
                
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Project Report // Search Button " + Ex.Message);

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
                    intColumns = TheProjectReportDataSet.projectreport.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        ProjectReportTable.Columns.Add(new TableColumn());
                    }
                    ProjectReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Project Report For " + txtEnterProjectID.Text))));
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
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Received"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Issued"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Reported"))));
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

                    intNumberOfRecords = TheProjectReportDataSet.projectreport.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheProjectReportDataSet.projectreport[intReportRowCounter][intColumnCounter].ToString()))));


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
                    pdProjectReport.PrintDocument(((IDocumentPaginatorSource)fdProjectReport).DocumentPaginator, "MSR Report For " + txtEnterProjectID.Text);
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // MSR Report // Print Button " + Ex.Message);
            }
        }

        private void btnExportCSVFile_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
