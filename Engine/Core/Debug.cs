

namespace SpatialSim.Engine.Core
{
    public static class Debug
    {
        static bool createdLogFile;
        static string logFile;

        public static void Init()
        {
            createdLogFile = false;
            CreateLogFile();
        }
        
        public static void LogDebug(string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [DEBUG]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    File.AppendAllLines(logFile, [(i == 0 ? msgType : msgType + indent) + msgs[i]]);
                }
            }
        }
        
        public static void LogInfo(string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [INFO]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    File.AppendAllLines(logFile, [(i == 0 ? msgType : msgType + indent) + msgs[i]]);
                }
            }
        }
        
        public static void Warning(string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [WARNING]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    File.AppendAllLines(logFile, [(i == 0 ? msgType : msgType + indent) + msgs[i]]);
                }
            }
        }
        
        public static void Error(string msg)
        {
            string[] msgs = msg.Split('\n');
            string timeStamp = '[' + DateTime.Now.ToString("HH:mm:ss") + ']';
            string msgType = timeStamp + " [ERROR]: ";
            string indent = new string(' ', msgType.Length);

            if (AppState.EnableConsoleLogging)
            {
                for (int i = 0; i < msgs.Length; i++)
                {
                    Console.WriteLine((i == 0 ? msgType : msgType + indent) + msgs[i]);
                }
            }

            if (AppState.EnableLogging)
            {
                CreateLogFile();
                for (int i = 0; i < msgs.Length; i++)
                {
                    File.AppendAllLines(logFile, [(i == 0 ? msgType : msgType + indent) + msgs[i]]);
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
                string file = Resources.LogPath + date + ".log";
                if (File.Exists(file))
                {
                    String[] files = Directory.GetFiles(Resources.LogPath, date + "*.log");
                    File.Create(Resources.LogPath + date + $"({files.Length + 1}).log").Close();
                    logFile = Resources.LogPath + date + $"({files.Length + 1}).log";
                }
                else
                {
                    File.Create(Resources.LogPath + date + ".log").Close();
                    logFile = file;
                }
            }
            
            createdLogFile = true;

            LogInfo("Time: " + DateTime.Now);
        }
    }
}