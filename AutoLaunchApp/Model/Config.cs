using AutoLaunchApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLaunchApp
{
    public class Config
    {
        internal static bool activeLogs = false;
        internal static bool activeAllRules = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
            JsonData.Savejson(fileType.Configuration);
        }

        public bool ActiveLogs
        {
            get
            {
                return activeLogs;
            }
            set
            {
                activeLogs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ActiveLogs"));
            }
        }

        public bool ActiveAllRules
        {
            get
            {
                return activeAllRules;
            }
            set
            {
                activeAllRules = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ActiveAllRules"));
            }
        }

        public Config(bool _activeLogs = false, bool _activeAllRules = false)
        {
            activeLogs = _activeLogs;
            activeAllRules = _activeAllRules;
        }
    }
}
