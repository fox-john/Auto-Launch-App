using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoLaunchApp
{
    class Utils
    {
        internal static DateTime lastScreenChanged;

        /// <summary>
        /// Init taskbar
        /// </summary>
        internal static TaskbarIcon InitTaskBar()
        {
            TaskbarIcon taskbar = new TaskbarIcon();
            taskbar.Icon = new System.Drawing.Icon("logo.ico");
            taskbar.ToolTipText = "Auto Launch App";

            taskbar.ContextMenu = new ContextMenu();

            return taskbar;
        }

        /// <summary>
        /// Get parent by Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static T GetParentByType<T>(object item) where T : class
        {
            DependencyObject entity = (DependencyObject)item;

            while (entity != null && !(entity is T))
            {
                entity = VisualTreeHelper.GetParent(entity);
            }

            return entity as T;
        }

        /// <summary>
        /// Find row index
        /// </summary>
        /// <param name="row"></param>
        internal static int FindRowIndex(object item)
        {
            DataGridRow row = GetParentByType<DataGridRow>(item);

            // convert DataGridRow element to DataGrid
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;

            // get index of datagrid row
            int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);

            return index;
        }

        /// <summary>
        /// Get specifically cell
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        internal static List<DataGridCell> GetCell(object sender)
        {
            DataGrid dataGrid = GetParentByType<DataGrid>(sender);
            DataGridRow row = GetParentByType<DataGridRow>(sender);

            List<DataGridCell> cellsList = new List<DataGridCell>();

            for(int i = 0; i < dataGrid.Columns.Count(); i++)
            {
                DataGridCell cell = dataGrid.Columns[i].GetCellContent(row).Parent as DataGridCell;
                cellsList.Add(cell);
            }

            return cellsList;
        }

        /// <summary>
        /// Get TrackedApp on this row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        internal static TrackedApp GetTrackedApp(object sender)
        {
            DataGrid grid = GetParentByType<DataGrid>(sender);
            DataGridRow row = GetParentByType<DataGridRow>(sender);

            int rowIndex = Utils.FindRowIndex(row);
            TrackedApp trackedApp = grid.Items.GetItemAt(rowIndex) as TrackedApp;

            if (trackedApp != null)
                return trackedApp;
            else return null;
        }

        /// <summary>
        /// Get value of one cell
        /// </summary>
        /// <param name="row"></param>
        /// <param name="cell"></param>
        internal static object ExtractCellValue(DataGridRow row, DataGridCell cell)
        {
            // find the column that this cell belongs to
            DataGridBoundColumn col = cell.Column as DataGridBoundColumn;

            // find the property that this column is bound to
            Binding binding = col.Binding as Binding;
            string boundPropertyName = binding.Path.Path;

            // find the object that is related to this row
            object data = row.Item;

            // extract the property value
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data);

            PropertyDescriptor property = properties[boundPropertyName];
            object value = property.GetValue(data);

            return value;
        }
    }
}
