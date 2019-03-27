using AutoLaunchApp.Model;
using AutoLaunchApp.view;
using displayConvert;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace AutoLaunchApp
{
    public class TrackedApp : INotifyPropertyChanged
    {
        private static List<Thread> ThreadList = new List<Thread>();

        private string trackedAppName;
        private StateType state;
        private List<ActionApp> launchAppList;
        private List<ActionApp> closeAppList;
        private int screenResolutionIndex;
        private bool isActive;
        private Thread thread;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
            JsonData.Savejson(fileType.trackedList);
        }

        public string TrackedAppName
        {
            get
            {
                return trackedAppName;
            }
            set
            {
                trackedAppName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TrackedAppName"));
            }
        }

        public int State
        {
            get
            {
                return (int)state;
            }
            set
            {
                state = (StateType)value;
                OnPropertyChanged(new PropertyChangedEventArgs("State"));
            }
        }

        public List<ActionApp> LaunchAppList
        {
            get
            {
                return launchAppList;
            }
            set
            {
                launchAppList = value;
                OnPropertyChanged(new PropertyChangedEventArgs("LaunchAppList"));
            }
        }

        public List<ActionApp> CloseAppList
        {
            get
            {
                return closeAppList;
            }
            set
            {
                closeAppList = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CloseAppList"));
            }
        }

        public int ScreenResolutionIndex
        {
            get
            {
                return screenResolutionIndex;
            }
            set
            {
                screenResolutionIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ScreenResolution"));
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsActive"));
                ActiveTracking();
            }
        }

        public TrackedApp(string _trackedAppName, StateType _state, List<ActionApp> _launchAppList, List<ActionApp> _closeAppList, int _screenResolutionIndex, bool _isActive)
        {
            TrackedAppName = _trackedAppName;
            State = (int)_state;
            LaunchAppList = _launchAppList;
            CloseAppList = _closeAppList;
            ScreenResolutionIndex = _screenResolutionIndex;
            IsActive = _isActive;

            JsonData.AddTrackedApp(this);
        }

        /// <summary>
        /// Active tracking for one software in another thread
        /// </summary>
        public void ActiveTracking()
        {
            if (isActive)
            {
                // create new thread
                thread = new Thread(() =>
                {
                    new LogWriter(LogWriter.LogType.INFORMATION, "Thread detection started (" + trackedAppName + ")");

                    // wile tracking is active
                    while (isActive)
                    {
                        // check all process
                        foreach (Process process in Process.GetProcesses())
                        {
                            // if process detected
                            if (process.ProcessName == trackedAppName.Substring(0, trackedAppName.Length -4))
                            {
                                new LogWriter(LogWriter.LogType.INFORMATION, "Process " + trackedAppName + " is started");

                                // wait application exited
                                if (State == (int)StateType.atExit)
                                {
                                    while (isActive && !process.HasExited)
                                    {
                                        new LogWriter(LogWriter.LogType.INFORMATION, "Process " + trackedAppName + " is stopped");
                                        Thread.Sleep(1000);
                                    }
                                }

                                // launch all applications in launchAppList
                                foreach (ActionApp app in launchAppList)
                                {
                                    new LogWriter(LogWriter.LogType.INFORMATION, "application " + app.AppName + " is launched");
                                    StartProcess(app.AppName, app.Arguments);
                                }

                                // close all application in closeAppList
                                foreach (ActionApp app in closeAppList)
                                {
                                    StopProcess(app.AppName.Substring(0, app.AppName.Length - 4));
                                }

                                ChangeScreenResolution();

                                // wait application exited
                                while (isActive && !process.HasExited)
                                {
                                    new LogWriter(LogWriter.LogType.INFORMATION, "Process " + trackedAppName + " is stopped");
                                    Thread.Sleep(1000);
                                }
                            }
                        }

                        Thread.Sleep(1000);
                    }
                });
                thread.Start();
                ThreadList.Add(thread);
            }
            else
            {
                // if thread exist, join & abort it
                if (thread != null)
                {
                    thread.Join();
                    thread.Abort();
                    ThreadList.Remove(thread);

                    new LogWriter(LogWriter.LogType.INFORMATION, "Thread detection stopped (" + trackedAppName + ")");
                }
            }
        }

        /// <summary>
        /// Start process if tracked Soft is started
        /// </summary>
        /// <param name="exeFileName"></param>
        private static void StartProcess(string _exeFileName, string _arguments)
        {
            try
            {
                using (Process myProcess = new Process())
                {
                    new LogWriter(LogWriter.LogType.INFORMATION, "Process " + _exeFileName + " executed");
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    myProcess.StartInfo.FileName = _exeFileName;
                    myProcess.StartInfo.Arguments = _arguments;

                    myProcess.Start();
                }
            }
            catch(Exception ex)
            {
                new LogWriter(LogWriter.LogType.ERROR, ex.ToString());
            }

        }

        /// <summary>
        /// Start process if tracked Soft is stopped
        /// </summary>
        /// <param name="exeFileName"></param>
        private static void StopProcess(string _exeFileName)
        {
            try
            {
                bool processFound = false;

                foreach (Process process in Process.GetProcessesByName(_exeFileName))
                {
                    if (process.ProcessName == _exeFileName)
                    {
                        processFound = true;
                        process.Kill();
                        return;
                    }
                }

                if(!processFound) new LogWriter(LogWriter.LogType.INFORMATION, "application " + _exeFileName + " isn't found and isn't stopped");
                else new LogWriter(LogWriter.LogType.INFORMATION, "Process " + _exeFileName + " as stopped");
            }
            catch(Exception ex)
            {
                new LogWriter(LogWriter.LogType.ERROR, ex.ToString());
            }
        }

        private void ChangeScreenResolution()
        {
            if(Utils.lastScreenChanged == null) Utils.lastScreenChanged = DateTime.Now;
            else
            {
                if(DateTime.Now.Subtract(Utils.lastScreenChanged).Seconds > 1)
                {
                    Utils.lastScreenChanged = DateTime.Now;

                    DisplayInfos currentDisplay = DisplayInfos.GetCurrentMode();

                    // get index of current display in displayList
                    int currentDisplayIndex = MainWindow.displayList.FindIndex(display => (display.width == currentDisplay.width) && (display.height == currentDisplay.height) && (display.color == currentDisplay.color));

                    DisplayInfos selectedDisplay = MainWindow.displayList[screenResolutionIndex];

                    // if current display index != displayIndex selected
                    if (screenResolutionIndex != currentDisplayIndex)
                    {
                        new LogWriter(LogWriter.LogType.INFORMATION, "Change display setting from " +
                            currentDisplay.width + "x" + currentDisplay.height + "(" + currentDisplay.color + ") to " +
                            selectedDisplay.width + "x" + selectedDisplay.height + "(" + selectedDisplay.color + ")");

                        ChangeResolutionResult result = (ChangeResolutionResult)DisplayInfos.ChangeDisplaySettings(selectedDisplay.width, selectedDisplay.height, selectedDisplay.color);

                        new LogWriter(LogWriter.LogType.WARNING, "Resolution change with this code : " + result.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Close all thread in ThreadList
        /// </summary>
        public static void CloseAllThreads()
        {
            ThreadList.ForEach(thread => thread.Abort());
        }
    }

    /// <summary>
    /// State of application tracked
    /// </summary>
    public enum StateType
    {
        noState = 0,
        atLaunch = 1,
        atExit = 2,
    }

    /// <summary>
    /// Result after change resolution
    /// </summary>
    public enum ChangeResolutionResult
    {
        DISP_CHANGE_SUCCESSFUL = 0,
        DISP_CHANGE_BADMODE = -2,
        DISP_CHANGE_FAILED = -1,
        DISP_CHANGE_RESTART = 1,
        ENUM_CURRENT_SETTINGS = -1,
        ENUM_REGISTRY_SETTINGS = -2,
    }
}
