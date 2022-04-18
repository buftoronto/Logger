using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoggerUtil
{
       public class FileLoggerBase
    {
        private StreamWriter streamWriter = null;
        private string fileSource;

        protected void InitBase(string logFile)
        {
            FileSource = logFile;

            if (fileSource != null)
            {
                LoggingManager.AddLogger(LogEnvironment.File, (sender, args) =>
                {
                    LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
                });
                LoggingManager.AddStart(LogEnvironment.File, (sender, args) =>
                {
                    LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
                });
                LoggingManager.AddStop(LogEnvironment.File, (sender, args) =>
                {
                    LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
                    LoggingManager.RemoveLoggerAll(LogEnvironment.File);
                    streamWriter.Close();
                    streamWriter.Dispose();
                    FileLogger.LogFileMutex.ReleaseMutex();
                });
            }
        }

        private void LogFileMessageFormat(string msgFormat, LogLevel logLevel, string msg)
        {
            switch (msgFormat)
            {
                case LogMessageFormat.NoneFormat:
                    streamWriter.WriteLine(LogMessageFormat.NoneFormat, msg);
                    break;
                case LogMessageFormat.GeneralFormat:
                    streamWriter.WriteLine(LogMessageFormat.GeneralFormat, logLevel.ToLogString(), msg);
                    break;
                case LogMessageFormat.TimeFormat:
                case LogMessageFormat.DateFormat:
                case LogMessageFormat.DateTimeFormat:
                    streamWriter.WriteLine(msgFormat, DateTime.Now, logLevel.ToLogString(), msg);
                    break;
                default:
                    break;
            }
        }
        public string FileSource
        {
            get { return fileSource; }
            set
            {
                if (streamWriter != null)
                    streamWriter.Dispose();

                fileSource = value;
                if (File.Exists(fileSource))
                {
                    streamWriter = new StreamWriter(fileSource, true);
                }
                else
                {
                    streamWriter = new StreamWriter(fileSource);
                }
            }
        }
    }
    public class FileLogger : FileLoggerBase
    {
        /// <summary>
        /// Intanciate the Mutex in the main program. There can be a better way. 
        /// </summary>
        public static Mutex LogFileMutex;
        public FileLogger(string logFile)
        {
            // Timeout: 10 minutes
            if (!LogFileMutex.WaitOne(600000))
            {
                throw new Exception("Wait for file over 10 minutes");
            }
            base.InitBase(logFile);
        }
    }
}
