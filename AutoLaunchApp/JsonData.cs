using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutoLaunchApp.Model
{
    public static class JsonData
    {
        private static List<TrackedApp> trackedApps = new List<TrackedApp>();
        public static string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AutoLaunchApp";
        private static readonly string jsonFile = Path.Combine(appDataFolder, "TrackedList.json");

        public static List<TrackedApp> TrackedApps
        {
            get { return trackedApps; }
            set { trackedApps = value; }
        }

        public static void CheckIfJsonExist()
        {
            // Check if AppData app folder exist
            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            // Check if json file config exist
            if (!File.Exists(jsonFile))
                File.Create(jsonFile);

            // Check if Json file is empty
            if (File.ReadAllText(jsonFile) == "")
                File.WriteAllText(jsonFile, "[]");
        }

        /// <summary>
        /// Add one trackedApp to list
        /// </summary>
        /// <param name="_trackedApp"></param>
        public static void AddTrackedApp(TrackedApp _trackedApp)
        {
            trackedApps.Add(_trackedApp);
            Save();
        }

        /// <summary>
        /// Remove one app to trackedApp
        /// </summary>
        /// <param name="_trackedApp"></param>
        /// <returns></returns>
        public static void RemoveTrackedApp(TrackedApp _trackedApp)
        {
            trackedApps.Remove(_trackedApp);
            Save();
        }

        /// <summary>
        /// Get object and save to Json file
        /// </summary>
        public static void Save()
        {
            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(TrackedApps));
        }

        /// <summary>
        /// Load Json File and convert to object
        /// </summary>
        public static List<TrackedApp> Load()
        {
            if (File.Exists(jsonFile))
            {
                var json = File.ReadAllText(jsonFile);

                trackedApps = JsonConvert.DeserializeObject<List<TrackedApp>>(json);
                return trackedApps;
            }
            else return null;
        }
    }
}
