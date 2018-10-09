using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Core.Utils
{
    /// <summary>
    ///     This is a very simply class to abstract the use of log4net.
    ///     If log4net is included in the project, you simply use this
    ///     class as it is. If you remove the log4net.dll from your
    ///     project. All you have to do is comment out the relevant
    ///     statements in this class - you don't need to change any
    ///     other of your existing code that may be using log4net.
    ///     You can do a simple search and replace /**/ to /**///
    ///     to include or exclude logging...
    /// </summary>
    /// <author>Jashan Chittesh - info@jashan-chittesh.de</author>
    public class LoggerAdapter {
        private const string Startmsg
            = "log4net configuration file:{0}\n\n"
              + "    =======================================\n"
              + "    === Logging configured successfully ===\n"
              + "    =======================================\n";


        /**/private ILog _log;

        /// <summary>
        ///     A logger to be used for logging statements in the code.
        ///     It is recommended to follow a pattern for instantiating this:
        ///     <code>
        /// 
        ///     </code>
        /// </summary>
        /// <param name="type">the type that is using this logger</param>
        public LoggerAdapter(Type type) {
            /**/_log = LogManager.GetLogger(type);
        }

        public LoggerAdapter(string name)
        {
            /**/
            _log = LogManager.GetLogger(name);
        }

        static LoggerAdapter()
        {
            if (File.Exists("log4net.xml"))
            {
                Init("log4net.xml");
            }
        }
        /// <summary>
        ///     This is automatically called before the first instance of
        ///     LoggerAdapter is created, and initializes logging. You can change
        ///     this according to your needs.
        /// </summary>
        public static void Init(string configFile) {

            /**/var fileInfo = new FileInfo(configFile);
            /**/XmlConfigurator.ConfigureAndWatch(fileInfo);
            /**/LogManager.GetLogger(typeof(LoggerAdapter)).InfoFormat(Startmsg, configFile);
        }

        /* Test if a level is enabled for logging */
        public bool IsDebugEnabled {
            get
            {
                /**/
                var result = _log.IsDebugEnabled;
                return result;
            }
        }
        public bool IsInfoEnabled {
            get
            {
                /**/
                var result = _log.IsInfoEnabled;
                return result;
            }
        }
        public bool IsWarnEnabled {
            get
            {
                /**/
                var result = _log.IsWarnEnabled;
                return result;
            }
        }
        public bool IsErrorEnabled {
            get
            {
                /**/
                var result = _log.IsErrorEnabled;
                return result;
            }
        }
        public bool IsFatalEnabled {
            get
            {
                /**/
                var result = _log.IsFatalEnabled;
                return result;
            }
        }

        /* Log a message object */
        public void Debug(object message) {
            /**/_log.Debug(message);
        }
        public void Info(object message) {
            /**/_log.Info(message);
        }
        public void Warn(object message) {
            /**/_log.Warn(message);
        }
        public void Error(object message) {
            /**/_log.Error(message);
        }
        public void Fatal(object message) {
            /**/_log.Fatal(message);
        }

        /* Log a message object and exception */
        public void Debug(object message, Exception t) {
            /**/_log.Debug(message, t);
        }
        public void Info(object message, Exception t) {
            /**/_log.Info(message, t);
        }
        public void Warn(object message, Exception t) {
            /**/_log.Warn(message, t);
        }
        public void Error(object message, Exception t) {
            /**/_log.Error(message, t);
        }
        public void Fatal(object message, Exception t) {
            /**/_log.Fatal(message, t);
        }

        /* Log a message string using the System.String.Format syntax */
        public void DebugFormat(string format, params object[] args) {
            /**/_log.DebugFormat(format, args);
        }
        public void InfoFormat(string format, params object[] args) {
            /**/_log.InfoFormat(format, args);
        }
        public void WarnFormat(string format, params object[] args) {
            /**/_log.WarnFormat(format, args);
        }
        public void ErrorFormat(string format, params object[] args) {
            /**/_log.ErrorFormat(format, args);
        }
        public void FatalFormat(string format, params object[] args) {
            /**/_log.FatalFormat(format, args);
        }

        /* Log a message string using the System.String.Format syntax */
        public void DebugFormat(IFormatProvider provider, string format, params object[] args) {
            /**/_log.DebugFormat(provider, format, args);
        }
        public void InfoFormat(IFormatProvider provider, string format, params object[] args) {
            /**/_log.InfoFormat(provider, format, args);
        }
        public void WarnFormat(IFormatProvider provider, string format, params object[] args) {
            /**/_log.WarnFormat(provider, format, args);
        }
        public void ErrorFormat(IFormatProvider provider, string format, params object[] args) {
            /**/_log.ErrorFormat(provider, format, args);
        }
        public void FatalFormat(IFormatProvider provider, string format, params object[] args) {
            /**/_log.FatalFormat(provider, format, args);
        }
    }
}
