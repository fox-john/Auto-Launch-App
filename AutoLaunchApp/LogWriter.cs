using AutoLaunchApp;
using System;
using System.Diagnostics;
using System.IO;


public class LogWriter
{
    public static string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/AutoLaunchApp";
    private static string logFile = Path.Combine(appDataFolder, "debug.log");


    public LogWriter(LogType _type, string logMessage)
    {
        WriteLog(_type.ToString() + " : " + logMessage);
    }

    public static void CheckIfLogFileExist()
    {
        if (!File.Exists(logFile))
        {
            File.Create(logFile);
        }
    }

    public void WriteLog(string _message)
    {
        try
        {
            if(Config.activeLogs)
            {
                using (StreamWriter stream = File.AppendText(logFile))
                {
                    Log(_message, stream);
                }
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine("ERROR WRITE LOG");
        }
    }

    public void Log(string _message, TextWriter _txtWriter)
    {
        try
        {
            _txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            _txtWriter.WriteLine(_message);
            _txtWriter.WriteLine("-------------------------------");
        }
        catch {}
    }

    public enum LogType
    {
        INFORMATION = 1,
        ERROR = 2,
        WARNING = 3,
        CRITICAL = 4
    }
}