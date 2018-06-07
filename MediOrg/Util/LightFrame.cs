using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace Ops.NetCoe.LightFrame {

    /// <summary>
    /// Action result
    /// </summary>
    public enum ActionResult {
        Success = 0,
        Warnings = 1,
        Failed = 2
    }

    #region Logging

    /// <summary>
    /// Simple logging
    /// </summary>
    public interface ILog {
        /// <summary>
        /// Logs the technical error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="x">The exception.</param>
        /// <returns>Log id or reference number if available</returns>
        string LogTechError(string message, Exception x);

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        string LogTrace(string message);

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        string LogWarning(string message);
    }

    /// <summary>
    /// Logs to the VS Diagnostic stream
    /// </summary>
    public class VSLog : ILog {
        private int _cnt;
        /// <summary>
        /// Logs the technical error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="x">The exception.</param>
        /// <returns>Log id or reference number if available</returns>
        public string LogTechError(string message, Exception x) {
            string result= (++_cnt).ToString();
            System.Diagnostics.Debug.WriteLine(string.Format(":: #{2} :>> {0}\r\n{1}", message, x.GetDetails(), result), "Error");
            return result;
        }


        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogTrace(string message) {
            string result = (++_cnt).ToString();
            System.Diagnostics.Debug.WriteLine(string.Format(":: #{1} :>> {0}", message, "Trace"));
            return result;
        }

        /// <summary>
        /// Logs the Warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogWarning(string message) {
            string result = (++_cnt).ToString();
            System.Diagnostics.Debug.WriteLine(string.Format(":: #{1} :>> {0}", message, "Warning"));
            return result;
        }
    
    }


    /// <summary>
    /// Logging with Windows Event Log
    /// </summary>
    public class WinLog : ILog {
        const string cnGenLog = "Application";
        //public event NameValueCollector CollectData;
        private string _logName;
        private string _actSource;
        private int _cnt;


        /// <summary>
        /// Initializes a new instance of the <see cref="WinLog"/> class.
        /// </summary>
        /// <param name="logName">Name of the log in the Windows Event Log.</param>
        public WinLog(string logName) {
            this._logName = this._actSource = logName;
        }

        int NextId {
            get { return ++_cnt; }
        }

        void LogEntry(EventLogEntryType entryType, string message) {
            try {
                System.Diagnostics.EventLog.WriteEntry(this._actSource, message, entryType, 1);
            }
            catch (Exception x) {
                if (cnGenLog != this._actSource) {
                    System.Diagnostics.Trace.WriteLine(string.Format("Failed to write to EventLog @source='{0}': {2}.\r\n\tSwitching to EventLog @source='{1}'", this._actSource, cnGenLog, x.Message));
                    string tmp = this._actSource;
                    this._actSource = cnGenLog;
                    LogEntry(EventLogEntryType.Error, string.Format(
                        "{0} [{1}] failed to write to EventLog(@source='{2}'): {4}.\r\nSwitched to @source='{3}'\r\nUse the following command to create the source:\r\n" +
                        "eventcreate /SO \"{2}\" /T SUCCESS /id 1 /L Application /D \"Created Event Log Source\"",
                        AppContext.Current.ApplicationTitle,
                        this.GetType().Assembly.CodeBase,
                        tmp, this._actSource,
                        x.Message
                        ));
                    LogEntry(entryType, message);
                }
                else {
                    System.Diagnostics.Trace.WriteLine(string.Format("Failed to write to EventLog: {0}:> {1}", entryType, message));
                }
            }
        }

        /// <summary>
        /// Logs the specified exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="x">The exception to log.</param>
        /// <returns>
        /// Log Id - identification information for the user and support reference if log operation succeeded; otherwise <c>null</c> or <see cref="F:System.String.Empty"/>.
        /// </returns>
        public string LogTechError(string message, Exception x) {
            string result = NextId.ToString();
            LogEntry(EventLogEntryType.Error, string.Format("Technical Error #{0}\r\n\r\n{1}\r\n\r\nMessage: {2}\r\n\r\nDetails: {3}",
                    result,
                    message,
                    (x == null) ? "n/a" : x.Message,
                    x.GetDetails()));
            return result;
        }


        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogTrace(string message) {
            string result = NextId.ToString();
            LogEntry(EventLogEntryType.Information, string.Format("Trace #{0}: {1}", result, message));
            return result;
        }

        /// <summary>
        /// Logs the Warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string LogWarning(string message) {
            string result = NextId.ToString();
            LogEntry(EventLogEntryType.Warning, string.Format("Warning #{0}: {1}", result, message));
            return result;
        }

    }

    #endregion

    #region Service Locator

    /// <summary>
    /// Simple service locator contract
    /// </summary>
    public interface IServiceLocator {
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="Tservice">The type of the service.</typeparam>
        /// <returns></returns>
        Tservice GetService<Tservice>() where Tservice : class;
    }

    /// <summary>
    /// Simple Service Locator
    /// </summary>
    public class ServiceLocator : IServiceLocator {
        private readonly Dictionary<Type, Func<object>> Container = new Dictionary<Type, Func<object>>();

        #region IServiceLocator Members

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="Tservice">The type of the service.</typeparam>
        /// <returns></returns>
        public Tservice GetService<Tservice>() where Tservice : class {
            Func<object> ctor;
            if (Container.TryGetValue(typeof(Tservice), out ctor)) {
                return (Tservice)ctor();
            }
            throw new ArgumentException("Service Locator is not initialized for type " + typeof(Tservice).FullName);
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <typeparam name="Tservice">The type of the service.</typeparam>
        /// <param name="serviceInstance">The service instance.</param>
        public void RegisterInstance<Tservice>(Tservice serviceInstance) {
            Container[typeof(Tservice)] = delegate() { return serviceInstance; };
        }

        /// <summary>
        /// Registers the instance.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="instance">The instance.</param>
        public void RegisterInstance(Type targetType, object instance) {
            Container[targetType] = delegate() { return instance; };
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <typeparam name="Ttarget">The type of the target.</typeparam>
        /// <param name="factory">The type factory method that builds instance of the target type.</param>
        public void RegisterType<Ttarget>(Func<object> factory) {
            Container[typeof(Ttarget)] = factory;
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <typeparam name="Ttarget">The type of the target.</typeparam>
        /// <typeparam name="Timplementation">The type of the implementation.</typeparam>
        public void RegisterType<Ttarget, Timplementation>() where Timplementation : Ttarget, new() {
            Container[typeof(Ttarget)] = ()=>new Timplementation();
        }

        /// <summary>
        /// Registers the type that simultaneously defines contract and implementation.
        /// </summary>
        /// <typeparam name="Ttargetimpl">The type of the target and implementation.</typeparam>
        public void RegisterType<Ttargetimpl>() where Ttargetimpl : class, new() {
            Container[typeof(Ttargetimpl)] = () => new Ttargetimpl();
        }


        /// <summary>
        /// Registers the singleton. Singleton pattern is implemented by the <see cref="ServiceLocatior"/> itself
        /// </summary>
        /// <typeparam name="Ttarget">The type of the target.</typeparam>
        /// <param name="factory">The factory method that creates instance of the class.</param>
        public void RegisterSingleton<Ttarget>(Func<object> factory) {
            RegisterType<Ttarget>((new Singleton(factory)).GetInstance);
        }

        /// <summary>
        /// Registers the singleton. Singleton pattern is implemented by the <see cref="ServiceLocatior"/> itself
        /// </summary>
        /// <typeparam name="Ttarget">The type of the target.</typeparam>
        public void RegisterSingleton<Ttarget>() where Ttarget : new() {
            RegisterType<Ttarget>((new Singleton(()=>new Ttarget())).GetInstance);
        }

        /// <summary>
        /// Registers the singleton. Singleton pattern is implemented by the <see cref="ServiceLocatior"/> itself
        /// </summary>
        /// <typeparam name="Ttarget">The type of the target.</typeparam>
        /// <typeparam name="Timplementation">The type of the implementation.</typeparam>
        public void RegisterSingleton<Ttarget, Timplementation>() where Timplementation : Ttarget, new() {
            RegisterType<Ttarget>((new Singleton(()=>new Timplementation())).GetInstance);
        }
        #endregion

        class Singleton {
            private object _instance;
            private Func<object> _factory;

            /// <summary>
            /// Initializes a new instance of the <see cref="Singleton"/> class.
            /// </summary>
            /// <param name="factory">The factory.</param>
            public Singleton(Func<object> factory) {
                this._factory= factory;
            }

            /// <summary>
            /// Gets the instance.
            /// </summary>
            /// <returns></returns>
            public object GetInstance() {
                if (_instance == null) {
                    _instance = _factory();
                }
                return _instance;
            }
        }
    }


    #endregion

    #region Cryptography
    /// <summary>
    /// Cryptography helper
    /// </summary>
    public interface ICrypto {
        /// <summary>
        /// Encrypts the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        byte[] Encrypt(byte[] source, byte[] key, byte[] iv);
        /// <summary>
        /// Decrypts the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv.</param>
        /// <returns></returns>
        byte[] Decrypt(byte[] source, byte[] key, byte[] iv);
        /// <summary>
        /// Calculates the hash.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        byte[] CalculateHash(string source);
    }

    /// <summary>
    /// ICrypto extension methods
    /// </summary>
    public static class CryptoExtensions {
        static byte[] CalculateHash(string source) {
            SHA256 shaM = new SHA256Managed();
            byte[] result = shaM.ComputeHash(Encoding.UTF8.GetBytes(source));
            return result;
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="crypto">The crypto.</param>
        /// <param name="source">The source. If <c>null</c> or empty then returned as is.</param>
        /// <param name="password">The password. If <c>null</c> or empty then <paramref name="source"/> is returned as is.</param>
        /// <param name="salt">The salt. If <c>null</c> or empty then <paramref name="password"/> is used as salt.</param>
        /// <returns></returns>
        public static string EncryptString(this ICrypto crypto, string source, string password, string salt = null) {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(password)) return source;
            if (string.IsNullOrEmpty(salt)) salt = password;

            byte[] key = CalculateHash(password);
            byte[] saltBytes = CalculateHash(salt);
            byte[] iv = new byte[16];
            for (int i = 0; i < iv.Length; i++) iv[i] = saltBytes[i];
            byte[] srcBytes = Encoding.UTF8.GetBytes(source);
            byte[] resultBytes = crypto.Encrypt(srcBytes, key, iv);
            string result = Convert.ToBase64String(resultBytes);
            return result;
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="crypto">The crypto.</param>
        /// <param name="source">The source. If <c>null</c> or empty then returned as is.</param>
        /// <param name="password">The password. If <c>null</c> or empty then <paramref name="source"/> is returned as is.</param>
        /// <param name="salt">The salt. If <c>null</c> or empty then <paramref name="password"/> is used as salt.</param>
        /// <returns></returns>
        public static string DecryptString(this ICrypto crypto, string source, string password, string salt = null) {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(password)) return source;
            if (string.IsNullOrEmpty(salt)) salt = password;

            byte[] key = CalculateHash(password);
            byte[] saltBytes = CalculateHash(salt);
            byte[] iv = new byte[16];
            for (int i = 0; i < iv.Length; i++) iv[i] = saltBytes[i];
            byte[] srcBytes = Convert.FromBase64String(source);
            byte[] resultBytes = crypto.Decrypt(srcBytes, key, iv);
            string result = Encoding.UTF8.GetString(resultBytes);
            return result;
        }

    }

    #endregion

    #region Context

    /// <summary>
    /// Application Context Contract
    /// </summary>
    public interface IAppContext {
        /// <summary>
        /// Gets the application title.
        /// </summary>
        /// <value>The application title.</value>
        string ApplicationTitle { get; }

        /// <summary>
        /// Gets the service locator.
        /// </summary>
        /// <value>The service locator.</value>
        IServiceLocator ServiceLocator { get; }
    }

    /// <summary>
    /// Application Context
    /// </summary>
    public class AppContext : IAppContext {
        #region Private Members
        ///<summary>Service Locator</summary>
        private IServiceLocator _serviceLocator;
        ///<summary>Application Title</summary>
        private string _applicationTitle = "X34";

        static IAppContext _current;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="AppContext"/> class.
        /// </summary>
        public AppContext() {
        }

        /// <summary>
        /// Creates the default.
        /// </summary>
        /// <param name="appTitle">The app title.</param>
        /// <returns></returns>
        public static IAppContext CreateDefault(string appTitle) {
            ServiceLocator locator = new ServiceLocator();
            locator.RegisterInstance<ILog>(new VSLog());
            AppContext result = new AppContext() {
                 ApplicationTitle= appTitle??"X34",
                 ServiceLocator = locator
            };
            return result;
        }

        /// <summary>Creates the default.</summary>
        /// <param name="appTitle">The app title.</param>
        /// <param name="locator">The locator.</param>
        /// <returns></returns>
        public static IAppContext CreateDefault(string appTitle, IServiceLocator locator) {
            AppContext result = new AppContext() {
                ApplicationTitle = appTitle ?? "X34",
                ServiceLocator = locator
            };
            return result;
        }
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current Application Context.
        /// </summary>
        /// <value>The current.</value>
        public static IAppContext Current {
            get { return _current; }
        }

        ///<summary>Application Title</summary>
        public string ApplicationTitle {
            get { return this._applicationTitle; }
            set { this._applicationTitle = value; }
        }

        ///<summary>Service Locator</summary>
        public IServiceLocator ServiceLocator {
            get { return this._serviceLocator; }
            set { this._serviceLocator = value; }
        }
        
        #endregion

        #region Operations

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <param name="newContext">The new context.</param>
        public static void SetContext(IAppContext newContext) {
            _current = newContext;
        }

        #endregion

    }

    #endregion

    #region Xml Serialization
 
    /// <summary>
    /// XML Provider interface
    /// </summary>
    public interface IXmlSource {
        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="password">The password to be used to encrypt sensitive data. Can be <c>null</c>.</param>
        /// <returns></returns>
        XElement GetXml(string password=null);
    }


    /// <summary>
    /// Interface of the object that can be configured via XML
    /// </summary>
    public interface IXmlConfigurable {
        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configSource">The configuration source. Can be <c>null</c>.</param>
        /// <param name="password">The password to be used to decrypt sensitive data. Can be <c>null</c>.</param>
        void ApplyConfiguration(XElement configSource, string password= null);
    }


    #endregion

    #region Exceptions
    /// <summary>
    /// Business Exception is used to signal Busines Logic violation, e.g. Validation errors.
    /// </summary>
    public class BizException : ApplicationException {
        #region Private Members
        ///<summary>Backing field for Additional Information</summary>
        private object additionalInformation = null;
        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="BizException"/> class.
        /// </summary>
        public BizException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BizException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public BizException(string errorMessage) : base(errorMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BizException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="additionalInformation">The additional information, e.g. Validation Result(s).</param>
        public BizException(string errorMessage, object additionalInformation)
            : base(errorMessage) {
            this.additionalInformation = additionalInformation;
        }


        #endregion

        #region Properties

        ///<summary>Get or Set Additional Information</summary>
        public virtual object AdditionalInformation {
            get { return this.additionalInformation; }
            set { this.additionalInformation = value; }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        public override string ToString() {
            return (this.AdditionalInformation == null) ? this.Message :
                string.Format("{0}\r\n{1}", this.Message, this.AdditionalInformation.ToString());
        }

        #endregion

    }

    /// <summary>
    /// Technical exception
    /// </summary>
    public class TechnicalException : Exception {
        ///<summary>Log Id</summary>
        private string _logId;
        ///<summary>User Message</summary>
        private string _userMessage;

        ///<summary>User Message</summary>
        public string UserMessage {
            get { return this._userMessage; }
            set { this._userMessage = value; }
        }

        /// <summary>Gets the user message with log ref no if available.</summary>
        /// <value>The user message log.</value>
        public string UserMessageL {
            get {
                return this.IsLogged ? string.Format("{0}. Log Ref# {1}", this._userMessage, this.LogId) : this.UserMessage;
            }
        }

        ///<summary>Log Id</summary>
        public string LogId {
            get { return this._logId; }
            set { this._logId = value; }
        }


        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="TechnicalException"/> class.
        /// </summary>
        public TechnicalException() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechnicalException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message.</param>
        public TechnicalException(string userMessage)
            : base(userMessage) {
            this._userMessage = userMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TechnicalException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TechnicalException(string userMessage, Exception innerException)
            : base(userMessage, innerException) {
            this._userMessage = userMessage;
        }

        #endregion


        /// <summary>Gets a value indicating whether this instance is logged.</summary>
        /// <value><c>true</c> if this instance is logged; otherwise, <c>false</c>.</value>
        public bool IsLogged {
            get { return (!string.IsNullOrEmpty(this.LogId)); }
        }

        /// <summary>Gets the user message.</summary>
        /// <value>The message.</value>
        public override string Message {
            get {
                return this.UserMessageL;
            }
        }

        /// <summary>Gets the log title.</summary>
        /// <value>The log title.</value>
        public virtual string LogTitle {
            get {
                if (this.InnerException != null) {
                    var xx = this.InnerException;
                    return string.Format("Internal error {0}: {1}", xx.GetType().Name, xx.Message);
                }
                else {
                    return this.UserMessage;
                }
            }
        }
    }

    #endregion

    /////////////////////////////////////////////////////////////////////////
    #region Standard Extensions

    /// <summary>
    /// Helper class for StringBuilder extension
    /// </summary>
    public class StringDivider {
        private string _divider = ", ";
        /// <summary>
        /// Gets or sets a value indicating whether [need divider].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [need divider]; otherwise, <c>false</c>.
        /// </value>
        public bool NeedDivider { get; set; }
        /// <summary>
        /// Gets or sets the divider.
        /// </summary>
        /// <value>
        /// The divider.
        /// </value>
        public string Divider {
            get { return this._divider; }
            set { this._divider = value; }
        }

    }

    /// <summary>
    /// Standard Extensions
    /// </summary>
    public static class StdExtensions {

        /// <summary>
        /// Gets the exception details including inner exceptions and stack traces.
        /// </summary>
        /// <param name="x">The exception.</param>
        /// <returns></returns>
        public static string GetDetails(this Exception x) {
            if (x == null) return String.Empty;
            bool isRoot = true;
            StringBuilder sb= new StringBuilder(512);
            foreach (var ex in EnumerateReverse(x)) {
                if (isRoot) {
                    sb.Append(">>> Root reason: ");
                    isRoot = false;
                }
                else {
                    sb.AppendLine("========");
                    sb.Append(", that triggered the following: ");
                }
                sb.AppendLine(ex.GetType().FullName);
                sb.AppendLine(ex.Message).AppendLine();
                if (ex.Data.Count > 0) {
                    sb.AppendLine();
                    sb.AppendLine("Additional details:");
                    foreach (var ky in ex.Data.Keys) {
                        sb.AppendFormat("{0}: {1}", ky, ex.Data[ky]).AppendLine();
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("--- Stack:");
                sb.AppendLine(ex.StackTrace);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Tries to convert this exception to the <typeparamref name="Texc"/>. If this is <see cref="AggregateException"/> 
        /// then tries the first from the inner exceptions.
        /// </summary>
        /// <typeparam name="Texc">The type of the exc.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Texc TryConvertTo<Texc>(this Exception source) where Texc : Exception {
            Texc rz = source as Texc;
            if (rz != null) return rz;
            AggregateException agx = source as AggregateException;
            if ((agx != null) && (agx.InnerExceptions.Count > 0)) {
                rz = agx.InnerExceptions[0] as Texc;
            }
            return rz;
        }

        /// <summary>
        /// Gets the first aggregated exception.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Exception GetFirstAggregate(this AggregateException source) {
            if ((source != null) && (source.InnerExceptions.Count > 0))
                return source.InnerExceptions[0];
            return null;
        }


        static IEnumerable<Exception> EnumerateReverse(Exception x) {
            if (x != null) {
                List<Exception> tmp = new List<Exception>();
                while (x != null) {
                    tmp.Add(x);
                    x = x.InnerException;
                }
                for (int i = tmp.Count - 1; i >= 0; i--)
                    yield return tmp[i];
            }
        }

        /// <summary>
        /// To the short string.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public static string ToShortMsg(this Exception x, string operationName="") {
            return string.Format("Operation {0} failed. {1}: {2}", operationName, x.GetType().Name, x.Message);
        }

        /// <summary>
        /// To the medium message (recursive but no stack).
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns></returns>
        public static string ToMediumMsg(this Exception x, string operationName = "") {
            StringBuilder sb = new StringBuilder(128);
            sb.AppendFormat("Operation {0} failed.", operationName).AppendLine();
            foreach (var e in EnumerateReverse(x)) {
                sb.AppendFormat("--- {0}: {1}", e.GetType().Name, e.Message).AppendLine();
                if (x.Data.Count > 0) {
                    foreach (var ky in x.Data.Keys) {
                        sb.AppendFormat("\t{0}: {1}", ky, x.Data[ky]).AppendLine();
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Gets the service via locator.
        /// </summary>
        /// <typeparam name="Tservice">The type of the service.</typeparam>
        /// <param name="appContext">The app context.</param>
        /// <returns></returns>
        public static Tservice GetServiceViaLocator<Tservice>(this IAppContext appContext) where Tservice : class {
            if (appContext.ServiceLocator != null) {
                return appContext.ServiceLocator.GetService<Tservice>();
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Logs the technical error.
        /// </summary>
        /// <param name="appContext">The application context.</param>
        /// <param name="message">The additional message.</param>
        /// <param name="x">The exception.</param>
        /// <returns></returns>
        public static string LogTechError(this IAppContext appContext, string message, Exception x) {
            ILog log = appContext.GetServiceViaLocator<ILog>();
            return log.LogTechError(message, x);
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="appContext">The app context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string LogTrace(this IAppContext appContext, string message) {
            ILog log = appContext.GetServiceViaLocator<ILog>();
            return log.LogTrace(message);
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="appContext">The app context.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static string LogWarning(this IAppContext appContext, string message) {
            ILog log = appContext.GetServiceViaLocator<ILog>();
            return log.LogWarning(message);
        }

        /// <summary>
        /// Logs the tech error.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="x">The Exception.</param>
        /// <param name="format">The format.</param>
        /// <param name="prms">The parameters to inject into the format string.</param>
        /// <returns></returns>
        public static string LogTechError(this ILog log, Exception x, string format, params object[] prms) {
            return log.LogTechError(string.Format(format, prms), x);
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="format">The format.</param>
        /// <param name="prms">The PRMS.</param>
        /// <returns></returns>
        public static string LogTrace(this ILog log, string format, params object[] prms) {
            return log.LogTrace(string.Format(format, prms));
        }

        /// <summary>
        /// Logs the Warning.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="format">The format.</param>
        /// <param name="prms">The parameters.</param>
        /// <returns></returns>
        public static string LogWarning(this ILog log, string format, params object[] prms) {
            return log.LogWarning(string.Format(format, prms));
        }


        ///// <summary>
        ///// Splits string into the two trimmed substrings on the first encounter of one of the <paramref name="separators"/>.
        ///// </summary>
        ///// <param name="source">The source. Can be <value>null</value> or <see cref="String.Empty"/>.</param>
        ///// <param name="separators">The separators.</param>
        ///// <param name="first">The first substring. Returns <value>null</value> if <paramref name="source"/> is null
        ///// and <see cref="String.Empty"/> if source is empty.</param>
        ///// <param name="second">The second substring. Returns <value>null</value> if <paramref name="source"/> is null
        ///// and <see cref="String.Empty"/> if source is empty or separator no found.</param>
        //public static void SplitInTwoTrimmed(this string source, char[] separators, out string first, out string second) {
        //    if (source == null) {
        //        first = second = null;
        //        return;
        //    }
        //    if (source == string.Empty) {
        //        first = second = string.Empty;
        //        return;
        //    }
        //    int ix = source.IndexOfAny(separators);
        //    if (ix < 0) {
        //        first = source.Trim();
        //        second = string.Empty;
        //    }
        //    else {
        //        first = source.Substring(0, ix).Trim();
        //        second = source.Substring(ix + 1).Trim();
        //    }
        //}

        /// <summary>
        /// Safely get the left part of the string with length no more than <paramref name="maxLength"/>.
        /// Null-aware.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="maxLength">Max length of the result.</param>
        /// <returns>Left part of the source string with length no more than <paramref name="maxLength"/> or <c>null</c> if source is <c>null</c>.</returns>
        public static string GetLeft(this string source, int maxLength) {
            if (string.IsNullOrEmpty(source)) return source;
            return source.Substring(0, Math.Min(source.Length, maxLength));
        }

        /// <summary>
        /// Safely get the right part of the string with length no more than <paramref name="maxLength"/>.
        /// Null-aware.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="maxLength">Max length of the result.</param>
        /// <returns>Right part of the source string with length no more than <paramref name="maxLength"/> or <c>null</c> if source is <c>null</c>.</returns>
        public static string GetRight(this string source, int maxLength) {
            if (string.IsNullOrEmpty(source)||(maxLength>=source.Length)) return source;
            return source.Substring(source.Length-maxLength);
        }

        /// <summary>
        /// To the human-friendly string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static string ToHumanString(this TimeSpan source) {
            if (source.Hours > 0)
                return source.ToString(@"HH\:mm\:ss");
            else
                return source.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Convert string to <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TEnum ToEnum<TEnum>(this string source, TEnum defaultValue) where TEnum : struct {
            if (string.IsNullOrEmpty(source)) return defaultValue;
            TEnum rz= defaultValue;;
            if (!Enum.TryParse<TEnum>(source, out rz)) {
                //int t;
                //if (int.TryParse(source, out t)) {
                //    rz = Enum. (TEnum)t;
                //}
                return defaultValue;
            }
            return rz;
        }

        /// <summary>
        /// Appends to the specified target using divider if necessary.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="divider">The divider.</param>
        /// <param name="toAdd">To add.</param>
        /// <returns></returns>
        public static StringBuilder Append(this StringBuilder target, StringDivider divider, object toAdd) {
            if (divider.NeedDivider)
                target.Append(divider.Divider);
            else
                divider.NeedDivider = true;
            target.Append(toAdd);
            return target;
        }

        #region Collections

        /// <summary>
        /// Applies the specified method to the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void Apply<T>(this IEnumerable<T> source, Action<T> action) {
            if (source != null) {
                foreach (T itm in source)
                    action(itm);
            }
        }

        /// <summary>
        /// Adds the items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="item1">The item1.</param>
        /// <param name="items">The items.</param>
        public static void AddItems<T>(this ICollection<T> target, T item1, params T[] items) {
            target.Add(item1);
            foreach (var itm in items)
                target.Add(itm);
        }

        /// <summary>
        /// Adds the items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        public static void AddItems<T>(this ICollection<T> target, IEnumerable<T> items) {
            foreach (var itm in items)
                target.Add(itm);
        }


        /// <summary>
        /// Get the first element from the enumeration.
        /// Returns <c>defaultValue</c> if it is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static T FirstElement<T>(this IEnumerable<T> source, T defaultValue) {
            if (source != null) {
                foreach (var itm in source)
                    return itm;
            }
            return defaultValue;
        }

        /// <summary>
        /// Finds index of the first element that matches the filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static int FirstIndex<T>(this IEnumerable<T> source, Func<T, bool> filter) {
            int ix = -1;
            if (source != null) {
                foreach (var itm in source) {
                    ix++;
                    if (filter(itm)) return ix;
                }
            }
            return ix;
        }

        /// <summary>Replaces the items in the the source using match predicate and builder methods.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate to filter items that have to be changed.</param>
        /// <param name="builder">The builder to create new items to substitute the existing item.</param>
        /// <returns></returns>
        public static int Replace<T>(this IList<T> source, Func<T, bool> predicate, Func<T, T> builder) {
            int cnt = 0;
            if (source != null) {
                for (int i = 0; i < source.Count; i++) {
                    if (predicate(source[i])) {
                        source[i] = builder(source[i]);
                        cnt++;
                    }
                }
            }
            return cnt;
        }

        /// <summary>
        /// Removes the matching.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        public static void RemoveMatching<T>(this IList<T> source, Func<T, bool> predicate) {
            if (source != null) {
                int ix = -1;
                for (int i = 0; i < source.Count; i++) {
                    if (!predicate(source[i])) {
                        ix++;
                        if (ix != i) {
                            source[ix] = source[i];
                        }
                    }
                }
                if (ix < 0) {
                    source.Clear();
                }
                else {
                    for (int i = source.Count - 1; i > ix; i--) {
                        source.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates non-null elements casted to the <paramref name="Tcast"/>.
        /// </summary>
        /// <typeparam name="Tcast">The type of the cast.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="filter">The filter. Can be <c>null</c>.</param>
        /// <returns></returns>
        public static IEnumerable<Tcast> EnumerateAs<Tcast>(this IEnumerable source, Func<Tcast, bool> filter=null) where Tcast : class {
            Tcast tmp;
            if (source != null) {
                foreach (var it in source) {
                    tmp = it as Tcast;
                    if (tmp != null) {
                        if (filter == null)
                            yield return tmp;
                        else {
                            if (filter(tmp))
                                yield return tmp;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Safely gets the value from the dictionary.
        /// If source is <c>null</c> or <paramref name="key"/> is not found then <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="Tkey">The type of the key.</typeparam>
        /// <typeparam name="Tval">The type of the val.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static Tval GetValueSafe<Tkey, Tval>(this IDictionary<Tkey, Tval> source, Tkey key, Tval defaultValue = null) where Tval : class {
            if (source == null) return defaultValue;
            Tval rz;
            if (!source.TryGetValue(key, out rz))
                return defaultValue;
            return rz;
        }

        /// <summary>
        /// Traverses the hierarchy in depth.
        /// </summary>
        /// <typeparam name="Titem">The type of the item.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="getChildren">The get children delegate. Can be <c>null</c>.</param>
        /// <returns></returns>
        public static IEnumerable<Titem> TraverseDeep<Titem>(this IEnumerable<Titem> source, Func<Titem, IEnumerable<Titem>> getChildren) {
            if (source != null) {
                foreach (var it in source) {
                    yield return it;
                    if (getChildren != null) {
                        foreach (var subIt in getChildren(it).TraverseDeep(getChildren)) {
                            yield return subIt;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find appropriate index for <paramref name="item"/> using Binary Search. Source list must be not null and ordered using
        /// <paramref name="comparer"/> method.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="pos">The result index.</param>
        /// <param name="ix1">The start index of the range. If not specified: <c>0</c></param>
        /// <param name="ixN">The last index of the range. If not specified: <c>source.Count-1</c></param>
        /// <returns>True if equal element exists at <paramref name="pos"/>.</returns>
        public static bool BinaryFindIndex<T>(this IList<T> source, T item, Comparison<T> comparer, out int pos, int ix1 = 0, int ixN = int.MinValue) {
            int L = ix1, R = (ixN == int.MinValue) ? source.Count - 1 : ixN, M = ix1, cr;
            while (L < R) {
                M = (L + R) >> 1;
                if ((cr = comparer(item, source[M])) == 0) {
                    pos = M;
                    return true;
                }
                else if (cr < 0) {
                    R = M - 1;
                }
                else {
                    L = M + 1;
                }
            }
            pos = M;
            return false;
        }

        /// <summary>
        /// Find appropriate index using Binary Search. Source list must be not null and ordered using the same rules as
        /// <paramref name="comparer"/> method.
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="comparer">The comparer. Thsi method compares the specified item with some criteria and return int value.</param>
        /// <param name="pos">The result index.</param>
        /// <param name="ix1">The start index of the range. If not specified: <c>0</c></param>
        /// <param name="ixN">The last index of the range. If not specified: <c>source.Count-1</c></param>
        /// <returns>True if equal element exists at <paramref name="pos"/>.</returns>
        public static bool BinaryFindIndex<T>(this IList<T> source, Func<T, int> comparer, out int pos, int ix1 = 0, int ixN = int.MinValue) {
            int L = ix1, R = (ixN == int.MinValue) ? source.Count - 1 : ixN, M = ix1, cr;
            while (L < R) {
                M = (L + R) >> 1;
                if ((cr = comparer(source[M])) == 0) {
                    pos = M;
                    return true;
                }
                else if (cr < 0) {
                    R = M - 1;
                }
                else {
                    L = M + 1;
                }
            }
            pos = M;
            return false;
        }

        #endregion

        #region XML

        /// <summary>
        /// Gets the mandatory sub-element.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="elName">Name of the subelement.</param>
        /// <param name="handleMissing">The handle missing delegate. If null then the error message is logged as an error an null is returned.</param>
        /// <returns></returns>
        public static XElement GetMandatoryElement(this XElement source, XName elName, Action<string> handleMissing=null) {
            XElement result = null;
            if (source != null) {
                result = source.Element(elName);
                if (result == null) {
                    string errMsg = string.Format("Mandatory sub-element '{0}' is missing in the  {1}", elName, source.ToString().GetLeft(80));
                    if (handleMissing == null)
                        AppContext.Current.LogTechError(errMsg, null);
                    else
                        handleMissing(errMsg);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the mandatory attribute.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="handleMissing">The handle missing delegate. If null then the error message is logged as an error and <c>null</c> is returned.</param>
        /// <returns></returns>
        public static string GetMandatoryAttribute(this XElement source, XName attrName, Action<string> handleMissing = null) {
            string result = null;
            XAttribute xa;
            if (source != null) {
                xa = source.Attribute(attrName);
                if (xa == null) {
                    string errMsg = string.Format("Mandatory attribute '{0}' is missing in the {1}", attrName, source.ToString().GetLeft(80));
                    if (handleMissing == null)
                        AppContext.Current.LogTechError(errMsg, null);
                    else
                        handleMissing(errMsg);
                }
                else {
                    result = xa.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the mandatory attribute and cast it to the specified type using <see cref="convert"/> method.
        /// If attribute is missing or conversion failed then <see cref="handleError"/> is used to get value to return or throw the error message.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="handleError">The handle error delegate. If null then <see cref="BizException"/> is thrown. This method must either throw error or return value to use.</param>
        /// <returns></returns>
        public static T GetMandatoryAttribute<T>(this XElement source, XName attrName, Func<XAttribute, T> convert, Func<string, T> handleError=null) {
            T result= default(T);
            XAttribute xa;
            string errMsg;
            if (source != null) {
                xa = source.Attribute(attrName);
                if (xa == null) {
                    errMsg = string.Format("Mandatory attribute '{0}' is missing in the {1}", attrName, source.ToString().GetLeft(80));
                    if (handleError == null) {
                        throw new BizException(errMsg);
                    }
                    else {
                        result= handleError(errMsg);
                    }
                }
                else {
                    try {
                        result = convert(xa);
                    }
                    catch (Exception x) {
                        errMsg= string.Format("Failed to convert mandatory attribute '{0}' to {1} in the {2}", attrName, typeof(T).Name, source.ToString().GetLeft(80));
                        if (handleError == null) {
                            throw new BizException(errMsg, x.Message);
                        }
                        result = handleError(errMsg);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Tries the get attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="apply">The apply delegate. This thing should either return false or throw an exception to signal failure</param>
        /// <returns></returns>
        public static bool TryGetAttribute<T>(this XElement source, XName attrName, Func<XAttribute, bool> apply) {
            XAttribute xa=source.Attribute(attrName);
            if (xa == null) return false;
            try {
                return apply(xa);
            }
            catch {
                return false;
            }
        }


        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attr.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetAttributeValue(this XElement source, XName attrName, string defaultValue= "") {
            if (source == null) return defaultValue;
            XAttribute xa = source.Attribute(attrName);
            if (xa == null) return defaultValue;
            else return xa.Value;
        }

        /// <summary>
        /// Gets the int attribute value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int? GetAttributeInt(this XElement source, XName attrName, int? defaultValue = null) {
            if (source == null) return defaultValue;
            XAttribute xa = source.Attribute(attrName);
            if (xa == null) return defaultValue;
            else return (int)xa;
        }

        /// <summary>
        /// Gets the time span attribute value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="attrName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TimeSpan? GetAttributeTimeSpan(this XElement source, XName attrName, TimeSpan? defaultValue = null) {
            if (source == null) return defaultValue;
            XAttribute xa = source.Attribute(attrName);
            if (xa == null) return defaultValue;
            else return (TimeSpan)xa;
        }

        /// <summary>
        /// Adds the XML items. <c>items</c> can be null. <c>Null</c> entries are skipped.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static XElement AddItems(this XElement target, IEnumerable items) {
            if (items != null) {
                foreach (var it in items) {
                    if (it != null)
                        target.Add(it);
                }
            }
            return target;
        }

        #endregion

        #region Configuration
        /// <summary>
        /// Gets the app configuration item.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetAppCfgItem(this IAppContext ctx, string key, string defaultValue= null) {
            string result = System.Configuration.ConfigurationManager.AppSettings[key];
            if (result == null) {
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// Enumerate appSettings with matching keys
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="keyFilter">Key filter or null to return all</param>
        /// <returns></returns>
        public static IEnumerable<CodeValuePair> EnumerateAppCfgItems(this IAppContext ctx, Func<string, bool> keyFilter=null) {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            foreach (var ky in (from k in appSettings.AllKeys where ((keyFilter == null) || keyFilter(k)) select k)) {
               yield return new CodeValuePair() { Code = ky, Value = appSettings[ky] };
            }
        }

        /// <summary>
        /// Gets the app configuration item.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="ctx">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="converter">The converter. Leave it <c>null</c> to use <see cref="System.Convert"/></param>
        /// <returns></returns>
        public static TItem GetAppCfgItem<TItem>(this IAppContext ctx, string key, TItem defaultValue, Converter<string, TItem> converter=null) {
            string srz = System.Configuration.ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(srz)) return defaultValue;
            TItem rz = defaultValue;
            try {
                if (converter != null) {
                    rz = converter(srz);
                }
                else {
                    rz = (TItem)Convert.ChangeType(srz, typeof(TItem), System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception x) {
                rz = defaultValue;
                ctx.LogWarning(string.Format("Failed to convert AppConfig item '{0}' from '{1}' to {2}: {3}", key, srz, typeof(TItem).FullName, x.Message));
            }
            return rz;
        }

        /// <summary>Gets the application folder.</summary>
        /// <returns></returns>
        public static string GetAppRoot(this IAppContext src) {
            string fileName =  typeof(AppContext).Assembly.CodeBase.Replace("file:///", string.Empty);
            return System.IO.Path.GetDirectoryName(fileName);

        }
        #endregion
    }
    #endregion
}