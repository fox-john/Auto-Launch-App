using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AutoLaunchApp.Model
{
    public static class JsonData
    {
        private static List<TrackedApp> trackedApps = new List<TrackedApp>();
        private static Config config = new Config();
        public static string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AutoLaunchApp";
        private static readonly string trackedListFile = Path.Combine(appDataFolder, "list.json");
        private static readonly string configfile = Path.Combine(appDataFolder, "config.json");
        public static bool fileLocked = false; 

        public static List<TrackedApp> TrackedApps
        {
            get { return trackedApps; }
            set { trackedApps = value; }
        }

        public static void CheckIfJsonExist()
        {
            // Check if AppData app folder and files exist
            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            // Check if config file exist
            if (!File.Exists(configfile))
            {
                using(StreamWriter sr = File.CreateText(configfile))
                {
                    sr.WriteLine(JsonConvert.SerializeObject(config));
                }
            }

            // Check if trackedList json file exist
            if (!File.Exists(trackedListFile))
            {
                using (StreamWriter sr = File.CreateText(trackedListFile))
                {
                    sr.WriteLine(JsonConvert.SerializeObject(trackedApps));
                }
            }
        }

        /// <summary>
        /// Add one trackedApp to list
        /// </summary>
        /// <param name="_trackedApp"></param>
        public static void AddTrackedApp(TrackedApp _trackedApp)
        {
            trackedApps.Add(_trackedApp);
            Savejson(fileType.trackedList);
        }

        /// <summary>
        /// Remove one app to trackedApp
        /// </summary>
        /// <param name="_trackedApp"></param>
        /// <returns></returns>
        public static void RemoveTrackedApp(TrackedApp _trackedApp)
        {
            trackedApps.Remove(_trackedApp);
            Savejson(fileType.trackedList);
        }

        /// <summary>
        /// Get object and save to Json file
        /// </summary>
        public static void Savejson(fileType type)
        {
            if(!fileLocked)
            {
                string file = "";

                if (type == fileType.trackedList)
                    file = trackedListFile;
                else if (type == fileType.Configuration)
                    file = configfile;

                using (StreamWriter sw = new StreamWriter(file, false))
                {
                    if (type == fileType.trackedList)
                        sw.Write(JsonConvert.SerializeObject(trackedApps));
                    else if (type == fileType.Configuration)
                        sw.Write(JsonConvert.SerializeObject(config));
                }
            }
        }

        /// <summary>
        /// Load Json File and convert to object
        /// </summary>
        public static object LoadFile(fileType type)
        {
            string file = "";

            if (type == fileType.trackedList)
                file = trackedListFile;
            else if (type == fileType.Configuration)
                file = configfile;

            if(file != "")
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    fileLocked = true;
                    string json = sr.ReadToEnd();

                    if (type == fileType.trackedList)
                    {
                        trackedApps = JsonConvert.DeserializeObject<List<TrackedApp>>(json);
                        fileLocked = false;
                        return trackedApps;
                    }
                    else if (type == fileType.Configuration)
                    {
                        config = JsonConvert.DeserializeObject<Config>(json);
                        fileLocked = false;
                        return config;
                    }
                    else
                    {
                        fileLocked = false;
                        return null;
                    }
                }
            }
            else return null;
        }
    }

    public enum fileType
    {
        trackedList = 1,
        Configuration = 2
    }
}
