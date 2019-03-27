using AutoLaunchApp.Model;
using System.ComponentModel;

namespace AutoLaunchApp
{
    public class ActionApp
    {
        private string appName;
        private string arguments;
        private ActionType type;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
            JsonData.Savejson(fileType.trackedList);
        }

        public string AppName
        {
            get
            {
                return appName;
            }
            set
            {
                appName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("AppName-" + type.ToString()));
            }
        }

        public string Arguments
        {
            get
            {
                return arguments;
            }
            set
            {
                arguments = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Arguments"));
            }
        }

        public int Type
        {
            get
            {
                return (int)type;
            }
            set
            {
                type = (ActionType)value;
                OnPropertyChanged(new PropertyChangedEventArgs("type"));
            }
        }

        public ActionApp(ActionType _type, string _appName = null, string _arguments = null)
        {
            Type = (int)_type;
            AppName = _appName;
            Arguments = _arguments;
        }
    }

    public enum ActionType
    {
        launchApp = 1,
        closeApp = 2,
    }
}
