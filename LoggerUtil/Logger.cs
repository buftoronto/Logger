using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoggerUtil
{
      public enum LogEnvironment
    {
        Console = 0,
        File,
        Event,
        Database,
    }

    public enum LogType
    {
        Normal = 0,
        Verbose,
        None,
    }

    public enum LogLevel
    {
        Diagnostic, // when LogType.Verbose
        Info,
        Warning,
        Error,
        None
    }

    public static class LogLevelExtensions
    {
        public static string ToLogString(this LogLevel l)
        {
            switch (l)
            {
                case LogLevel.Diagnostic:
                case LogLevel.Info:
                case LogLevel.Warning:
                case LogLevel.Error:
                    return l.ToString() + ":";
                case LogLevel.None:
                    return string.Empty;
                default:
                    return "Not a valid format";
            }
        }
    }

    public class LoggerEventArgs: EventArgs
    {
        public string Message { get; private set; }
        public string MessageFormat { get; set; } = LogMessageFormat.TimeFormat;
        public LogLevel Priority { get; private set; }

        public LoggerEventArgs(LogLevel priority, string message, string msgFormat)
        {
            Priority = priority;
            Message = message;
            MessageFormat = msgFormat;
        }
    }

    public sealed class LoggingManager
    {
        private static Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>> LogHandlers 
                    = new Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>>();
        private static Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>> StartHandlers
                    = new Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>>();
        private static Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>> StopHandlers
                    = new Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>>();
        public static LogType LoggerType { get; set; } = LogType.Normal;

        public static string MessageFormat { get; set; } = LogMessageFormat.TimeFormat;

        private LoggingManager()
        {
        }

        public static FileLogger CreateFileLogger(string logFile)
        {
            return new FileLogger(logFile);
        }

        private static void AddEvent(Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>> logHandler, LogEnvironment system, EventHandler<LoggerEventArgs> ev)
        {
            if (logHandler.ContainsKey(system))
            {
                logHandler[system] += ev;
            }
            else
            {
                logHandler.Add(system, ev);
            }
        }
        public static void AddLogger(LogEnvironment system, EventHandler<LoggerEventArgs> ev)
        {
            AddEvent(LogHandlers, system, ev);
        }
        public static void RemoveLogger(LogEnvironment system, EventHandler<LoggerEventArgs> ev)
        {
            if (LogHandlers.ContainsKey(system))
            {
                LogHandlers[system] -= ev;
            }
        }

        public static void RemoveLoggerAll(LogEnvironment system)
        {
            RemoveOneLogger(LogHandlers, system);
            RemoveOneLogger(StartHandlers, system);
            RemoveOneLogger(StopHandlers, system);
        }

        /// <summary>
        /// Unsubscribe the event handlers
        /// </summary>
        /// <param name="handles"></param>
        /// <param name="env"></param>
        public static void RemoveOneLogger(Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>> handles, LogEnvironment env)
        {
            if (handles.ContainsKey(env))
            {
                if (handles[env] != null)
                {
                    foreach (Delegate del in handles[env].GetInvocationList())
                    {
                        handles[env] -= (EventHandler<LoggerEventArgs>)del;
                    }
                }
            }
        }

        public static void AddMessage(LogEnvironment system, LogLevel priority, string msg)
        {
            if ((LoggerType == LogType.None) || (LoggerType == LogType.Normal && priority == LogLevel.Diagnostic))
                return;

            EventHandler<LoggerEventArgs> eh = LogHandlers[system] as EventHandler<LoggerEventArgs>;

            if (priority == LogLevel.None)
                eh?.Invoke(system, new LoggerEventArgs(priority, msg, LogMessageFormat.NoneFormat));
            else
                eh?.Invoke(system, new LoggerEventArgs(priority, msg, MessageFormat));
        }
        public static void AddMessage(LogLevel priority, string msg)
        {
            foreach (var lh in LogHandlers)
            {
                AddMessage(lh.Key, priority, msg);
            }
        }
        public static void AddStart(LogEnvironment system, EventHandler<LoggerEventArgs> ev)
        {
            AddEvent(StartHandlers, system, ev);
        }

        public static void Start(LogLevel priority, string msg)
        {
            foreach (var sh in StartHandlers)
            {
                sh.Value(sh.Key, new LoggerEventArgs(priority, msg, MessageFormat));
            }
        }

        public static void AddStop(LogEnvironment system, EventHandler<LoggerEventArgs> ev)
        {
            AddEvent(StopHandlers, system, ev);
        }
        /// <summary>
        /// Fire the Stop events
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="msg"></param>
        /// <remarks>
        /// Need to use a copy of the event handlers because it may unsubsribe the events in the Stop event.
        /// In that case StopHandlers is null.
        /// </remarks>
        public static void Stop(LogLevel priority, string msg)
        {
            var copyStopHandlers = new Dictionary<LogEnvironment, EventHandler<LoggerEventArgs>>(StopHandlers);
            foreach (var sh in copyStopHandlers)
            {
                if (sh.Value != null)
                {
                    sh.Value(sh.Key, new LoggerEventArgs(priority, msg, MessageFormat));
                }
            }
        }
    }//LogginManager

    public class ConsoleLogger
    {
        public static void Init()
        {
            LoggingManager.AddLogger(LogEnvironment.Console, (sender, args) =>
            {
                Console.Error.WriteLine(LogMessageFormat.GeneralFormat, args.Priority.ToLogString(), args.Message);
            });

            LoggingManager.AddStart(LogEnvironment.Console, (sender, args) =>
            {
                Console.Error.WriteLine("{0} Log starts", args.Priority.ToLogString());
            });
            LoggingManager.AddStop(LogEnvironment.Console, (sender, args) =>
            {
                Console.Error.WriteLine("{0} Log stops.", args.Priority.ToLogString());
            });
        }
    }

    /// <summary>
    /// This is only for unit tests.
    /// </summary>
    public class StringLogger
    {
        private static StringBuilder msg = new StringBuilder();
        public static void Init()
        {
            LoggingManager.AddLogger(LogEnvironment.Console, (sender, args) =>
            {
                msg.Append(string.Format(LogMessageFormat.GeneralFormat, args.Priority.ToLogString(), args.Message));
                msg.Append(Environment.NewLine);
            });
        }

        public static void Reset()
        {
            msg.Clear();
        }

        public static string LogInfo
        {
            get { return msg.ToString(); }
        }
    }

    //public class AppSupportLogger
    //{
    //    public AppSupportLogger()
    //    {
    //        if (_fileSource != null)
    //        {
    //            LoggingManager.AddLogger(LogEnvironment.File, (sender, args) =>
    //            {
    //                LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
    //            });
    //            LoggingManager.AddStart(LogEnvironment.File, (sender, args) =>
    //            {
    //                LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
    //            });
    //            LoggingManager.AddStop(LogEnvironment.File, (sender, args) =>
    //            {
    //                LogFileMessageFormat(args.MessageFormat, args.Priority, args.Message);
    //                _sw.Close();
    //                _sw.Dispose();
    //            });
    //        }
    //    }
    //}


    public class LogMessageFormat
    {
        public const string NoneFormat = "{0}";
        public const string GeneralFormat = "{0} {1}";
        public const string TimeFormat = "{0:H:mm:ss} {1} {2}";
        public const string DateFormat = "{0:MM/dd/yyyy} {1} {2}";
        public const string DateTimeFormat = "{0:MM/dd/yyyy H:mm:ss} {1} {2}";
    }
}
