using AutoLaunchApp.Model;
using displayConvert;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AutoLaunchApp.view
{
    /// <summary>
    /// Logique d'interaction pour Details.xaml
    /// </summary>
    public partial class Details : Window
    {
        public TrackedApp trackedApp;
        public int rowId;

        public Details(int _rowId, TrackedApp _trackedApp)
        {
            InitializeComponent();

            trackedApp = _trackedApp;
            rowId = _rowId;

            trackedApp.LaunchAppList.ForEach(app => launchAppListGrid.Items.Add(app));
            trackedApp.CloseAppList.ForEach(app => closeAppListGrid.Items.Add(app));

            this.Closing += CloseDetailsWindow;
        }

        /// <summary>
        /// Add one app to launch or close list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAppAction(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Name == "addLaunchAppButton")
            {
                ActionApp newApp = new ActionApp(ActionType.launchApp, "Select an application to launch");
                trackedApp.LaunchAppList.Add(newApp);
                launchAppListGrid.Items.Add(newApp);
            }
            else if (button.Name == "addCloseAppButton")
            {
                ActionApp newApp = new ActionApp(ActionType.closeApp, "Select an application to close");
                trackedApp.CloseAppList.Add(newApp);
                closeAppListGrid.Items.Add(newApp);
            }

            JsonData.Save();
        }
        
        /// <summary>
        /// Select one app to launch or close list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAppAction(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "executable file (*.exe)|*.exe";
            OFD.ShowDialog();

            int rowIndex = Utils.FindRowIndex(sender);

            if(OFD.SafeFileName.Length > 0)
            {
                if (Utils.GetParentByType<DataGrid>(sender).Name == "launchAppListGrid")
                {
                    trackedApp.LaunchAppList[rowIndex].AppName = OFD.FileName;
                    launchAppListGrid.Items.Refresh();
                }
                else
                {
                    trackedApp.CloseAppList[rowIndex].AppName = OFD.SafeFileName;
                    closeAppListGrid.Items.Refresh();
                }
            }
        }

        /// <summary>
        /// Edit arguments for launched app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditArgs(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int rowIndex = Utils.FindRowIndex(sender);

            trackedApp.LaunchAppList[rowIndex].Arguments = textBox.Text;
        }

        /// <summary>
        /// remove one app to launch or close list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveAppAction(object sender, RoutedEventArgs e)
        {
            int rowIndex = Utils.FindRowIndex(sender);

            if(Utils.GetParentByType<DataGrid>(sender).Name == "launchAppListGrid")
            {
                launchAppListGrid.Items.RemoveAt(rowIndex);
                trackedApp.LaunchAppList.RemoveAt(rowIndex);
            }
            else
            {
                closeAppListGrid.Items.RemoveAt(rowIndex);
                trackedApp.CloseAppList.RemoveAt(rowIndex);
            }

            JsonData.Save();
        }

        /// <summary>
        /// Load screen resolution list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadScreenResolutionList(object sender, RoutedEventArgs e)
        {
            List<DisplayInfos> displayList = DisplayInfos.GetAvailableModes();

            displayList.ForEach(display => ScreenResolutionList.Items.Add(display.width + "x" + display.height + " (" + display.color + "bits)"));

            ScreenResolutionList.SelectedIndex = trackedApp.ScreenResolutionIndex;
        }

        private void SelectScreenResolution(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            trackedApp.ScreenResolutionIndex = comboBox.SelectedIndex;
        }

        private void CloseDetailsWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!MainWindow.closeMainWindow)
                MainWindow.windowList.Remove(this.rowId);
        }
    }
}
