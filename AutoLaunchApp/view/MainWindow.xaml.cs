using AutoLaunchApp.Model;
using displayConvert;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AutoLaunchApp.view
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Dictionary<int, Window> windowList = new Dictionary<int, Window>();
        public static List<DisplayInfos> displayList = DisplayInfos.GetAvailableModes();
        public static Config config = (Config)JsonData.LoadFile(fileType.Configuration);
        public static bool closeMainWindow = false;
        public WindowState state = WindowState.Normal;
        public TaskbarIcon AppTrayIcon;

        /// <summary>
        /// Initialize components and tray icon
        /// </summary>
        public MainWindow()
        {
            JsonData.CheckIfJsonExist();
            LogWriter.CheckIfLogFileExist();

            InitializeComponent();

            LoadOver();
        }

        public void LoadOver()
        {
            AppTrayIcon = Utils.InitTaskBar();
            MenuItem item = new MenuItem();
            item.Header = "Exit";
            item.Click += CloseAppWithTrayIcon;

            AppTrayIcon.ContextMenu.Items.Add(item);

            List<TrackedApp> trackedList = (List<TrackedApp>)JsonData.LoadFile(fileType.trackedList);
            trackedList.ForEach(app => trackedListGrid.Items.Add(app));

            this.Closing += ClickOnCloseButton;
            AppTrayIcon.TrayMouseDoubleClick += DoubleClickOnTrayIcon;

            this.state = WindowState.Minimized;
            this.Hide();
        }

        private void ActiveLogs(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;

            if (checkbox.IsChecked == true)
                config.ActiveLogs = true;
            else config.ActiveLogs = false;
        }

        /// <summary>
        /// Add one app to tracking list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTrackedApp(object sender, RoutedEventArgs e)
        {
            DisplayInfos currentDisplay = DisplayInfos.GetCurrentMode();
            int currentModeIndex = displayList.FindIndex(display => (display.width == currentDisplay.width) && (display.height == currentDisplay.height) && (display.color == currentDisplay.color));

            TrackedApp newTrackedApp = new TrackedApp("select .exe file", StateType.noState, new List<ActionApp>(), new List<ActionApp>(), currentModeIndex, false);

            trackedListGrid.Items.Add(newTrackedApp);
        }

        /// <summary>
        /// Select app to track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectTrackedApp(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "executable file (*.exe)|*.exe";
            OFD.ShowDialog();

            if(OFD.SafeFileName.Length > 0)
            {
                TrackedApp trackedApp = Utils.GetTrackedApp(sender);
                trackedApp.TrackedAppName = OFD.SafeFileName;
            }
        }

        /// <summary>
        /// Switch state of tracked app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchState(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            TrackedApp trackedApp = Utils.GetTrackedApp(sender);

            trackedApp.State = comboBox.SelectedIndex;
        }

        /// <summary>
        /// Edit infos off tracked app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditTrackedAppDetails(object sender, RoutedEventArgs e)
        {
            DataGridRow row = Utils.GetParentByType<DataGridRow>(sender);
            int rowIndex = Utils.FindRowIndex(row);
            TrackedApp trackedApp = Utils.GetTrackedApp(sender);

            if (!windowList.TryGetValue(row.GetIndex(), out Window window))
            {
                Details detailsWindow = new Details(rowIndex, trackedApp);
                detailsWindow.Title = "Set associated actions" + (trackedApp.TrackedAppName != "select .exe file" ? " (" + trackedApp.TrackedAppName + ")" : "");

                windowList.Add(row.GetIndex(), detailsWindow);
                detailsWindow.Show();
            }
            else
            {
                window.Activate();
            }
        }

        /// <summary>
        /// enable or disable state of tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchIsActive(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            TrackedApp trackedApp = Utils.GetTrackedApp(sender);

            if(checkbox.IsChecked == true)
            {
                if(trackedApp.TrackedAppName == "select .exe file")
                {
                    checkbox.IsChecked = false;
                    Error errorWindow = new Error("You need to track application before active this row");
                    errorWindow.Show();
                }
                else if(trackedApp.State == (int)StateType.noState)
                {
                    checkbox.IsChecked = false;
                    Error errorWindow = new Error("You need to set state before active this row");
                    errorWindow.Show();
                    
                }
                else
                {
                    trackedApp.IsActive = true;
                }
            }
            else
            {
                trackedApp.IsActive = false;
            }

            foreach (DataGridCell cell in Utils.GetCell(sender))
            {
                if (cell.TabIndex != 0) cell.IsEnabled = !trackedApp.IsActive;
            }
        }

        /// <summary>
        /// remove one tracked app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveTrackedApp(object sender, RoutedEventArgs e)
        {
            int rowIndex = Utils.FindRowIndex(sender);
            TrackedApp trackedApp = Utils.GetTrackedApp(sender);

            trackedListGrid.Items.RemoveAt(rowIndex);
            JsonData.RemoveTrackedApp(trackedApp);
        }

        private void DoubleClickOnTrayIcon(object sender, RoutedEventArgs e)
        {
            if(this.state == WindowState.Minimized)
            {
                this.state = WindowState.Normal;
                this.Show();

                activeLogsCheckbox.IsChecked = config.ActiveLogs;
            }
        }
       
        /// <summary>
        /// Close AutoLaunchApp if click on tray sub icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseAppWithTrayIcon(object sender, RoutedEventArgs e)
        {
            TrackedApp.CloseAllThreads();
            JsonData.Savejson(fileType.trackedList);
            JsonData.Savejson(fileType.Configuration);
            closeMainWindow = true;

            foreach (Window window in windowList.Values)
            {
                window.Close();
            }

            AppTrayIcon.Dispose();

            foreach (Process process in Process.GetProcessesByName("AutoLaunchApp"))
            {
                new LogWriter(LogWriter.LogType.ERROR, "double process AutoLaunchApp stopped");
                process.Kill();
            }

            this.Close();
        }

        /// <summary>
        /// Cancel close app and hide it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickOnCloseButton(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!closeMainWindow)
                e.Cancel = true;

            if (this.state == WindowState.Normal)
            {
                this.state = WindowState.Minimized;
                this.Hide();
            }
        }
    }
}
