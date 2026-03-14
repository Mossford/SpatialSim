using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace SpatialSim.Engine.Core
{
    public static class Debug
    {
        static bool createdLogFile;
        static string logFile;
        static ulong offset;

        public static void Init()
        {
            createdLogFile = false;
            CreateLogFile();
        }
        
        public static void LogDebug(in string msg)
        {
            if(!AppState.EnableDebugLogging)
                return;
            
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [DEBUG]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.ResetColor();
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    WriteToLog((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }
        }
        
        public static void LogInfo(in string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [INFO]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    WriteToLog((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }
        }
        
        public static void Warning(in string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [WARNING]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    WriteToLog((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }
        }
        
        public static void Error(in string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [ERROR]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    WriteToLog((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }
        }

        static void CreateLogFile()
        {
            if(createdLogFile)
                return;
            
            if (!Directory.Exists(Resources.LogPath))
            {
                Directory.CreateDirectory(Resources.LogPath);
            }
            
            if (!createdLogFile)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string file = Resources.LogPath + date + ".zip";
                if (File.Exists(file))
                {
                    String[] files = Directory.GetFiles(Resources.LogPath, date + "*.zip");
                    file = Resources.LogPath + date + $"({files.Length + 1}).zip";
                    File.Create(Resources.LogPath + "log.txt").Close();
                    logFile = file;
                }
                else
                {
                    File.Create(Resources.LogPath + "log.txt").Close();
                    logFile = file;
                }
            }
            
            createdLogFile = true;

            LogInfo("Time: " + DateTime.Now);
        }

        static void WriteToLog(string msg)
        {
            File.AppendAllText(Resources.LogPath + "log.txt", msg + "\n");
        }

        public static void CompressLog()
        {
            ZipArchive archive = ZipFile.Open(logFile, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(Resources.LogPath + "log.txt", "log.txt");
            archive.Dispose();
        }
    }
}