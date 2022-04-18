using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerUtil
{
   public static class LoggerHelper
    {
        /// <summary>
        /// Log errors if <paramref name="errList"/> has any error record; otherwise success
        /// </summary>
        /// <typeparam name="T">Type of the element of <paramref name="errfList"/>.</typeparam>
        /// <param name="errList">List of errors.</param>
        /// <param name="errMsg">The error message if <paramref name="errList"/> contains records.</param>
        /// <param name="goodMsg">Success message if <paramref name="errList"/> is empty.</param>
        /// <param name="needThrow">If true, throw exception; otherwise just log error message.</param>
        public static void LogMessagesIfAnyError<T>(IList<T> errList, string errMsg, string goodMsg, bool needThrow = true)
        {
            LogMessage(errList.Any(), errMsg, ListToString(errList), goodMsg, needThrow);
        }

        /// <summary>
        /// Error if <paramref name="recList"/> has no record; otherwise success.
        /// </summary>
        /// <typeparam name="T">Type of the element of <paramref name="refList"/>.</typeparam>
        /// <param name="recList">The list to be processed.</param>
        /// <param name="errMsg">The error message if <paramref name="recList"/> contains no record.</param>
        /// <param name="goodMsg">The successful message if <paramref name="recList"/> contains records.</param>
        /// <param name="needThrow">If true, throw exception; otherwise just log error message.</param>
        public static void LogMessagesIfNoneError<T>(IList<T> recList, string errMsg, string goodMsg, bool needThrow = true)
        {
            LogMessage(!recList.Any(), errMsg, string.Empty, goodMsg, needThrow);
        }

        public static void LogMessagesIfNullError<T>(T item, string errMsg, string goodMsg, bool needThrow = true)
        {
            LogMessage(item == null, errMsg, string.Empty, goodMsg, needThrow);
        }

        /// <summary>
        /// Log error message or information based on <paramref name="isError"/>
        /// </summary>
        /// <param name="isError">true, error; false, message</param>
        /// <param name="errTitle">Error title</param>
        /// <param name="errDetail">Error details</param>
        /// <param name="goodTitle">Loggin message if not error.</param>
        /// <param name="needThrow">If true, throw exception; otherwise just log error message.</param>
        public static void LogMessage(bool isError, string errTitle, string errDetail, string goodTitle, bool needThrow = true)
        {
            if (isError)
            {
                LoggerHelper.LogErrors(errTitle, errDetail, needThrow);
            }
            else
            {
                LoggingManager.AddMessage(LogLevel.Info, goodTitle);
            }
        }

        /// <summary>
        /// Log errors
        /// </summary>
        /// <param name="msg">Error name</param>
        /// <param name="errDetail">Error details</param>
        /// <param name="needThrow">true means stopping the program and throwing an exception</param>
        public static void LogErrors(string msg, string errDetail, bool needThrow = true)
        {
            LoggingManager.AddMessage(LogLevel.Error, msg);
            LoggingManager.AddMessage(LogLevel.Error, errDetail);

            if (needThrow)
            {
                throw new ApplicationException(msg);
            }
        }

        private static string ListToString<T>(IList<T> list)
        {
            StringBuilder output = new StringBuilder();
            list.ToList().ForEach(
                    e => output.AppendFormat("{0}{1}", e.ToString(), Environment.NewLine)
                );

            return output.ToString();
        }
    }
}
