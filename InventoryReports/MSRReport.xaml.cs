/* Title:           MSR Report
 * Date:            5-5-17
 * Author:          Terry Holmes
 * 
 * Description:     this will run a report for MSRs */

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
using ReceivePartsDLL;
using CSVFileDLL;
using Microsoft.Win32;
using System.Printing;

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for MSRReport.xaml
    /// </summary>
    public partial class MSRReport : Window
    {
        //setting up the class
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        ReceivePartsClass TheReceivePartsClass = new ReceivePartsClass();
        ReadWirteCSV TheCSVClass = new ReadWirteCSV();

        //setting up the data 
        findReceivedPartsByPONumberDataSet TheFindReceivePartsByPONumberDataSet = new findReceivedPartsByPONumberDataSet();
                
        //setting global variables
        public MSRReport()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
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
            //setting up data
            try
            {
                dgrMSRResults.ItemsSource = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber;

                txtEnterMSRNumber.Focus();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // MSR Report // Window Loaded " + Ex.Message);

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
                    intColumns = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber.Columns.Count;

                    for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                    {
                        ProjectReportTable.Columns.Add(new TableColumn());
                    }
                    ProjectReportTable.RowGroups.Add(new TableRowGroup());

                    //Title row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    TableRow newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("MSR Report For " + txtEnterMSRNumber.Text))));
                    newTableRow.Cells[0].FontSize = 16;
                    newTableRow.Cells[0].FontFamily = new FontFamily("Times New Roman");
                    newTableRow.Cells[0].ColumnSpan = intColumns;
                    newTableRow.Cells[0].TextAlignment = TextAlignment.Center;
                    newTableRow.Cells[0].Padding = new Thickness(0, 0, 0, 20);

                    //Header Row
                    ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                    intCurrentRow++;
                    newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("JDE Part Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Description"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("First Name"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Last Name"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Warehouse ID"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("DID Number"))));
                    newTableRow.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
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

                    intNumberOfRecords = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber.Rows.Count;

                    //Data, Format Data

                    for (int intReportRowCounter = 0; intReportRowCounter < intNumberOfRecords; intReportRowCounter++)
                    {
                        ProjectReportTable.RowGroups[0].Rows.Add(new TableRow());
                        intCurrentRow++;
                        newTableRow = ProjectReportTable.RowGroups[0].Rows[intCurrentRow];
                        for (int intColumnCounter = 0; intColumnCounter < intColumns; intColumnCounter++)
                        {
                            newTableRow.Cells.Add(new TableCell(new Paragraph(new Run(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intReportRowCounter][intColumnCounter].ToString()))));


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
                    pdProjectReport.PrintDocument(((IDocumentPaginatorSource)fdProjectReport).DocumentPaginator, "MSR Report For " + txtEnterMSRNumber.Text);
                    intCurrentRow = 0;

                }
            }
            catch (Exception Ex)
            {
                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Report // MSR Report // Print Button " + Ex.Message);
            }
        }

        private void btnFindMSR_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intRecoredsReturned;
            string strMSRNumber;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                //data validation
                strMSRNumber = txtEnterMSRNumber.Text;
                if(strMSRNumber == "")
                {
                    TheMessagesClass.ErrorMessage("MSR Number Was Not Entered");
                    return;
                }

                TheFindReceivePartsByPONumberDataSet = TheReceivePartsClass.FindReceivedPartsByPONumber(strMSRNumber);

                intRecoredsReturned = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber.Rows.Count;

                if(intRecoredsReturned == -1)
                {
                    TheMessagesClass.InformationMessage("MSR Number Was Not Found");
                    return;
                }

                dgrMSRResults.ItemsSource = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // MSR Reports // Find MSR Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
        

        private void btnExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            //try catch for exceptions
            try
            {
                //creating the file writer
                SaveFileDialog file = new SaveFileDialog();
                file.ShowDialog();
                ReadWirteCSV.CsvFileWriter TheReconCSV = new ReadWirteCSV.CsvFileWriter(file.FileName + ".csv");

                intNumberOfRecords = TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber.Rows.Count - 1;

                //calling the writer
                using (TheReconCSV)
                {
                    ReadWirteCSV.CsvRow NewTitleRow = new ReadWirteCSV.CsvRow();

                    NewTitleRow.Add("DID Number");
                    NewTitleRow.Add("Part Number");
                    NewTitleRow.Add("JDE Part Number");
                    NewTitleRow.Add("Description");
                    NewTitleRow.Add("Quantity");
                    NewTitleRow.Add("Warehouse ID");


                    //writing the new row
                    TheReconCSV.WriteRow(NewTitleRow);

                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        //creating a new row
                        ReadWirteCSV.CsvRow NewCSVRow = new ReadWirteCSV.CsvRow();

                        NewCSVRow.Add(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].AssignedProjectID);
                        NewCSVRow.Add(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].PartNumber);
                        NewCSVRow.Add(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].JDEPartNumber);
                        NewCSVRow.Add(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].PartDescription);
                        NewCSVRow.Add(Convert.ToString(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].Quantity));
                        NewCSVRow.Add(Convert.ToString(TheFindReceivePartsByPONumberDataSet.FindReceivedPartsByPONumber[intCounter].WarehouseID));

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
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Inventory Reports // Project Inventory Report // Export to CSV File // " + Ex.Message);
            }
        }
    }
}
