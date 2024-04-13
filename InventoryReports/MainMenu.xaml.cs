/* Title:           Main Menu
 * Date:            4-5-17
 * Author:          Terry Holmes
 * 
 * Description:     This is the main menu for the application */

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

namespace InventoryReports
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();

        public MainMenu()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this will close the program
            TheMessagesClass.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        
        private void btnWarehouseInventory_Click(object sender, RoutedEventArgs e)
        {
            WarehouseInventoryReport WarehouseInventoryReport = new WarehouseInventoryReport();
            WarehouseInventoryReport.Show();
            Close();
        }

        private void btnProjectReports_Click(object sender, RoutedEventArgs e)
        {
            ProjectReport ProjectReport = new ProjectReport();
            ProjectReport.Show();
            Close();
        }

        private void btnMSRReport_Click(object sender, RoutedEventArgs e)
        {
            MSRReport MSRReport = new MSRReport();
            MSRReport.Show();
            Close();
        }

        private void btnWarehousePartReport_Click(object sender, RoutedEventArgs e)
        {
            PartNumberWarehouseReport PartNumberWarehouseReport = new PartNumberWarehouseReport();
            PartNumberWarehouseReport.Show();
            Close();
        }

        private void btnCostingReports_Click(object sender, RoutedEventArgs e)
        {
            CostingReports CostingReports = new CostingReports();
            CostingReports.Show();
            Close();
        }
    }
}
